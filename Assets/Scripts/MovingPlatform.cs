// Local: Assets/Scripts/MovingPlatform.cs

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour, ITimeSlowable
{
    [Header("Configurações de Patrulha")]
    [Tooltip("O ponto de partida da patrulha.")]
    [SerializeField] private Transform targetA;
    [Tooltip("O ponto final da patrulha.")]
    [SerializeField] private Transform targetB;
    [Tooltip("A velocidade de movimento da plataforma.")]
    public float speed = 3f, playerInfluence = 0.69f;

    [Header("Visual")]
    [SerializeField] private Color slowDownColor = Color.cyan;
    
    private Rigidbody _rb;
    private Transform _currentTarget;
    private Vector3 _savedVelocity;
    private Renderer _renderer;
    private Color _originalColor;

    private bool _isSlowed = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();

        if (_renderer != null) _originalColor = _renderer.material.color;

        // Configuração crucial do Rigidbody para plataformas
        _rb.useGravity = false;
        _rb.freezeRotation = true;
        _rb.isKinematic = false; // Deve ser dinâmico para interagir com o jogador
    }

    private void Start()
    {
        if (targetA == null || targetB == null)
        {
            Debug.LogError("Alvos de patrulha não configurados!", this);
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
        if (_isSlowed) return; // O movimento é controlado pelos métodos de slow

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

        // A velocidade é recalculada no próximo FixedUpdate, ou podemos restaurar a salva
        _rb.linearVelocity = _savedVelocity;

        if (_renderer != null) _renderer.material.color = _originalColor;
    }

    public void SetSlowDownColor(Color newColor)
    {
        slowDownColor = newColor;
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