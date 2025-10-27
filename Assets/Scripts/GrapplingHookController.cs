// Assets/Scripts/GrapplingHookController.cs

using UnityEngine;
using System;

public class GrapplingHookController : MonoBehaviour
{
    #region Events
    public event Action OnGrappleStarted;
    public event Action OnGrappleStopped;
    public event Action OnValidGrappleTargetAcquired;
    public event Action OnValidGrappleTargetLost;
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
    
    private bool _isGrappling = false;
    private bool _hasGrappleAvailable = true;
    private bool _hasValidGrappleTarget;
    private bool _isInputBuffered;
    private bool _wasPredictionTargetValidLastFrame = false; 

    private float _cooldownTimer;
    private float _grappleTimer;
    private float _grappleInputBufferTimer;
    
    private Vector2 _moveInput;
    private Vector3 _grapplePoint;
    private Vector3 _grappledPointLocalOffset;
    private Rigidbody _grappledRigidbody;
    
    private SpringJoint _joint;
    private GameObject _currentPredictionPoint;
    private RaycastHit _predictionHit;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovementController>();
        _hasGrappleAvailable = true;
    }

    private void OnEnable()
    {
        InputManager.Instance.OnGrappleStarted += OnGrappleInputStarted;
        InputManager.Instance.OnGrappleCanceled += OnGrappleInputCanceled;
        InputManager.Instance.OnMove += SetMoveInput;
        if (playerMovement != null) playerMovement.OnGroundLanded += ResetGrappleAvailability;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnGrappleStarted -= OnGrappleInputStarted;
        InputManager.Instance.OnGrappleCanceled -= OnGrappleInputCanceled;
        InputManager.Instance.OnMove -= SetMoveInput;
        if (playerMovement != null) playerMovement.OnGroundLanded -= ResetGrappleAvailability;

        if (_wasPredictionTargetValidLastFrame)
        {
             OnValidGrappleTargetLost?.Invoke();
             _wasPredictionTargetValidLastFrame = false;
        }
        if (_currentPredictionPoint != null) _currentPredictionPoint.SetActive(false);
    }

    private void Update()
    {
        HandleTimers();
        HandleInputBuffer();
        UpdateGrappleStateAndVisuals();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void FixedUpdate()
    {
        ApplySwingForce();
    }
    #endregion

    #region Input & Event Handlers
    private void SetMoveInput(Vector2 input) => _moveInput = input;

    private void OnGrappleInputStarted()
    {
        if (CanStartGrapple())
        {
            ExecuteGrapple();
        }
        else if (!_isGrappling && _cooldownTimer <= 0)
        {
            _isInputBuffered = true;
            _grappleInputBufferTimer = grappleInputBufferTime;
        }
    }

    private void OnGrappleInputCanceled()
    {
        StopGrapple();
        ClearInputBuffer();
    }

    private void ResetGrappleAvailability()
    {
        if (!canDoMultipleGrapple && !_hasGrappleAvailable)
        {
            _hasGrappleAvailable = true;
        }
    }

    public void ResetGrapple()
    {
        _hasGrappleAvailable = true;
        _cooldownTimer = 0f;
    }
    #endregion

    #region Core Grapple Logic
    private void ExecuteGrapple()
    {
        OnGrappleStarted?.Invoke();
        _isGrappling = true;
        _grappleTimer = maximumTimeGrappling;
        ClearInputBuffer();

        grappleDistance = _predictionHit.distance;
        _grapplePoint = _predictionHit.point;
        _grappledRigidbody = _predictionHit.rigidbody;

        if (_grappledRigidbody != null)
        {
            _grappledPointLocalOffset = _predictionHit.transform.InverseTransformPoint(_grapplePoint);
        }

        CreateAndConfigureJoint(_grapplePoint);

        if (!canDoMultipleGrapple && !playerMovement.isGrounded)
        {
            _hasGrappleAvailable = false;
        }
        
        UpdateGrappleStateAndVisuals();
    }

    private void StopGrapple()
    {
        if (!_isGrappling) return;

        OnGrappleStopped?.Invoke();
        _isGrappling = false;
        
        if (CheatManager.Instance == null || !CheatManager.Instance.IsInfiniteGrappleCooldownActive)
        {
            _cooldownTimer = grappleCooldown;
        }
        
        lineRenderer.positionCount = 0;
        Destroy(_joint);
        _grappledRigidbody = null;
        
        playerMovement.ResetDoubleJump();
        
        UpdateGrappleStateAndVisuals();
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
    
    #region Prediction, Visuals & State Broadcasting
    private void UpdateGrappleStateAndVisuals()
    {
        if (_isGrappling)
        {
            _hasValidGrappleTarget = false;
        }
        else
        {
            _hasValidGrappleTarget = FindValidGrappleTarget(out _predictionHit);
        }

        bool isPredictionReady = _hasValidGrappleTarget && CanCurrentlyAttemptGrapple();

        UpdatePredictionPoint(isPredictionReady);
        BroadcastPredictionState(isPredictionReady);
    }

    private void UpdatePredictionPoint(bool show)
    {
        if (show)
        {
            if (_currentPredictionPoint == null)
            {
                _currentPredictionPoint = Instantiate(predictionPointPrefab, _predictionHit.point, Quaternion.identity);
            }
            else
            {
                _currentPredictionPoint.SetActive(true);
                _currentPredictionPoint.transform.position = _predictionHit.point;
            }
        }
        else if (_currentPredictionPoint != null)
        {
            _currentPredictionPoint.SetActive(false);
        }
    }

    private void BroadcastPredictionState(bool isReady)
    {
        if (isReady && !_wasPredictionTargetValidLastFrame)
        {
            OnValidGrappleTargetAcquired?.Invoke();
        }
        else if (!isReady && _wasPredictionTargetValidLastFrame)
        {
            OnValidGrappleTargetLost?.Invoke();
        }
        _wasPredictionTargetValidLastFrame = isReady;
    }

    private bool FindValidGrappleTarget(out RaycastHit hit)
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleableLayer))
        {
            return true;
        }
        return Physics.SphereCast(cameraTransform.position, 1f, cameraTransform.forward, out hit, maxGrappleDistance, grappleableLayer);
    }
    
    private void DrawRope()
    {
        if (!_joint) return;
        UpdateGrappleAnchorPosition();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, grappleTip.position);
        lineRenderer.SetPosition(1, _grapplePoint);
    }

    private void UpdateGrappleAnchorPosition()
    {
        if (_grappledRigidbody != null)
        {
            _grapplePoint = _grappledRigidbody.transform.TransformPoint(_grappledPointLocalOffset);
            if (_joint) _joint.connectedAnchor = _grapplePoint;
        }
    }
    #endregion
    
    #region State & Timers
    private void HandleTimers()
    {
        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

        if (_isGrappling)
        {
            bool infiniteDuration = CheatManager.Instance != null && CheatManager.Instance.IsInfiniteGrappleDurationActive;
            if (!infiniteDuration)
            {
                _grappleTimer -= Time.deltaTime;
                if (_grappleTimer <= 0) StopGrapple();
            }
        }

        if (canDoMultipleGrapple && _cooldownTimer <= 0)
        {
            _hasGrappleAvailable = true;
        }
    }

    private void HandleInputBuffer()
    {
        if (!_isInputBuffered) return;
        _grappleInputBufferTimer -= Time.deltaTime;
        
        if (CanStartGrapple())
        {
            ExecuteGrapple();
        }
        else if (_grappleInputBufferTimer <= 0f)
        {
            ClearInputBuffer();
        }
    }

    private void ClearInputBuffer()
    {
        _isInputBuffered = false;
        _grappleInputBufferTimer = 0f;
    }
    
    private bool CanStartGrapple()
    {
        return _hasValidGrappleTarget && CanCurrentlyAttemptGrapple();
    }

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
    #endregion
}