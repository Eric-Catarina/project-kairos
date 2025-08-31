// Local: Assets/Scripts/PlayerMovementController.cs

using TMPro;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    public event Action OnSlideStart;
    public event Action OnSlideEnd;
    public event Action OnBoostJump;
    public event Action OnGroundLanded;

    [Header("Estado Atual")]
    public bool isGrounded;
    [SerializeField] private bool canDoubleJump;
    [SerializeField] private bool isSliding;

    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxMoveSpeed = 30f;
    [SerializeField] private float vfxMinMoveSpeed = 150f;
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 0.5f;
    [SerializeField] private float airMultiplier = 0.6f;

    [Header("Configurações de Pulo")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float gravityMultiplier = 2.5f;

    [Header("Verificação de Chão")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Configurações de Deslize")]
    [Tooltip("Velocidade mínima para iniciar o deslize ao aterrissar.")]
    [SerializeField] private float slideThresholdSpeed = 20f;
    [Tooltip("Duração total do deslize em segundos.")]
    [SerializeField] private float slideDuration = 1f;
    [Tooltip("Atrito aplicado durante o deslize.")]
    [SerializeField] private float slideDrag = 1f;
    [Tooltip("Força com que o jogador pode controlar a direção durante o deslize.")]
    [SerializeField] private float slideControlForce = 5f;
    [Tooltip("Janela de tempo no final do deslize para conseguir o boost (em segundos).")]
    [SerializeField] private float slideBoostWindow = 0.1f;
    [Tooltip("Força do impulso concedido no pulo com boost.")]
    [SerializeField] private float slideBoostForce = 25f;

    [Header("Referências")]
    [SerializeField] private Transform orientation;
    [SerializeField] private GrapplingHookController grapplingHookController;
    public GameObject velocityParticle;
    public TextMeshProUGUI debugText;

    private Rigidbody rb;
    private Vector2 moveInput;
    private float slideTimer;

    public Rigidbody Rb => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        InputManager.Instance.OnMove += SetMoveInput;
        InputManager.Instance.OnJump += HandleJump;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnMove -= SetMoveInput;
        InputManager.Instance.OnJump -= HandleJump;
    }

    private void Update()
    {
        CheckGroundedStatus();
        HandleSlideTimer();
        ApplyDrag();
        LimitVelocity();
        debugText.text = "Velocidade: " + rb.linearVelocity.magnitude.ToString("F2");
    }

    private void FixedUpdate()
    {
        MovePlayer();
        ApplyExtraGravity();
    }

    private void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void CheckGroundedStatus()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        
        if (isGrounded)
        {
            canDoubleJump = false;
        }

        if (!wasGrounded && isGrounded)
        {
            OnGroundLanded?.Invoke();
            if (rb.linearVelocity.magnitude > slideThresholdSpeed)
            {
                StartSlide();
            }
        }
    }

    private void ApplyDrag()
    {
        if (grapplingHookController.IsGrappling)
        {
            rb.linearDamping = 0; // Remove drag during grapple for smoother swings
            return;
        }
        
        rb.linearDamping = isSliding ? slideDrag : (isGrounded ? groundDrag : airDrag);
    }

    private void MovePlayer()
    {
        if (grapplingHookController.IsGrappling) return;

        Vector3 moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        moveDirection.Normalize();

        if (isSliding)
        {
            // Permite um controle direcional limitado durante o deslize
            rb.AddForce(moveDirection * slideControlForce, ForceMode.Force);
        }
        else
        {
            float forceMultiplier = isGrounded ? 1f : airMultiplier;
            rb.AddForce(moveDirection * moveSpeed * 10f * forceMultiplier, ForceMode.Force);
        }
    }

    private void LimitVelocity()
    {
        if (grapplingHookController.IsGrappling) return;

        if (rb.linearVelocity.magnitude > maxMoveSpeed)
        {
            Vector3 limitedVelocity = rb.linearVelocity.normalized * maxMoveSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
        
        if (rb.linearVelocity.magnitude > vfxMinMoveSpeed)
        {
            velocityParticle.SetActive(true);
        }
        else
        {
            velocityParticle.SetActive(false);
        }
    }

    private void HandleJump()
    {
        if (grapplingHookController.IsGrappling) return;

        if (isSliding && slideTimer <= slideBoostWindow)
        {
            BoostJump();
            return;
        }

        if (isGrounded)
        {
            Jump();
        }
        else if (canDoubleJump)
        {
            Jump();
            canDoubleJump = false;
        }
    }

    private void Jump()
    {
        if (isSliding) StopSlide();
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    
    private void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        OnSlideStart?.Invoke(); // Dispara o evento!
    }

    private void StopSlide()
    {
        if (!isSliding) return;
        isSliding = false;
        slideTimer = 0f;
        OnSlideEnd?.Invoke(); // Dispara o evento!
    }

    private void HandleSlideTimer()
    {
        if (!isSliding) return;

        slideTimer -= Time.deltaTime;
        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void BoostJump()
    {
        OnBoostJump?.Invoke(); // Dispara o evento de boost!
        StopSlide();

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        Vector3 boostDirection = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized;
        if(boostDirection == Vector3.zero)
        {
            boostDirection = orientation.forward;
        }
        
        rb.AddForce(boostDirection * slideBoostForce, ForceMode.Impulse);
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded && !grapplingHookController.IsGrappling)
        {
            rb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.y * -1, ForceMode.Acceleration);
        }
    }

    public void EnableDoubleJump()
    {
        canDoubleJump = true;
    }
}