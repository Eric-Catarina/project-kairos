// Local: Assets/Scripts/PlayerMovementController.cs

using TMPro;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    public event Action OnGroundLanded;

    [Header("Estado Atual")]
    public bool isGrounded;
    [SerializeField] private bool canDoubleJump;

    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxMoveSpeed = 30f;
    [SerializeField] private float maxGrappleMoveSpeed = 150f;
    [SerializeField] private float vfxMinMoveSpeed = 150f;
    [SerializeField] private float airMultiplier = 0.6f;

    [Header("Configurações de Atrito (Drag)")]
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 2f;
    [SerializeField] private float grappleAirDrag = 0.5f;

    [Header("Configurações de Pulo")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float doubleJumpForce = 14f;
    [SerializeField] private float gravityMultiplier = 2.5f;

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
    
    [Header("Ajuste Fino do Controle Aéreo")]
    [Tooltip("A velocidade que o input do jogador tenta atingir no ar. Impede que o input acelere o jogador além da velocidade base de corrida.")]
    [SerializeField] private float airControlTargetSpeed = 7f;

    [Header("Verificação de Chão")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Referências")]
    [SerializeField] private Transform orientation;
    [SerializeField] private GrapplingHookController grapplingHookController;
    public GameObject velocityParticle;
    public TextMeshProUGUI velocityText, distanceText;

    private Rigidbody rb;
    private Vector2 moveInput;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float timeSinceLanded;
    private bool isJumping;

    public Rigidbody Rb => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        airControlTargetSpeed = moveSpeed * 2; // Garante que o valor inicial seja o mesmo da velocidade de movimento
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
        CheckGroundedStatus();
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

        float velocityInKm = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude * 3.6f;
        velocityText.text = "Velocidade: " + velocityInKm.ToString("F2");
    }

    private void CheckGroundedStatus()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        
        if (isGrounded)
        {
            isJumping = false;
        }

        if (!wasGrounded && isGrounded)
        {
            timeSinceLanded = 0f;
            canDoubleJump = false;
            OnGroundLanded?.Invoke();

            if (jumpBufferCounter > 0f)
            {
                Jump(jumpForce);
            }
        }
        
        if (wasGrounded && !isGrounded && !isJumping)
        {
            coyoteTimeCounter = coyoteTimeDuration;
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
    }

    private void MovePlayer()
    {
        if (grapplingHookController.IsGrappling) return;

        Vector3 moveDirection = (orientation.forward * moveInput.y + orientation.right * moveInput.x).normalized;

        if (isGrounded)
        {
            // No chão, a lógica é simples: aplicamos força, e o atrito alto a equilibra.
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        }
        else // No Ar
        {
            // PONTO-CHAVE DA SOLUÇÃO:
            // Verificamos a velocidade atual na direção do input.
            Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            float speedInInputDirection = Vector3.Dot(currentVelocity, moveDirection);

            // Só aplicamos força se a velocidade na direção do input for menor que a nossa velocidade alvo.
            // Isso previne a aceleração extra ao pular, mas ainda permite que o jogador mude de direção no ar (air-strafe).
            if (speedInInputDirection < airControlTargetSpeed)
            {
                // A força aplicada aqui é calculada para ser forte o suficiente para dar bom controle.
                rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            }
        }
    }

    private void LimitVelocity()
    {
        float currentMaxSpeed = grapplingHookController.IsGrappling ? maxGrappleMoveSpeed : maxMoveSpeed;
        float currentSpeedInKm = rb.linearVelocity.magnitude * 3.6f;

        if (currentSpeedInKm > currentMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * (currentMaxSpeed / 3.6f);
        }

        if (velocityParticle != null)
        {
            velocityParticle.SetActive(rb.linearVelocity.magnitude > vfxMinMoveSpeed);
        }
    }

    private void HandleJumpInput()
    {
        if (grapplingHookController.IsGrappling) return;

        jumpBufferCounter = jumpBufferDuration;

        if (coyoteTimeCounter > 0f || isGrounded)
        {
            Jump(jumpForce);
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

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.y * -1, ForceMode.Acceleration);
        }
    }

    public void EnableDoubleJump()
    {
        canDoubleJump = true;
    }
}