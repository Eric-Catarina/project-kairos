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

    // Buffer de input para o grapple
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
        if (grappleTimer > 0) grappleTimer -= Time.deltaTime;
        if (grappleTimer <= 0 && isGrappling) StopGrapple();

        if (canDoMultipleGrapple && cooldownTimer <= 0) hasGrappleAvailable = true;

        // Lógica do buffer de input: só tenta iniciar se agora existe ponto viável
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

    // Novo método para lidar com o buffer
    private void BufferOrStartGrapple()
    {
        if (hasPredictedPoint && CanStartGrapple())
        {
            StartGrappleInternal();
        }
        else
        {
            // Só ativa o buffer se NÃO há ponto viável
            if (!hasPredictedPoint)
            {
                grappleInputBuffered = true;
                grappleInputBufferTimer = grappleInputBufferTime;
            }
        }
    }

    // Checa se pode iniciar o grapple agora
    private bool CanStartGrapple()
    {
        bool groundedOverride = playerMovement != null && playerMovement.isGrounded;
        return (canDoMultipleGrapple || hasGrappleAvailable || groundedOverride)
            && cooldownTimer <= 0
            && !isGrappling
            && hasPredictedPoint;
    }

    // Refatora o StartGrapple para ser chamado internamente
    private void StartGrapple()
    {
        // Mantém para compatibilidade, mas não usa mais diretamente
        BufferOrStartGrapple();
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

    // Novo método para cancelar o grapple e limpar o buffer
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
        cooldownTimer = grappleCooldown;
        lineRenderer.positionCount = 0;
        Destroy(joint);

        _grappleAnchorRigidbody = null;
        playerMovement.ResetDoubleJump();

        // Sempre limpa o buffer ao soltar o input
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

    // Tenta um Raycast simples (sem sphere cast) — usado ao iniciar o grapple para manter o comportamento anterior
    private bool TryRaycastGrapple(out RaycastHit hit)
    {
        return Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer);
    }

    // Tenta Raycast e, se falhar, faz um SphereCast como fallback — usado para predição visual
    private bool TryFindGrappleHit(out RaycastHit hit)
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer))
        {
            return true;
        }

        return Physics.SphereCast(cameraTransform.position, 1f, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer);
    }

    // Cria e configura a junta do grapple com os valores padronizados
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

    // Atualiza o ponto do grapple quando o objeto âncora se move
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