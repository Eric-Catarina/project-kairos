// Local: Assets/Scripts/GrapplingHookController.cs

using DG.Tweening;
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

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovementController>();
        hasGrappleAvailable = true;
    }

    private void OnEnable()
    {
        InputManager.Instance.OnGrappleStarted += StartGrapple;
        InputManager.Instance.OnGrappleCanceled += StopGrapple;
        InputManager.Instance.OnMove += SetMoveInput;
        if (playerMovement != null)
            playerMovement.OnGroundLanded += OnGroundLanded;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnGrappleStarted -= StartGrapple;
        InputManager.Instance.OnGrappleCanceled -= StopGrapple;
        InputManager.Instance.OnMove -= SetMoveInput;
        if (playerMovement != null)
            playerMovement.OnGroundLanded -= OnGroundLanded;
    }

    private void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        if (grappleTimer > 0) grappleTimer -= Time.deltaTime;
        if (grappleTimer <= 0 && isGrappling) StopGrapple();

        if (canDoMultipleGrapple && cooldownTimer <= 0) hasGrappleAvailable = true;
        
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
        bool hitFound = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer);
        if (!hitFound)
        {
            hitFound = Physics.SphereCast(cameraTransform.position, 3f, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer);
        }

        if (hitFound)
        {
            predictedPoint = hit.point;
            hasPredictedPoint = true;
            grappleDistance = hit.distance;

            if (currentPredictionPoint == null)
            {
                currentPredictionPoint = Instantiate(predictionPointPrefab, predictedPoint, Quaternion.identity);
            }
            else
            {
                currentPredictionPoint.SetActive(true);
                currentPredictionPoint.transform.position = predictedPoint;
            }
        }
        else
        {
            hasPredictedPoint = false;
            if (currentPredictionPoint != null) currentPredictionPoint.SetActive(false);
            grappleDistance = 0f;
        }
    }

    private void StartGrapple()
    {
        bool groundedOverride = playerMovement != null && playerMovement.isGrounded;
        if (!canDoMultipleGrapple && !hasGrappleAvailable && !groundedOverride) return;
        if (cooldownTimer > 0 || isGrappling || !hasPredictedPoint) return;

        isGrappling = true;
        grappleTimer = maximumTimeGrappling;

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer))
        {
            grapplePoint = hit.point;
            _grappleAnchorRigidbody = hit.rigidbody;
            if (_grappleAnchorRigidbody != null)
            {
                _grapplePointRelativeOffset = hit.transform.InverseTransformPoint(grapplePoint);
            }
        }
        else // Se o SphereCast acertou
        {
            // Para simplicidade, não vamos lidar com anchor móvel do spherecast, mas a lógica seria similar
            grapplePoint = predictedPoint;
            _grappleAnchorRigidbody = null;
        }

        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = grapplePoint;

        float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

        joint.maxDistance = distanceFromPoint * maxSpringSize;
        joint.minDistance = distanceFromPoint * minSpringSize;
        joint.spring = springForce;
        joint.damper = damper;
        joint.massScale = massScale;

        lineRenderer.positionCount = 2;
        
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

    public void StopGrapple()
    {
        if (!isGrappling) return;

        isGrappling = false;
        cooldownTimer = grappleCooldown;
        lineRenderer.positionCount = 0;
        Destroy(joint);

        _grappleAnchorRigidbody = null;
        playerMovement.EnableDoubleJump();
    }

    private void DrawRope()
    {
        if (!joint) return;

        if (_grappleAnchorRigidbody != null)
        {
            grapplePoint = _grappleAnchorRigidbody.transform.TransformPoint(_grapplePointRelativeOffset);
            joint.connectedAnchor = grapplePoint;
        }

        lineRenderer.SetPosition(0, grappleTip.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    private void OnGroundLanded()
    {
        if (!canDoMultipleGrapple && !hasGrappleAvailable)
        {
            hasGrappleAvailable = true;
        }
    }
}