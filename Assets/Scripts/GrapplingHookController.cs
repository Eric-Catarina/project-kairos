// Local: Assets/Scripts/GrapplingHookController.cs

using UnityEngine;

public class GrapplingHookController : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private bool canDoMultipleGrapple = false;
    [SerializeField] private bool isGrappling = false;
    [SerializeField] private float maximumTimeGrappling = 3f;
    public bool IsGrappling => isGrappling;
    private bool hasGrappleAvailable = true;

    [Header("Configurações do Gancho")]
    [SerializeField] private float maxGrappleDistance = 50f;
    [SerializeField] private float grappleCooldown = 1f;
    [SerializeField] private LayerMask grappleLayer;

    [Header("Configurações da Junta (Puxão)")]
    [SerializeField] private float springForce = 8f;
    [SerializeField] private float damper = 7f;
    [SerializeField] private float massScale = 4.5f;
    [SerializeField] private float minSpringSize = .1f;
    [SerializeField] private float maxSpringSize = .8f;

    [Header("Configurações do Pêndulo")]
    [SerializeField] private float swingForce = 50f;

    [Header("Referências")]
    [SerializeField] private Transform grappleTip;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject predictionPointPrefab;

    private PlayerMovementController playerMovement;
    private SpringJoint joint;
    private Vector3 grapplePoint;
    private Vector2 moveInput;
    private float cooldownTimer;
    private GameObject currentPredictionPoint;
    public float grappleDistance;

    private Vector3 predictedPoint;
    private bool hasPredictedPoint;
    private float grappleTimer;

    private Rigidbody _grappleAnchorRigidbody;
    private Vector3 _grapplePointRelativeOffset;
    
    private bool grappleInputBuffered = false;
    private float grappleInputBufferTimer = 0f;
    [SerializeField]
    private float grappleInputBufferTime = 1f;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovementController>();
        hasGrappleAvailable = true;
    }

    private void OnEnable()
    {
        InputManager.Instance.OnGrappleStarted += BufferOrStartGrapple;
        InputManager.Instance.OnGrappleCanceled += OnGrappleCanceled;
        InputManager.Instance.OnMove += SetMoveInput;
        if (playerMovement != null)
            playerMovement.OnGroundLanded += OnGroundLanded;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnGrappleStarted -= BufferOrStartGrapple;
        InputManager.Instance.OnGrappleCanceled -= OnGrappleCanceled;
        InputManager.Instance.OnMove -= SetMoveInput;
        if (playerMovement != null)
            playerMovement.OnGroundLanded -= OnGroundLanded;
    }

    private void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        if (isGrappling)
        {
            bool infiniteDuration = CheatManager.Instance != null && CheatManager.Instance.IsInfiniteGrappleDurationActive;
            if (!infiniteDuration)
            {
                if (grappleTimer > 0) grappleTimer -= Time.deltaTime;
                if (grappleTimer <= 0) StopGrapple();
            }
        }

        if (canDoMultipleGrapple && cooldownTimer <= 0) hasGrappleAvailable = true;
        
        if (grappleInputBuffered)
        {
            grappleInputBufferTimer -= Time.deltaTime;
            if (hasPredictedPoint && CanStartGrapple())
            {
                StartGrappleInternal();
                grappleInputBuffered = false;
            }
            else if (grappleInputBufferTimer <= 0f)
            {
                grappleInputBuffered = false;
            }
        }

        UpdatePredictionPoint();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void FixedUpdate()
    {
        ApplySwingForce();
    }

    private void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void UpdatePredictionPoint()
    {
        if (isGrappling)
        {
            if (currentPredictionPoint != null) currentPredictionPoint.SetActive(false);
            hasPredictedPoint = false;
            return;
        }
        RaycastHit hit;
        bool hitFound = TryFindGrappleHit(out hit);

        if (hitFound)
        {
            predictedPoint = hit.point;
            hasPredictedPoint = true;
            grappleDistance = hit.distance;

            DrawPredictionPoint(predictedPoint);
        }
        else
        {
            hasPredictedPoint = false;
            if (currentPredictionPoint != null) currentPredictionPoint.SetActive(false);
            grappleDistance = 0f;
        }
    }

    private void DrawPredictionPoint(Vector3 position)
    {
        if (!hasGrappleAvailable) return;
        if (currentPredictionPoint == null)
        {
            currentPredictionPoint = Instantiate(predictionPointPrefab, position, Quaternion.identity);
        }
        else
        {
            currentPredictionPoint.SetActive(true);
            currentPredictionPoint.transform.position = position;
        }
    }
    
    private void BufferOrStartGrapple()
    {
        if (hasPredictedPoint && CanStartGrapple())
        {
            StartGrappleInternal();
        }
        else
        {
            if (!hasPredictedPoint)
            {
                grappleInputBuffered = true;
                grappleInputBufferTimer = grappleInputBufferTime;
            }
        }
    }
    
    private bool CanStartGrapple()
    {
        bool groundedOverride = playerMovement != null && playerMovement.isGrounded;
        return (canDoMultipleGrapple || hasGrappleAvailable || groundedOverride)
            && cooldownTimer <= 0
            && !isGrappling
            && hasPredictedPoint;
    }
    
    private void StartGrappleInternal()
    {
        isGrappling = true;
        grappleTimer = maximumTimeGrappling;

        RaycastHit hit;
        if (TryRaycastGrapple(out hit))
        {
            grapplePoint = hit.point;
            _grappleAnchorRigidbody = hit.rigidbody;
            if (_grappleAnchorRigidbody != null)
            {
                _grapplePointRelativeOffset = hit.transform.InverseTransformPoint(grapplePoint);
            }
        }
        else
        {
            grapplePoint = predictedPoint;
            _grappleAnchorRigidbody = null;
        }

        CreateAndConfigureJoint(grapplePoint);

        bool groundedOverride = playerMovement != null && playerMovement.isGrounded;
        if (!canDoMultipleGrapple && !groundedOverride)
            hasGrappleAvailable = false;
    }

    private void ApplySwingForce()
    {
        if (!joint) return;

        Vector3 viewDirection = cameraTransform.forward;
        Vector3 rightDirection = cameraTransform.right;

        playerMovement.Rb.AddForce(viewDirection * moveInput.y * swingForce, ForceMode.Force);
        playerMovement.Rb.AddForce(rightDirection * moveInput.x * swingForce, ForceMode.Force);
    }
    
    private void OnGrappleCanceled()
    {
        StopGrapple();
        grappleInputBuffered = false;
        grappleInputBufferTimer = 0f;
    }

    public void StopGrapple()
    {
        if (!isGrappling) return;

        isGrappling = false;
        
        if (CheatManager.Instance == null || !CheatManager.Instance.IsInfiniteGrappleCooldownActive)
        {
            cooldownTimer = grappleCooldown;
        }
        
        lineRenderer.positionCount = 0;
        Destroy(joint);

        _grappleAnchorRigidbody = null;
        playerMovement.ResetDoubleJump();
        
        grappleInputBuffered = false;
        grappleInputBufferTimer = 0f;
    }

    private void DrawRope()
    {
        if (!joint) return;
        UpdateGrappleAnchor();
        lineRenderer.SetPosition(0, grappleTip.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }
    
    private bool TryRaycastGrapple(out RaycastHit hit)
    {
        return Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer);
    }
    
    private bool TryFindGrappleHit(out RaycastHit hit)
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer))
        {
            return true;
        }
        return Physics.SphereCast(cameraTransform.position, 1f, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer);
    }
    
    private void CreateAndConfigureJoint(Vector3 connectedPoint)
    {
        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = connectedPoint;
        float distanceFromPoint = Vector3.Distance(transform.position, connectedPoint);
        joint.maxDistance = distanceFromPoint * maxSpringSize;
        joint.minDistance = distanceFromPoint * minSpringSize;
        joint.spring = springForce;
        joint.damper = damper;
        joint.massScale = massScale;
        lineRenderer.positionCount = 2;
    }
    
    private void UpdateGrappleAnchor()
    {
        if (_grappleAnchorRigidbody != null)
        {
            grapplePoint = _grappleAnchorRigidbody.transform.TransformPoint(_grapplePointRelativeOffset);
            if (joint) joint.connectedAnchor = grapplePoint;
        }
    }

    private void OnGroundLanded()
    {
        if (!canDoMultipleGrapple && !hasGrappleAvailable)
        {
            hasGrappleAvailable = true;
        }
    }
    
    public void ResetGrapple()
    {
        hasGrappleAvailable = true;
        cooldownTimer = 0f;
    }
}