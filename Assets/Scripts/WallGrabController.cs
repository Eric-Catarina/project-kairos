using UnityEngine;

public class WallGrabController : MonoBehaviour
{
    [Header("Configurações de Agarrar na Parede")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float wallSlideSpeed = -1f;
    [SerializeField] private float wallJumpForceX = 10f;
    [SerializeField] private float wallJumpForceY = 15f;
    [SerializeField] private float wallGrabDuration = 1.5f;
    [SerializeField] private float groundCheckGracePeriod = 0.2f;

    [Header("Verificação de Parede")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform orientation;

    // Referências
    private Rigidbody rb;
    private PlayerMovementController playerMovement;
    private InputManager inputManager;
    
    // Estado da mecânica
    public bool IsWallGrabbing { get; private set; }
    private bool hasWallGrabAvailable = true;
    private float timeSinceGrounded;
    private float timeSinceWallGrab;
    private bool canWallJump = false;
    private Vector3 wallNormal;
    private float moveInputX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovementController>();
        inputManager = InputManager.Instance;
    }

    private void OnEnable()
    {
        inputManager.OnMove += SetMoveInput;
        inputManager.OnJumpPerformed += HandleJumpInput;
    }

    private void OnDisable()
    {
        if (inputManager == null) return;
        inputManager.OnMove -= SetMoveInput;
        inputManager.OnJumpPerformed -= HandleJumpInput;
    }

    private void FixedUpdate()
    {
        if (IsWallGrabbing && !playerMovement.isJumping)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallSlideSpeed, rb.linearVelocity.z);
    }

    private void Update()
    {
        CheckForWall();

        if (IsWallGrabbing)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallSlideSpeed, rb.linearVelocity.z);
        }
        timeSinceGrounded += Time.deltaTime;
    }

    private void SetMoveInput(Vector2 input)
    {
        moveInputX = input.x;
    }

   private void CheckForWall()
{
    RaycastHit hit;
    bool wallDetected = Physics.Raycast(transform.position, orientation.forward, out hit, wallCheckDistance, wallLayer);

    if (wallDetected && !playerMovement.isGrounded && timeSinceGrounded > groundCheckGracePeriod)
    {
        if (!IsWallGrabbing)
        {
            timeSinceWallGrab = 0f;
            playerMovement.DisableDoubleJump(); 
            playerMovement.Rb.linearVelocity = new Vector3(0, playerMovement.Rb.linearVelocity.y, 0);
            hasWallGrabAvailable = false; 
        }
        IsWallGrabbing = true;
        wallNormal = hit.normal;
        rb.useGravity = false;

        if (!canWallJump)
        {
            canWallJump = true;
        }
    }
    else
    {
        IsWallGrabbing = false;
        rb.useGravity = true;
    }

    if (IsWallGrabbing)
    {
        timeSinceWallGrab += Time.deltaTime;
        if (timeSinceWallGrab >= wallGrabDuration)
        {
            IsWallGrabbing = false;
            rb.useGravity = true;
            canWallJump = false;
        }
    }

    if (playerMovement.isGrounded)
    {
        canWallJump = true;
        timeSinceGrounded = 0f;
    }
}

    private void HandleJumpInput()
    {
        if (IsWallGrabbing && canWallJump)
        {
            WallJump();
        }
    }

    private void WallJump()
    {
        // Reseta a gravidade
        rb.useGravity = true;
        IsWallGrabbing = false;
        canWallJump = false;

        rb.linearVelocity = Vector3.zero;

        Vector3 jumpDirection = (wallNormal * wallJumpForceX) + (Vector3.up * wallJumpForceY);
        rb.AddForce(jumpDirection, ForceMode.Impulse);
    }

    public void ResetWallGrab()
    {
        hasWallGrabAvailable = true;
    }
}