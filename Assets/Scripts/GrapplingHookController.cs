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

    [Header("Configurações do Pêndulo")]
    [SerializeField] private float swingForce = 50f;

    [Header("Referências")]
    [SerializeField] private Transform grappleTip;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LineRenderer lineRenderer;

    private PlayerMovementController playerMovement;
    private SpringJoint joint;
    private Vector3 grapplePoint;
    private Vector2 moveInput;
    private float cooldownTimer;

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

    private void StartGrapple()
    {
        if (cooldownTimer > 0 || isGrappling) return;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxGrappleDistance, grappleLayer))
        {
            isGrappling = true;
            grapplePoint = hit.point;

            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            joint.maxDistance = distanceFromPoint ;
            joint.minDistance = 1;
            joint.spring = springForce;
            joint.damper = damper;
            joint.massScale = massScale;

            lineRenderer.positionCount = 2;
        }
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
    }

    private void DrawRope()
    {
        if (!joint) return;
        lineRenderer.SetPosition(0, grappleTip.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }
}