using UnityEngine;
using System;

public class GrapplingHookController : MonoBehaviour
{
    #region Events
    public event Action OnGrappleStarted;
    public event Action OnGrappleStopped;
    public event Action<Transform> OnPredictionTargetChanged;
    #endregion

    #region Dependencies
    [Header("Referências")]
    [SerializeField] private PlayerMovementController playerMovement;
    [SerializeField] private Transform grappleTip;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject predictionPointPrefab;
    #endregion

    #region Grapple Settings
    [Header("Configurações do Gancho")]
    [SerializeField] private float maxGrappleDistance = 50f;
    [SerializeField] private float grappleCooldown = 1f;
    [SerializeField] private LayerMask grappleableLayer;
    [SerializeField] private bool canDoMultipleGrapple = false;
    [SerializeField] private float maximumTimeGrappling = 3f;
    [Tooltip("Frequência de verificação da validade do grapple com objetos IGrappleable.")]
    [SerializeField] private float grappleValidityCheckInterval = 0.1f;
    #endregion
    
    #region Cooldown Tolerance
    [Header("Tolerância de Cooldown")]
    [Tooltip("Se um grapple durar menos que este tempo, o cooldown é ignorado (se a tolerância estiver ativa).")]
    [SerializeField] private float quickReleaseThreshold = 0.1f;
    [Tooltip("Um grapple precisa durar mais que este tempo para restaurar a tolerância de cooldown.")]
    [SerializeField] private float longGrappleThreshold = 1.0f;
    #endregion

    #region Joint Settings
    [Header("Configurações da Junta (Puxão)")]
    [SerializeField] private float springForce = 8f;
    [SerializeField] private float damper = 7f;
    [SerializeField] private float massScale = 4.5f;
    [SerializeField] private float minSpringSize = .1f;
    [SerializeField] private float maxSpringSize = .8f;
    #endregion

    #region Swing Settings
    [Header("Configurações do Pêndulo")]
    [SerializeField] private float swingForce = 50f;
    #endregion

    #region Input Buffer
    [Header("Buffer de Input")]
    [SerializeField] private float grappleInputBufferTime = 0.5f;
    #endregion

    #region State
    public bool IsGrappling => _isGrappling;
    public float grappleDistance { get; private set; }
    public Vector3 GrapplePoint { get; private set; }

    private bool _isGrappling = false;
    private bool _hasGrappleAvailable = true;
    private bool _hasValidGrappleTarget;
    private bool _isInputBuffered;
    private bool _wasPredictionTargetValidLastFrame = false;

    private float _cooldownTimer;
    private float _grappleTimer;
    private float _grappleInputBufferTimer;
    private float _grappleValidityCheckTimer;
    
    private float _grappleElapsedTime;
    private bool _hasCooldownTolerance = true;

    private Vector2 _moveInput;
    private Rigidbody _grappledRigidbody;
    private IGrappleable _currentGrappleableTarget;
    private SpringJoint _joint;
    private GameObject _predictionPointInstance;
    private RaycastHit _predictionHit;
    
    private Vector3 _grappledPointOffsetLocalToTarget; 
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovementController>();
        _hasGrappleAvailable = true;

        if (predictionPointPrefab != null)
        {
            _predictionPointInstance = Instantiate(predictionPointPrefab);
            _predictionPointInstance.SetActive(false);
        }
        
        if (lineRenderer != null) lineRenderer.positionCount = 0;
    }

    private void OnEnable()
    {
        InputManager.Instance.OnGrappleStarted += OnGrappleInputStarted;
        InputManager.Instance.OnGrappleCanceled += OnGrappleInputCanceled;
        InputManager.Instance.OnMove += SetMoveInput;
        InputManager.Instance.OnJumpPerformed += HandleJumpInput;
        if (playerMovement != null) playerMovement.OnGroundLanded += ResetGrappleAvailability;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnGrappleStarted -= OnGrappleInputStarted;
        InputManager.Instance.OnGrappleCanceled -= OnGrappleInputCanceled;
        InputManager.Instance.OnMove -= SetMoveInput;
        InputManager.Instance.OnJumpPerformed -= HandleJumpInput;
        if (playerMovement != null) playerMovement.OnGroundLanded -= ResetGrappleAvailability;

        if (_wasPredictionTargetValidLastFrame)
        {
            OnPredictionTargetChanged?.Invoke(null);
            _wasPredictionTargetValidLastFrame = false;
        }
        if (_predictionPointInstance != null) _predictionPointInstance.SetActive(false);
        
        if (_isGrappling)
        {
            StopGrapple();
        }
        if (lineRenderer != null) lineRenderer.positionCount = 0;
    }

    private void Update()
    {
        HandleTimers();
        HandleInputBuffer();
        UpdateGrappleStateAndVisuals();
        CheckGrappleValidity();
    }

    private void LateUpdate() 
    { 
        if (_isGrappling) 
        {
            DrawRope(); 
        }
    }
    private void FixedUpdate() { ApplySwingForce(); }
    #endregion

    #region Prediction & State Broadcasting
    private void UpdateGrappleStateAndVisuals()
    {
        _hasValidGrappleTarget = !_isGrappling && FindValidGrappleTarget(out _predictionHit);
        bool isPredictionReady = _hasValidGrappleTarget && CanCurrentlyAttemptGrapple();

        UpdatePredictionPointVisual(isPredictionReady);
        BroadcastPredictionStateChange(isPredictionReady);
    }

    private void UpdatePredictionPointVisual(bool show)
    {
        if (_predictionPointInstance == null) return;

        if (show && !_predictionPointInstance.activeSelf)
        {
            _predictionPointInstance.SetActive(true);
        }
        else if (!show && _predictionPointInstance.activeSelf)
        {
            _predictionPointInstance.SetActive(false);
        }

        if (show)
        {
            _predictionPointInstance.transform.position = _predictionHit.point;
        }
    }

    private void BroadcastPredictionStateChange(bool isReady)
    {
        if (isReady && !_wasPredictionTargetValidLastFrame)
        {
            OnPredictionTargetChanged?.Invoke(_predictionPointInstance.transform);
        }
        else if (!isReady && _wasPredictionTargetValidLastFrame)
        {
            OnPredictionTargetChanged?.Invoke(null);
        }
        _wasPredictionTargetValidLastFrame = isReady;
    }
    #endregion

    #region Input & Event Handlers
    private void SetMoveInput(Vector2 input) => _moveInput = input;
    private void OnGrappleInputStarted()
    {
        if (CanStartGrapple()) { ExecuteGrapple(); }
        else if (!_isGrappling && _cooldownTimer <= 0) { _isInputBuffered = true; _grappleInputBufferTimer = grappleInputBufferTime; }
    }
    private void OnGrappleInputCanceled() { StopGrapple(); ClearInputBuffer(); }
    
    private void HandleJumpInput()
    {
        if (IsGrappling)
        {
            StopGrapple();
        }
    }
    
    private void ResetGrappleAvailability() { if (!canDoMultipleGrapple && !_hasGrappleAvailable) { _hasGrappleAvailable = true; } }
    public void ResetGrapple() { _hasGrappleAvailable = true; _cooldownTimer = 0f; _hasCooldownTolerance = true; }
    #endregion

    #region Core Grapple Logic
    private void ExecuteGrapple()
    {
        _grappleElapsedTime = 0f;
        OnGrappleStarted?.Invoke();
        _isGrappling = true;
        _grappleTimer = maximumTimeGrappling;
        ClearInputBuffer();
        grappleDistance = _predictionHit.distance;
        GrapplePoint = _predictionHit.point;
        
        _currentGrappleableTarget = _predictionHit.collider.GetComponentInParent<IGrappleable>();

        if (_predictionHit.rigidbody != null)
        {
             _grappledRigidbody = _predictionHit.rigidbody;
             _grappledPointOffsetLocalToTarget = _predictionHit.rigidbody.transform.InverseTransformPoint(GrapplePoint);
        }
        else
        {
            _grappledRigidbody = null;
            _grappledPointOffsetLocalToTarget = _predictionHit.collider.transform.InverseTransformPoint(GrapplePoint);
        }

        CreateAndConfigureJoint(GrapplePoint);
        if (!canDoMultipleGrapple && !playerMovement.isGrounded) { _hasGrappleAvailable = false; }
        UpdateGrappleStateAndVisuals();
    }

    public void StopGrapple()
    {
        if (!_isGrappling) return;

        bool isQuickRelease = _hasCooldownTolerance && _grappleElapsedTime < quickReleaseThreshold;

        OnGrappleStopped?.Invoke();
        _isGrappling = false;
        
        Destroy(_joint);
        _grappledRigidbody = null;
        _currentGrappleableTarget = null;
        _grappledPointOffsetLocalToTarget = Vector3.zero;

        if (isQuickRelease)
        {
            _hasCooldownTolerance = false;
        }
        else
        {
            if (CheatManager.Instance == null || !CheatManager.Instance.IsInfiniteGrappleCooldownActive)
            {
                _cooldownTimer = grappleCooldown;
            }
            playerMovement.ResetDoubleJump();
            
            if (!_hasCooldownTolerance && _grappleElapsedTime >= longGrappleThreshold)
            {
                _hasCooldownTolerance = true;
            }
        }
        
        UpdateGrappleStateAndVisuals();
        if (lineRenderer != null) lineRenderer.positionCount = 0;
    }

    private void ApplySwingForce()
    {
        if (!_joint) return;
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        playerMovement.Rb.AddForce(forward * _moveInput.y * swingForce, ForceMode.Force);
        playerMovement.Rb.AddForce(right * _moveInput.x * swingForce, ForceMode.Force);
    }
    #endregion
    private bool FindValidGrappleTarget(out RaycastHit hit)
    {
        return TryFindValidGrappleTarget(cameraTransform.position, cameraTransform.forward, out hit) ||
               TryFindValidGrappleTarget(playerMovement.Rb.position, cameraTransform.forward, out hit, 3f);
    }

    private bool TryFindValidGrappleTarget(Vector3 origin, Vector3 direction, out RaycastHit hit, float sphereRadius = 0f)
    {
        bool didHit;
        if (sphereRadius > 0)
        {
            didHit = Physics.SphereCast(origin, sphereRadius, direction, out hit, maxGrappleDistance, grappleableLayer);
        }
        else
        {
            didHit = Physics.Raycast(origin, direction, out hit, maxGrappleDistance, grappleableLayer);
        }

        if (didHit)
        {
            IGrappleable targetGrappleable = hit.collider.GetComponentInParent<IGrappleable>();

            if ((targetGrappleable != null && targetGrappleable.CanBeGrappledNow()) ||
                (targetGrappleable == null && (hit.rigidbody == null || !hit.rigidbody.isKinematic)))
            {
                return true;
            }
        }
        return false;
    }

    private void DrawRope()
    {
        if (!_joint) return;
        
        if (_grappledRigidbody != null) 
        {
            UpdateGrappleAnchorPosition();
        }
        else if (_currentGrappleableTarget != null && _currentGrappleableTarget.GetGameObject() != null)
        {
            GrapplePoint = _currentGrappleableTarget.GetGameObject().transform.TransformPoint(_grappledPointOffsetLocalToTarget);
        }
        
        if (_joint) _joint.connectedAnchor = GrapplePoint;
        
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, grappleTip.position);
        lineRenderer.SetPosition(1, GrapplePoint);
    }
    
    private void UpdateGrappleAnchorPosition()
    {
        GrapplePoint = _grappledRigidbody.transform.TransformPoint(_grappledPointOffsetLocalToTarget);
    }

    #region State & Timers
    private void HandleTimers()
    {
        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;
        if (_isGrappling)
        {
            _grappleElapsedTime += Time.deltaTime;
            bool infiniteDuration = CheatManager.Instance != null && CheatManager.Instance.IsInfiniteGrappleDurationActive;
            if (!infiniteDuration) { _grappleTimer -= Time.deltaTime; if (_grappleTimer <= 0) StopGrapple(); }
        }
        if (canDoMultipleGrapple && _cooldownTimer <= 0) { _hasGrappleAvailable = true; }
    }

    private void HandleInputBuffer()
    {
        if (!_isInputBuffered) return;
        _grappleInputBufferTimer -= Time.deltaTime;
        if (CanStartGrapple()) { ExecuteGrapple(); }
        else if (_grappleInputBufferTimer <= 0f) { ClearInputBuffer(); }
    }
    private void ClearInputBuffer() { _isInputBuffered = false; _grappleInputBufferTimer = 0f; }
    private bool CanStartGrapple() { return _hasValidGrappleTarget && CanCurrentlyAttemptGrapple(); }
    private bool CanCurrentlyAttemptGrapple()
    {
        bool hasUseAvailable = canDoMultipleGrapple || _hasGrappleAvailable || playerMovement.isGrounded;
        return hasUseAvailable && _cooldownTimer <= 0 && !_isGrappling;
    }
    #endregion

    #region Utility
    private void CreateAndConfigureJoint(Vector3 connectedPoint)
    {
        _joint = gameObject.AddComponent<SpringJoint>();
        _joint.autoConfigureConnectedAnchor = false;
        _joint.anchor = Vector3.zero;
        _joint.connectedAnchor = connectedPoint;

        float distanceFromPoint = Vector3.Distance(transform.position, connectedPoint);
        _joint.maxDistance = distanceFromPoint * maxSpringSize;
        _joint.minDistance = distanceFromPoint * minSpringSize;
        _joint.spring = springForce;
        _joint.damper = damper;
        _joint.massScale = massScale;
    }

    private void CheckGrappleValidity()
    {
        if (!_isGrappling || _currentGrappleableTarget == null) return;

        _grappleValidityCheckTimer -= Time.deltaTime;
        if (_grappleValidityCheckTimer <= 0f)
        {
            if (_currentGrappleableTarget.GetGameObject() == null || !_currentGrappleableTarget.CanBeGrappledNow())
            {
                StopGrapple();
            }
            _grappleValidityCheckTimer = grappleValidityCheckInterval;
        }
    }
    #endregion
}