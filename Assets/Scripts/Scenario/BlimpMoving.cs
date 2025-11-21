using UnityEngine;

public class BlimpMoving : MonoBehaviour, ITimeSlowable, IResettable
{
    public float speed = 5.0f;
    public float turnSpeed = 15.0f;

    private float _currentSpeedMultiplier = 1.0f;
    private bool _isSlowed;

    // Variáveis para guardar o estado inicial
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private void Awake()
    {
        // Captura a posição antes de começar a se mover
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void Start()
    {
        if (TimeManipulationManager.Instance != null)
            TimeManipulationManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (TimeManipulationManager.Instance != null)
            TimeManipulationManager.Instance.Unregister(this);
    }

    void Update()
    {
        float timeFactor = Time.deltaTime * _currentSpeedMultiplier;

        transform.Translate(transform.forward * speed * timeFactor, Space.World);
        transform.Rotate(0, turnSpeed * timeFactor, 0);
    }

    // --- ITimeSlowable ---
    public void SlowDown(float slowPercentage)
    {
        _isSlowed = true;
        _currentSpeedMultiplier = 1f - (slowPercentage / 100f);
    }

    public void RestoreNormalTime()
    {
        _isSlowed = false;
        _currentSpeedMultiplier = 1.0f;
    }

    public void SetSlowDownColor(Color newColor) { }

    // --- IResettable ---
    public void ResetState()
    {
        // 1. Restaura posição física
        transform.position = _startPosition;
        transform.rotation = _startRotation;

        // 2. Garante que ele não volte "em câmera lenta"
        RestoreNormalTime();
    }
}