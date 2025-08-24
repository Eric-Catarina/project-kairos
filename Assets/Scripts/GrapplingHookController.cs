// Local: Assets/Scripts/GrapplingHookController.cs

using DG.Tweening;
using UnityEngine;

public class GrapplingHookController : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private bool isGrappling = false;
    public bool IsGrappling => isGrappling;

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
    private Vector3 grapplePoint, currentGrapplePosition;
    private Vector2 moveInput;
    private float cooldownTimer;
    private GameObject currentPredictionPoint;

    private Vector3 predictedPoint;
    private bool hasPredictedPoint;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovementController>();
    }

    private void OnEnable()
    {
        InputManager.Instance.OnGrappleStarted += StartGrapple;
        InputManager.Instance.OnGrappleCanceled += StopGrapple;
        InputManager.Instance.OnMove += SetMoveInput;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnGrappleStarted -= StartGrapple;
        InputManager.Instance.OnGrappleCanceled -= StopGrapple;
        InputManager.Instance.OnMove -= SetMoveInput;
    }

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
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
            if (currentPredictionPoint != null)
                currentPredictionPoint.SetActive(false);
            hasPredictedPoint = false;
            return;
        }

        RaycastHit hit;
        bool hitFound = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer);
        if (!hitFound)
        {
            hitFound = Physics.SphereCast(cameraTransform.position, 10f, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer);
        }

        if (hitFound)
        {
            predictedPoint = hit.point;
            hasPredictedPoint = true;

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
            if (currentPredictionPoint != null)
                currentPredictionPoint.SetActive(false);
        }
    }

    private void StartGrapple()
    {
        if (cooldownTimer > 0 || isGrappling || !hasPredictedPoint) return;

        isGrappling = true;
        grapplePoint = predictedPoint;

        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;

        float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

        joint.maxDistance = distanceFromPoint * maxSpringSize;
        joint.minDistance = distanceFromPoint * minSpringSize;
        joint.spring = springForce;
        joint.damper = damper;
        joint.massScale = massScale;

        lineRenderer.positionCount = 2;
    }

    private void CheckForSwingPoints()
    {
        RaycastHit sphereCastHit;
        Physics.SphereCast(cameraTransform.position, 0.5f, cameraTransform.forward,
                            out sphereCastHit, maxGrappleDistance, grappleLayer);

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

        playerMovement.EnableDoubleJump();

        // Prediction point volta a aparecer após o grapple
        if (currentPredictionPoint != null)
        {
            currentPredictionPoint.SetActive(true);
        }
    }

    private void DrawRope()
    {
        if (!joint) return;
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);


        lineRenderer.SetPosition(0, grappleTip.position);
        lineRenderer.SetPosition(1, currentGrapplePosition);
    }
}