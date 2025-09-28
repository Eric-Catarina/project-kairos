// Local: Assets/Scripts/GrapplingHookController.cs

using DG.Tweening;
using UnityEngine;

public class GrapplingHookController : MonoBehaviour
{
    // ALTERAÇÃO: Adicionada uma referência para a estratégia atual.
    private IGrappleStrategy _currentStrategy;

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
    // ALTERAÇÃO: Campos tornados públicos para serem acessados pelas estratégias
    public float springForce = 8f;
    public float damper = 7f;
    public float massScale = 4.5f;
    public float minSpringSize = .1f;
    public float maxSpringSize = .8f;

    [Header("Configurações do Pêndulo")]
    // ALTERAÇÃO: Campo tornado público
    public float swingForce = 50f;

    [Header("Referências")]
    [SerializeField] private Transform grappleTip;
    // ALTERAÇÃO: Campo tornado público
    public Transform cameraTransform;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject predictionPointPrefab;

    // ALTERAÇÃO: Propriedade pública para acessar o PlayerMovementController
    public PlayerMovementController PlayerMovement { get; private set; }

    // ALTERAÇÃO: Removida a referência direta ao SpringJoint daqui.
    // private SpringJoint joint;

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
        // ALTERAÇÃO: Renomeado para usar a nova propriedade pública
        PlayerMovement = GetComponent<PlayerMovementController>();
        hasGrappleAvailable = true;
    }

    private void OnEnable()
    {
        InputManager.Instance.OnGrappleStarted += StartGrapple;
        InputManager.Instance.OnGrappleCanceled += StopGrapple;
        InputManager.Instance.OnMove += SetMoveInput;
        if (PlayerMovement != null)
            PlayerMovement.OnGroundLanded += OnGroundLanded;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnGrappleStarted -= StartGrapple;
        InputManager.Instance.OnGrappleCanceled -= StopGrapple;
        InputManager.Instance.OnMove -= SetMoveInput;
        if (PlayerMovement != null)
            PlayerMovement.OnGroundLanded -= OnGroundLanded;
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
        // ALTERAÇÃO: Delega a chamada para a estratégia
        _currentStrategy?.LateUpdate(this);
    }

    private void FixedUpdate()
    {
        // ALTERAÇÃO: Delega a chamada para a estratégia
        _currentStrategy?.FixedUpdate(this);
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
        bool groundedOverride = PlayerMovement != null && PlayerMovement.isGrounded;
        if (!canDoMultipleGrapple && !hasGrappleAvailable && !groundedOverride) return;
        if (cooldownTimer > 0 || isGrappling || !hasPredictedPoint) return;

        grappleTimer = maximumTimeGrappling;

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer))
        {
            grapplePoint = hit.point;
            _grappleAnchorRigidbody = hit.rigidbody;

            // --- INÍCIO DA LÓGICA DE ESTRATÉGIA ---
            PullableObject pullable = hit.collider.GetComponent<PullableObject>();
            if (pullable != null)
            {
                _currentStrategy = pullable.CreateStrategy();
            }
            else
            {
                _currentStrategy = new SwingStrategy();
            }
            // --- FIM DA LÓGICA DE ESTRATÉGIA ---

            if (_grappleAnchorRigidbody != null)
            {
                _grapplePointRelativeOffset = hit.transform.InverseTransformPoint(grapplePoint);
            }
        }
        else
        {
            grapplePoint = predictedPoint;
            _grappleAnchorRigidbody = null;
            // Se não acertou nada específico, usa a estratégia de balanço por padrão
            _currentStrategy = new SwingStrategy();
        }

        isGrappling = true;

        // ALTERAÇÃO: Delega a execução para o objeto de estratégia
        _currentStrategy.Execute(this);

        lineRenderer.positionCount = 2;

        if (!canDoMultipleGrapple && !groundedOverride)
            hasGrappleAvailable = false;
    }

    // ALTERAÇÃO: Removido ApplySwingForce. A lógica agora está na SwingStrategy.
    // private void ApplySwingForce() { ... }

    public void StopGrapple()
    {
        if (!isGrappling) return;

        // ALTERAÇÃO: Delega a parada para a estratégia antes de limpar.
        _currentStrategy?.Stop(this);
        _currentStrategy = null;

        isGrappling = false;
        cooldownTimer = grappleCooldown;
        lineRenderer.positionCount = 0;

        // ALTERAÇÃO: Removido Destroy(joint). Isso agora é responsabilidade da SwingStrategy.
        _grappleAnchorRigidbody = null;
        PlayerMovement.EnableDoubleJump();
    }

    // ALTERAÇÃO: Método tornado público para ser chamado pelas estratégias.
    public void DrawRope()
    {
        // Se não houver estratégia ativa (ou seja, não está com gancho), não desenha.
        if (_currentStrategy == null) return;

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

    public void ResetGrapple()
    {
        hasGrappleAvailable = true;
        cooldownTimer = 0f;
    }

    #region MÉTODOS PÚBLICOS PARA ESTRATÉGIAS
    // ALTERAÇÃO: Adicionados métodos públicos para que as estratégias possam obter
    // informações do controlador de forma segura, sem expor campos privados.

    public Vector2 GetMoveInput() => moveInput;
    public Vector3 GetGrapplePoint() => grapplePoint;
    public void SetGrapplePoint(Vector3 newPoint) => grapplePoint = newPoint;
    public Rigidbody GetGrappleAnchorRigidbody() => _grappleAnchorRigidbody;
    public Vector3 GetGrapplePointRelativeOffset() => _grapplePointRelativeOffset;

    #endregion
}