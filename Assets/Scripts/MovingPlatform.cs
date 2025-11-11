using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour, ITimeSlowable, IResettable, IGrappleable
{
    [Header("Configurações de Patrulha")]
    [SerializeField] private Transform targetA;
    [SerializeField] private Transform targetB;
    [Tooltip("A velocidade da plataforma em unidades por segundo.")]
    public float speed = 3f;
    [Tooltip("Fator de influência no movimento do jogador quando está sobre a plataforma.")]
    public float playerInfluence = 0.69f;

    [Header("Grapple & Visuals")]
    [SerializeField] private bool canBeGrappledWhileMoving = true;
    [SerializeField] private Color slowDownColor = Color.cyan;

    private Rigidbody _rb;
    private Renderer _renderer;
    private Color _originalColor;

    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private float _journeyLength;
    private float _journeyTravelled;

    private bool _isSlowed = false;
    private Vector3 _initialPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        if (_renderer != null) _originalColor = _renderer.material.color;

        _rb.useGravity = false;
        _rb.freezeRotation = true;
        _rb.isKinematic = true;

        _initialPosition = transform.position;
    }

    private void Start()
    {
        if (targetA == null || targetB == null)
        {
            Debug.LogError("Os alvos de patrulha (Target A e Target B) não foram configurados!", this);
            enabled = false;
            return;
        }
        InitializeJourney();
        TimeManipulationManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.Unregister(this);
        }
    }

    private void FixedUpdate()
    {
        if (!_isSlowed)
        {
            UpdateMovement();
        }
    }

    private void UpdateMovement()
    {
        if (_journeyLength <= 0) return;

        _journeyTravelled += speed * Time.fixedDeltaTime;

        if (_journeyTravelled >= _journeyLength)
        {
            float overshoot = _journeyTravelled - _journeyLength;
            SwapEndpoints();
            _journeyTravelled = overshoot;
        }

        float fractionOfJourney = Mathf.Clamp01(_journeyTravelled / _journeyLength);
        Vector3 newPosition = Vector3.Lerp(_startPoint, _endPoint, fractionOfJourney);
        _rb.MovePosition(newPosition);
    }

    private void InitializeJourney()
    {
        transform.position = _initialPosition;
        _startPoint = targetA.position;
        _endPoint = targetB.position;
        _journeyLength = Vector3.Distance(_startPoint, _endPoint);
        _journeyTravelled = 0f;
    }

    private void SwapEndpoints()
    {
        Vector3 temp = _startPoint;
        _startPoint = _endPoint;
        _endPoint = temp;
    }

    #region ITimeSlowable Implementation
    public void SlowDown(float slowPercentage)
    {
        if (_isSlowed) return;
        _isSlowed = true;
        if (_renderer != null) _renderer.material.color = slowDownColor;
    }

    public void RestoreNormalTime()
    {
        if (!_isSlowed) return;
        _isSlowed = false;
        if (_renderer != null) _renderer.material.color = _originalColor;
    }

    public void SetSlowDownColor(Color newColor)
    {
        slowDownColor = newColor;
    }
    #endregion

    #region IResettable Implementation
    public void ResetState()
    {
        InitializeJourney();
        RestoreNormalTime();
    }
    #endregion

    #region IGrappleable Implementation
    public bool CanBeGrappledNow()
    {
        return canBeGrappledWhileMoving || _isSlowed;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (targetA != null && targetB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(targetA.position, targetB.position);
            Gizmos.DrawWireSphere(targetA.position, 0.5f);
            Gizmos.DrawWireSphere(targetB.position, 0.5f);
        }
    }
}