// Assets/Scripts/PlayerMovementController.cs

using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    #region Events
    public event Action OnGroundLanded;
    public event Action OnLeftGround;
    public event Action OnJumped;
    public event Action OnDoubleJumpGained;
    public event Action OnDoubleJumpUsed;
    public event Action<float> OnHorizontalVelocityChanged;
    #endregion

    #region State
    [Header("Estado Atual (Debug)")]
    public bool isGrounded;
    public bool canDoubleJump;
    [SerializeField] private bool isJumping;
    private float _horizontalSpeed;
    #endregion

    #region Dependencies
    [Header("Dependências")]
    [SerializeField] private Transform orientation;
    [SerializeField] private GrapplingHookController grapplingHookController;
    [SerializeField] private TextMeshProUGUI velocityText, distanceText;
    private Rigidbody _rigidbody;
    private Vector2 _moveInput;
    #endregion

    #region Movement Settings
    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float maxMoveSpeed = 30f;
    [SerializeField] private float maxGrappleMoveSpeed = 150f;
    [SerializeField] private float airMultiplier = 0.6f;
    [SerializeField] private float groundMultiplier = 2.0f;
    [SerializeField] private float airControlMultiplier = 5f;
    #endregion

    #region Drag Settings
    [Header("Configurações de Atrito (Drag)")]
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 2f;
    [SerializeField] private float grappleAirDrag = 0.5f;
    [SerializeField] private float highSpeedAirDragMultiplier = 1.2f;
    [SerializeField] private float highSpeedThreshold = 55.5f; // 200 km/h
    private float _baseAirDrag;
    #endregion

    #region Jump Settings
    [Header("Configurações de Pulo")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float doubleJumpForce = 14f;
    [SerializeField] private bool allowDoubleJumpFromGround = false;
    [SerializeField] private float jumpForwardBoost = 5f;
    [SerializeField] private float jumpReleaseMultiplier = 0.5f;
    [SerializeField, Range(0f, 1f)] private float landingVelocityDampening = 0.9f;
    #endregion
    
    #region Gravity
    [Header("Gravidade")]
    [SerializeField] private float gravityMultiplier = 2.5f;
    #endregion

    #region Timers & Buffers
    [Header("Coyote Time & Jump Buffer")]
    [SerializeField] private float coyoteTimeDuration = 0.1f;
    [SerializeField] private float jumpBufferDuration = 0.1f;
    [SerializeField] private float bunnyHopWindow = 0.1f;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private float _timeSinceLanded;
    #endregion

    #region Ground Check
    [Header("Verificação de Chão")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundCheckLayer;
    private Rigidbody _currentPlatformRb;
    private Vector3 _lastPlatformPosition;
    #endregion
    
    private const float METERS_PER_SECOND_TO_KM_PER_HOUR = 3.6f;

    public Rigidbody Rb => _rigidbody;

    #region Unity Lifecycle
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
        _baseAirDrag = airDrag;
    }

    private void OnEnable()
    {
        InputManager.Instance.OnMove += SetMoveInput;
        InputManager.Instance.OnJumpPerformed += ProcessJumpRequest;
        InputManager.Instance.OnJumpCanceled += HandleJumpRelease;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnMove -= SetMoveInput;
        InputManager.Instance.OnJumpPerformed -= ProcessJumpRequest;
        InputManager.Instance.OnJumpCanceled -= HandleJumpRelease;
    }

    private void Update()
    {
        UpdateTimers();
        UpdateDebugUI();
    }

    private void FixedUpdate()
    {
        HandleGroundCheck();
        HandleMovement();
        ApplyDrag();
        ApplyExtraGravity();
        LimitVelocity();
        BroadcastHorizontalVelocity();
    }
    #endregion

    #region Event Handlers
    private void SetMoveInput(Vector2 input) => _moveInput = input;

    private void UpdateTimers()
    {
        _coyoteTimeCounter = isGrounded ? coyoteTimeDuration : _coyoteTimeCounter - Time.deltaTime;
        _jumpBufferCounter -= Time.deltaTime;
        _timeSinceLanded += Time.deltaTime;
    }
    #endregion
    
    #region Core Logic (FixedUpdate)
    private void HandleGroundCheck()
    {
        bool wasGrounded = isGrounded;
        float rayOriginY = transform.position.y + (playerHeight * 0.5f);
        Vector3 rayOrigin = new Vector3(transform.position.x, rayOriginY, transform.position.z);
        float rayDistance = playerHeight + 0.2f;

        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, rayDistance, groundCheckLayer);

        UpdatePlatform(hitInfo);

        if (!wasGrounded && isGrounded) HandleLanding();
        if (wasGrounded && !isGrounded) HandleLeavingGround();
    }
    
    private void UpdatePlatform(RaycastHit hitInfo)
    {
        if (isGrounded && hitInfo.rigidbody != null)
        {
            if (_currentPlatformRb != hitInfo.rigidbody)
            {
                _currentPlatformRb = hitInfo.rigidbody;
                _lastPlatformPosition = _currentPlatformRb.position;
            }
            ApplyPlatformMovement();
        }
        else
        {
            _currentPlatformRb = null;
        }
    }

    private void ApplyPlatformMovement()
    {
        if (_currentPlatformRb == null) return;
        
        Vector3 platformDelta = _currentPlatformRb.position - _lastPlatformPosition;
        if (platformDelta != Vector3.zero)
        {
            MovingPlatform mp = _currentPlatformRb.GetComponent<MovingPlatform>();
            float playerInfluence = mp != null ? mp.playerInfluence : 0.69f;
            _rigidbody.position += platformDelta * playerInfluence;
        }
        _lastPlatformPosition = _currentPlatformRb.position;
    }

    private void HandleLanding()
    {
        _timeSinceLanded = 0f;
        isJumping = false;
        
        if (canDoubleJump)
        {
            canDoubleJump = false;
            OnDoubleJumpUsed?.Invoke();
        }

        float speedInKmh = _horizontalSpeed * METERS_PER_SECOND_TO_KM_PER_HOUR;
        if (isJumping && speedInKmh <= 100f)
        {
            ApplyLandingDampening();
        }
        
        OnGroundLanded?.Invoke();
        
        if (_jumpBufferCounter > 0f) PerformJump(jumpForce);
    }
    
    private void HandleLeavingGround()
    {
        if (!isJumping)
        {
            OnLeftGround?.Invoke();
        }
    }
    
    private void HandleMovement()
    {
        if (grapplingHookController.IsGrappling) return;

        if (isGrounded) ApplyGroundMovement();
        else ApplyAirMovement();
    }

    private void ApplyDrag()
    {
        if (grapplingHookController.IsGrappling)
        {
            _rigidbody.linearDamping = grappleAirDrag;
            return;
        }

        if (isGrounded && _timeSinceLanded > bunnyHopWindow)
        {
            _rigidbody.linearDamping = groundDrag;
        }
        else
        {
            airDrag = _horizontalSpeed > highSpeedThreshold ? _baseAirDrag * highSpeedAirDragMultiplier : _baseAirDrag;
            _rigidbody.linearDamping = airDrag;
        }
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded)
        {
            float gravity = -Physics.gravity.y * gravityMultiplier;
            _rigidbody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        }
    }

    private void LimitVelocity()
    {
        float currentMaxSpeed = grapplingHookController.IsGrappling ? maxGrappleMoveSpeed : maxMoveSpeed;
        float maxSpeedInMps = currentMaxSpeed / METERS_PER_SECOND_TO_KM_PER_HOUR;

        Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeedInMps)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * maxSpeedInMps;
            _rigidbody.linearVelocity = new Vector3(limitedVelocity.x, _rigidbody.linearVelocity.y, limitedVelocity.z);
        }
    }

    private void BroadcastHorizontalVelocity()
    {
        Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        _horizontalSpeed = horizontalVelocity.magnitude;
        OnHorizontalVelocityChanged?.Invoke(_horizontalSpeed);
    }
    #endregion
    
    #region Movement
    private void ApplyGroundMovement()
    {
        if (_moveInput.sqrMagnitude < 0.01f) return;

        Vector3 moveDirection = (orientation.forward * _moveInput.y + orientation.right * _moveInput.x).normalized;
        _rigidbody.AddForce(moveDirection * moveSpeed * 10f * groundMultiplier, ForceMode.Force);
    }

    private void ApplyAirMovement()
    {
        if (_moveInput.sqrMagnitude < 0.01f) return;

        Vector3 moveDirection = (orientation.forward * _moveInput.y + orientation.right * _moveInput.x).normalized;
        Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        
        float angle = Vector3.Angle(horizontalVelocity.normalized, moveDirection);
        float angleBoost = Mathf.InverseLerp(0f, 90f, angle);
        float lateralInfluence = Mathf.Lerp(1f, airControlMultiplier, angleBoost);

        _rigidbody.AddForce(moveDirection * moveSpeed * 10f * airMultiplier * lateralInfluence, ForceMode.Force);
    }
    #endregion
    
    #region Jumping
    private void ProcessJumpRequest()
    {
        _jumpBufferCounter = jumpBufferDuration;

        if (grapplingHookController.IsGrappling) return;

        if (CanPerformJump())
        {
            PerformJump(jumpForce);
            if (allowDoubleJumpFromGround) GainDoubleJump();
        }
        else if (CanPerformDoubleJump())
        {
            PerformDoubleJump();
        }
    }
    
    private void HandleJumpRelease()
    {
        if (_rigidbody.linearVelocity.y > 0 && isJumping)
        {
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, _rigidbody.linearVelocity.y * jumpReleaseMultiplier, _rigidbody.linearVelocity.z);
        }
    }

    private bool CanPerformJump() => _coyoteTimeCounter > 0f;
    private bool CanPerformDoubleJump() => canDoubleJump || (CheatManager.Instance != null && CheatManager.Instance.IsInfiniteDoubleJumpActive);

    private void PerformJump(float force)
    {
        _coyoteTimeCounter = 0f;
        _jumpBufferCounter = 0f;
        isJumping = true;

        Vector3 platformVelocity = _currentPlatformRb ? _currentPlatformRb.linearVelocity : Vector3.zero;
        _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z) + platformVelocity;
        _rigidbody.AddForce(transform.up * force, ForceMode.Impulse);

        if (_moveInput.sqrMagnitude > 0.01f) ApplyJumpForwardBoost();
        
        OnJumped?.Invoke();
    }
    
    private void PerformDoubleJump()
    {
        PerformJump(doubleJumpForce);
        if (CheatManager.Instance == null || !CheatManager.Instance.IsInfiniteDoubleJumpActive)
        {
            canDoubleJump = false;
            OnDoubleJumpUsed?.Invoke();
        }
    }

    private void ApplyJumpForwardBoost()
    {
        Vector3 forwardDirection = (orientation.forward * _moveInput.y + orientation.right * _moveInput.x).normalized;
        _rigidbody.AddForce(forwardDirection * jumpForwardBoost, ForceMode.Impulse);
    }
    
    private void GainDoubleJump()
    {
        if (!canDoubleJump)
        {
            canDoubleJump = true;
            OnDoubleJumpGained?.Invoke();
        }
    }
    #endregion

    #region Public API
    public void ResetDoubleJump()
    {
        if (!isGrounded)
        {
            GainDoubleJump();
        }
    }
    
    public void ApplyExternalForce(Vector3 direction, float force, bool resetVelocity)
    {
        if (resetVelocity) _rigidbody.linearVelocity = Vector3.zero;
        
        _rigidbody.AddForce(direction * force, ForceMode.Impulse);
        
        GainDoubleJump();
        isJumping = true;
        OnLeftGround?.Invoke();
    }

    public void MultiplyVelocity(float multiplier)
    {
        Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        Vector3 newVelocity = horizontalVelocity * multiplier;
        _rigidbody.linearVelocity = new Vector3(newVelocity.x, _rigidbody.linearVelocity.y, newVelocity.z);
    }
    
    public Transform GetOrientation()
    {
        return orientation;
    }
    #endregion
    
    #region Utility & Debug
    private void ApplyLandingDampening()
    {
        Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        Vector3 dampenedVelocity = horizontalVelocity * landingVelocityDampening;
        _rigidbody.linearVelocity = new Vector3(dampenedVelocity.x, _rigidbody.linearVelocity.y, dampenedVelocity.z);
    }

    private void UpdateDebugUI()
    {
        if (distanceText != null)
        {
            distanceText.text = "Distancia: " + grapplingHookController.grappleDistance.ToString("F2");
        }
        
        if (velocityText != null)
        {
            float speedInKmh = _horizontalSpeed * METERS_PER_SECOND_TO_KM_PER_HOUR;
            velocityText.text = "Velocidade: " + speedInKmh.ToString("F2");
        }
    }
    #endregion
}