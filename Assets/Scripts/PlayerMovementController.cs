using TMPro;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    public event Action OnGroundLanded;
    public event Action<float> OnHorizontalVelocityChanged;
    public event Action OnJumped;
    public event Action OnLeftGround;

    [Header("Estado Atual")]
    public bool isGrounded;
    public bool canDoubleJump;
    
    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxMoveSpeed = 30f;
    [SerializeField] private float maxGrappleMoveSpeed = 150f;
    [SerializeField] private float airMultiplier = 0.6f, groundMultiplier = 2.0f, airControlMultiplier = 5f;

private Vector2 lastMoveInput;

    [Header("Configurações de Atrito (Drag)")]
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 2f;
    private float baseAirDrag;
    [SerializeField] private float grappleAirDrag = 0.5f;

    [Header("Configurações de Pulo")]
    [SerializeField] private bool allowDoubleJumpFromGround = false;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float doubleJumpForce = 14f;
    [SerializeField] private float gravityMultiplier = 2.5f;

    [Header("Impulso de Velocidade no Pulo")]
    [Tooltip("Força do impulso para a frente ao pular enquanto se move.")]
    [SerializeField] private float jumpForwardBoost = 5f;
    [Tooltip("Multiplicador da velocidade ao aterrissar (0 = parada total, 1 = sem perda de velocidade).")]
    [SerializeField, Range(0f, 1f)] private float landingVelocityDampening = 0.9f;

    [Header("Pulo Variável (Low/High Jump)")]
    [Tooltip("Multiplicador aplicado na velocidade Y ao soltar o pulo, para um pulo mais curto.")]
    [SerializeField] private float jumpReleaseMultiplier = 0.5f;

    [Header("Coyote Time & Jump Buffer")]
    [Tooltip("Tempo em segundos que o jogador pode pular após sair de uma plataforma.")]
    [SerializeField] private float coyoteTimeDuration = 0.1f;
    [Tooltip("Tempo em segundos que um pulo é 'guardado' se pressionado antes de tocar o chão.")]
    [SerializeField] private float jumpBufferDuration = 0.1f;

    [Header("Bunny Hop")]
    [Tooltip("Janela de tempo após aterrissar para pular e manter a velocidade (ignorar ground drag).")]
    [SerializeField] private float bunnyHopWindow = 0.1f;

    [Header("Verificação de Chão")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Referências")]
    [SerializeField] private Transform orientation;
    [SerializeField] private GrapplingHookController grapplingHookController;
    public TextMeshProUGUI velocityText, distanceText;

    private Rigidbody rb;
    private Vector2 moveInput;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float timeSinceLanded;
    private float currentVelocityInKm;
    private bool isJumping;

    private Vector3 _groundVelocity;
    private Rigidbody _currentPlatformRb;
    private Vector3 _lastPlatformPosition;

    public Rigidbody Rb => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        baseAirDrag = airDrag;
    }

    private void OnEnable()
    {
        InputManager.Instance.OnMove += SetMoveInput;
        InputManager.Instance.OnJumpPerformed += HandleJumpInput;
        InputManager.Instance.OnJumpCanceled += HandleJumpRelease;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnMove -= SetMoveInput;
        InputManager.Instance.OnJumpPerformed -= HandleJumpInput;
        InputManager.Instance.OnJumpCanceled -= HandleJumpRelease;
    }

    private void Update()
    {
        HandleTimers();
        UpdateUI();
    }

    private void FixedUpdate()
    {
        UpdateCurrentVelocityInKm();
        CheckGroundedStatus();
        ApplyPlatformMovement();
        ApplyDrag();
        LimitVelocity();
        MovePlayer();
        ApplyExtraGravity();
    }

    private void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void HandleTimers()
    {
        if (!isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        timeSinceLanded += Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
    }

    private void UpdateUI()
    {
        if (grapplingHookController != null && distanceText != null)
        {
            distanceText.text = "Distancia: " + grapplingHookController.grappleDistance.ToString("F2");
        }
        
        if (velocityText != null)
        {
            velocityText.text = "Velocidade: " + currentVelocityInKm.ToString("F2");
        }
    }

    private void CheckGroundedStatus()
    {
        bool wasGrounded = isGrounded;

        RaycastHit hitInfo;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hitInfo, playerHeight * 0.5f + 0.2f, groundLayer);

        if (isGrounded && hitInfo.rigidbody != null)
        {
            _groundVelocity = hitInfo.rigidbody.linearVelocity;
            if (_currentPlatformRb != hitInfo.rigidbody)
            {
                _currentPlatformRb = hitInfo.rigidbody;
                _lastPlatformPosition = _currentPlatformRb.position;
            }
        }
        else
        {
            _groundVelocity = Vector3.zero;
            _currentPlatformRb = null;
        }

        if (!wasGrounded && isGrounded)
        {
            if (isJumping && currentVelocityInKm <= 100f)
            {
                ApplyLandingDampening();
            }

            isJumping = false;
            timeSinceLanded = 0f;
            canDoubleJump = false;
            OnGroundLanded?.Invoke();

            if (jumpBufferCounter > 0f)
            {
                Jump(jumpForce);
            }
        }

        if (wasGrounded && !isGrounded)
        {
            if(!isJumping)
            {
                 OnLeftGround?.Invoke();
                 coyoteTimeCounter = coyoteTimeDuration;
            }
        }
    }

    private void ApplyPlatformMovement()
    {
        if (_currentPlatformRb != null && isGrounded)
        {
            Vector3 platformDelta = _currentPlatformRb.position - _lastPlatformPosition;
            MovingPlatform mp = _currentPlatformRb.GetComponent<MovingPlatform>();
            float playerInfluence = mp != null ? mp.playerInfluence : 0.69f;
            if (platformDelta != Vector3.zero)
            {
                platformDelta *= playerInfluence;
                rb.position += platformDelta;
            }
            _lastPlatformPosition = _currentPlatformRb.position;
        }
    }

    private void ApplyDrag()
    {
        if (grapplingHookController.IsGrappling)
        {
            rb.linearDamping = grappleAirDrag;
            return;
        }

        if (isGrounded)
        {
            rb.linearDamping = (timeSinceLanded > bunnyHopWindow) ? groundDrag : airDrag;
        }
        else
        {
            rb.linearDamping = airDrag;
        }
    }private void MovePlayer()
{
    if (grapplingHookController.IsGrappling) return;

    Vector3 moveDirection = (orientation.forward * moveInput.y + orientation.right * moveInput.x).normalized;
    Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

    float appliedForceMultiplier = isGrounded ? groundMultiplier : airMultiplier;

    if (!isGrounded)
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            // Direção atual e input desejado
            Vector3 velocityDir = horizontalVelocity.sqrMagnitude > 0.01f ? horizontalVelocity.normalized : moveDirection;
            float angle = Vector3.Angle(velocityDir, moveDirection);

            // Intensidade de correção direcional (apenas redireciona o momentum)
            float angleBoost = Mathf.InverseLerp(0f, 90f, angle);
            float lateralInfluence = Mathf.Lerp(1f, airControlMultiplier, angleBoost);

            // Aplica uma força na direção desejada
            rb.AddForce(moveDirection * moveSpeed * 10f * appliedForceMultiplier * lateralInfluence, ForceMode.Force);

            // 🔒 Clampa a velocidade horizontal para evitar ganho exagerado
            float maxHorizontalSpeed = maxMoveSpeed / 3.6f; // converte km/h → m/s
            Vector3 clampedVelocity = horizontalVelocity;

            if (horizontalVelocity.magnitude > maxHorizontalSpeed)
            {
                clampedVelocity = horizontalVelocity.normalized * maxHorizontalSpeed;
                rb.linearVelocity = new Vector3(clampedVelocity.x, rb.linearVelocity.y, clampedVelocity.z);
            }
        }
    }
    else
    {
        // Movimento normal no chão
        if (moveInput.sqrMagnitude > 0.01f)
        {
            rb.AddForce(moveDirection * moveSpeed * 10f * appliedForceMultiplier, ForceMode.Force);
        }
    }
}



    private void LimitVelocity()
    {
        float currentMaxSpeed = grapplingHookController.IsGrappling ? maxGrappleMoveSpeed : maxMoveSpeed;
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > 200 / 3.6f)
        {
            airDrag = baseAirDrag * 1.2f;
        }
        else
        {
            airDrag = baseAirDrag;
        }

        if (horizontalVelocity.magnitude > (currentMaxSpeed / 3.6f))
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * (currentMaxSpeed / 3.6f);
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }

        OnHorizontalVelocityChanged?.Invoke(horizontalVelocity.magnitude);
    }

    private void HandleJumpInput()
    {
        if (grapplingHookController.IsGrappling) return;

        jumpBufferCounter = jumpBufferDuration;

        if (coyoteTimeCounter > 0f || isGrounded)
        {
            Jump(jumpForce);
            if (allowDoubleJumpFromGround)
            {
                canDoubleJump = true;
            }
        }
        else if (canDoubleJump)
        {
            Jump(doubleJumpForce);
            canDoubleJump = false;
        }
    }

    private void HandleJumpRelease()
    {
        if (rb.linearVelocity.y > 0 && isJumping)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * jumpReleaseMultiplier, rb.linearVelocity.z);
        }
    }

    private void Jump(float jumpStrength)
    {
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        isJumping = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z) + _groundVelocity;
        rb.AddForce(transform.up * jumpStrength, ForceMode.Impulse);

        if (moveInput.sqrMagnitude > 0.01f)
        {
            ApplyJumpForwardBoost();
        }
        
        OnJumped?.Invoke();
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.y * -1, ForceMode.Acceleration);
        }
    }

    public void ResetDoubleJump()
    {
        if (!canDoubleJump && !isGrounded)
        canDoubleJump = true;
    }
    
    public void ApplyExternalForce(Vector3 direction, float force, bool resetVelocity)
    {
        if (resetVelocity)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        rb.AddForce(direction * force, ForceMode.Impulse);
        
        canDoubleJump = true;
        isJumping = true;
        OnLeftGround?.Invoke();
    }

    public void MultiplyVelocity(float multiplier)
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 newVelocity = horizontalVelocity * multiplier;
        rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
    }
    
    public Transform GetOrientation()
    {
        return orientation;
    }

    private void ApplyLandingDampening()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 dampenedVelocity = horizontalVelocity * landingVelocityDampening;
        rb.linearVelocity = new Vector3(dampenedVelocity.x, rb.linearVelocity.y, dampenedVelocity.z);
    }

    private void ApplyJumpForwardBoost()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 forwardDirection = orientation.forward;
            forwardDirection.y = 0;
            rb.AddForce(forwardDirection.normalized * jumpForwardBoost, ForceMode.Impulse);
        }
    }
    private void UpdateCurrentVelocityInKm()
    {
        currentVelocityInKm = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude * 3.6f;
    }
}