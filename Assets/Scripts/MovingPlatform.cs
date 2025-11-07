using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour, ITimeSlowable, IResettable
{
    [Header("Configurações de Patrulha")]
    [SerializeField] private Transform targetA;
    [SerializeField] private Transform targetB;
    public float speed = 3f, playerInfluence = 0.69f;

    [Header("Visual")]
    [SerializeField] private Color slowDownColor = Color.cyan;
    
    private Rigidbody _rb;
    private Transform _currentTarget;
    private Vector3 _startPosition;
    private Vector3 _savedVelocity;
    private Renderer _renderer;
    private Color _originalColor;
    private bool _isSlowed = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        if (_renderer != null) _originalColor = _renderer.material.color;

        _rb.useGravity = false;
        _rb.freezeRotation = true;
        _rb.isKinematic = false;
        _startPosition = transform.position;
    }

    private void Start()
    {
        if (targetA == null || targetB == null)
        {
            enabled = false;
            return;
        }
        _currentTarget = targetB;
        TimeManipulationManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (_isSlowed) RestoreNormalTime();
        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.Unregister(this);
        }
    }

    private void FixedUpdate()
    {
        if (_isSlowed) return;
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (_currentTarget.position - _rb.position).normalized;
        _rb.linearVelocity = direction * speed;

        if (Vector3.Distance(_rb.position, _currentTarget.position) < 0.5f)
        {
            _currentTarget = (_currentTarget == targetA) ? targetB : targetA;
        }
    }

    public void SlowDown(float slowPercentage)
    {
        if (_isSlowed) return;
        _isSlowed = true;
        _savedVelocity = _rb.linearVelocity;
        float slowFactor = 1.0f - (slowPercentage / 100.0f);
        _rb.linearVelocity = _savedVelocity * slowFactor;
        if (_renderer != null) _renderer.material.color = slowDownColor;
    }

    public void RestoreNormalTime()
    {
        if (!_isSlowed) return;
        _isSlowed = false;
        _rb.linearVelocity = _savedVelocity;
        if (_renderer != null) _renderer.material.color = _originalColor;
    }

    public void SetSlowDownColor(Color newColor)
    {
        slowDownColor = newColor;
    }
    
    public void ResetState()
    {
        transform.position = _startPosition;
        _currentTarget = targetB;
    }

    private void OnDrawGizmos()
    {
        if (targetA != null && targetB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(targetA.position, targetB.position);
        }
    }
}