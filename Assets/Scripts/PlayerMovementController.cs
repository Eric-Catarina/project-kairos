// Local: Assets/Scripts/PlayerMovementController.cs

using TMPro;
using System;

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Estado Atual")]
    public bool isGrounded;
    [SerializeField] private bool canDoubleJump;
    private bool wasGroundedLastFrame; // novo campo para rastrear o estado anterior

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

    [Header("Referências")]
    [SerializeField] private Transform orientation;
    [SerializeField] private GrapplingHookController grapplingHookController;
    public GameObject velocityParticle;    
    public TextMeshProUGUI debugText;

    private Rigidbody rb;
    private Vector2 moveInput;

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
        // Invoca o evento apenas na transição de "no ar" para "no chão"
        if (!wasGrounded && isGrounded)
        {
            OnGroundLanded?.Invoke();
        }
    }

    private void ApplyDrag()
    {
        // CORRIGIDO: A propriedade correta é 'drag', não 'linearDamping'.
        rb.linearDamping = isGrounded ? groundDrag : airDrag;
    }

    private void MovePlayer()
    {
        if (grapplingHookController.IsGrappling) return; 

        Vector3 moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        moveDirection.Normalize();

        float forceMultiplier = isGrounded ? 1f : airMultiplier;
        rb.AddForce(moveDirection * moveSpeed * 10f * forceMultiplier, ForceMode.Force);
    }

    private void LimitVelocity()
    {


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
        // CORRIGIDO: Usando 'linearVelocity' para resetar a velocidade vertical.
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded )
        {
            rb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.y * -1, ForceMode.Acceleration);
        }
    }

    public void EnableDoubleJump()
    {
        canDoubleJump = true;
    }
}