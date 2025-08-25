// Local: Assets/Scripts/PlayerMovementController.cs

using TMPro;
using System;
using DG.Tweening;

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Estado Atual")]
    public bool isGrounded;
    [SerializeField] private bool canDoubleJump;
    [SerializeField] private bool isSliding; // Novo estado para o deslize

    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 7f, maxMoveSpeed = 30f, vfxMinMoveSpeed = 150f;
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 0.5f;
    [SerializeField] private float airMultiplier = 0.6f;

    [Header("Configurações de Pulo")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float gravityMultiplier = 2.5f;

    [Header("Verificação de Chão")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;
    public event Action OnGroundLanded;

    [Header("Configurações de Deslize")]
    [Tooltip("Velocidade mínima para iniciar o deslize ao aterrissar.")]
    [SerializeField] private float slideThresholdSpeed = 20f;
    [Tooltip("Duração total do deslize em segundos.")]
    [SerializeField] private float slideDuration = 1f;
    [Tooltip("Atrito aplicado durante o deslize.")]
    [SerializeField] private float slideDrag = 1f;
    [Tooltip("Janela de tempo no final do deslize para conseguir o boost (em segundos).")]
    [SerializeField] private float slideBoostWindow = 0.1f;
    [Tooltip("Força do impulso concedido no pulo com boost.")]
    [SerializeField] private float slideBoostForce = 25f;

    [Header("Referências")]
    [SerializeField] private Transform orientation;
    [SerializeField] private GrapplingHookController grapplingHookController;
    [SerializeField] private GameObject playerVisuals;
    public GameObject velocityParticle;
    public TextMeshProUGUI debugText;

    private Rigidbody rb;
    private Vector2 moveInput;
    private float slideTimer; // Timer para controlar a duração do deslize

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
        HandleSlide(); // Nova função para gerenciar o timer do deslize
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

        // Invoca o evento e verifica se deve iniciar o deslize
        if (!wasGrounded && isGrounded)
        {
            OnGroundLanded?.Invoke();
            // Inicia o deslize se a velocidade for alta o suficiente
            if (rb.linearVelocity.magnitude > slideThresholdSpeed)
            {
                StartSlide();
            }
        }
    }

    private void ApplyDrag()
    {
        if (isSliding)
        {
            rb.linearDamping = slideDrag;
        }
        else
        {
            rb.linearDamping = isGrounded ? groundDrag : airDrag;
        }
    }

    private void MovePlayer()
    {
        // Desativa o controle de movimento durante o grapple ou o deslize
        if (grapplingHookController.IsGrappling || isSliding) return;

        Vector3 moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        moveDirection.Normalize();

        float forceMultiplier = isGrounded ? 1f : airMultiplier;
        rb.AddForce(moveDirection * moveSpeed * 10f * forceMultiplier, ForceMode.Force);
    }

    private void LimitVelocity()
    {
        if (grapplingHookController.IsGrappling) return; // Não limita a velocidade durante o grapple

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

        // Lógica para o pulo com boost
        if (isSliding && slideTimer <= slideBoostWindow)
        {
            BoostJump();
            return; // Sai da função para não executar o pulo normal
        }

        if (isGrounded) // Permite o pulo normal (mesmo durante o slide, fora da janela de boost)
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
        if (isSliding) StopSlide(); // Para o slide se pular no meio dele

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    // --- NOVAS FUNÇÕES PARA O DESLIZE ---

    private void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        playerVisuals.transform.DOLocalRotate(new Vector3(-40f, 0f, 0f), 0.2f).SetEase(Ease.OutQuad);
    }

    private void StopSlide()
    {
        isSliding = false;
        slideTimer = 0f;
    }

    private void HandleSlide()
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
        StopSlide();

        // Pulo normal
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        // Aplica o boost na direção do movimento atual
        Vector3 boostDirection = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized;
        if (boostDirection == Vector3.zero) // Caso o jogador pare completamente, usa a orientação
        {
            boostDirection = orientation.forward;
        }

        rb.AddForce(boostDirection * slideBoostForce, ForceMode.Impulse);
        playerVisuals.transform.DOLocalRotate(new Vector3(40f, 0f, 0f), 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            playerVisuals.transform.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.OutQuad);
        });


    }
    
    // ------------------------------------

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