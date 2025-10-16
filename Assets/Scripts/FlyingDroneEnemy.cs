// Local: Assets/Scripts/Enemies/FlyingDroneEnemy.cs

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlyingDroneEnemy : MonoBehaviour, ITimeSlowable
{
    private enum DroneState
    {
        Patrolling,
        Chasing,
        Boosting
    }

    [Header("Referências")]
    [Tooltip("O Transform do jogador que o drone deve perseguir.")]
    private Transform playerTarget;

    [Header("Parâmetros de Detecção")]
    [SerializeField] private float detectionRadius = 25f;
    [SerializeField] private float loseSightRadius = 40f;
    [SerializeField] private float attackRange = 5f;

    [Header("Parâmetros de Movimento")]
    [SerializeField] private float patrolForce = 20f;
    [SerializeField] private float chaseForce = 40f;
    [SerializeField] private float boostForce = 80f;
    [Tooltip("Altura que o drone tenta manter acima do jogador durante a perseguição.")]
    [SerializeField] private float hoverHeight = 3f;
    [Tooltip("Força que estabiliza a altitude do drone.")]
    [SerializeField] private float hoverForce = 15f;
    [Tooltip("Quão rápido o drone rotaciona para encarar o alvo.")]
    [SerializeField] private float rotationSpeed = 5f;
    [Tooltip("Atrito linear para controlar a velocidade máxima.")]
    [SerializeField] private float linearDrag = 2f;

    [Header("Ciclo de Boost")]
    [SerializeField] private float boostDuration = 2f;
    [SerializeField] private float boostCooldown = 2f;

    [Header("Patrulha")]
    [SerializeField] private float patrolRadius = 20f;
    
    [Header("Feedback Visual")]
    [SerializeField] private Renderer droneRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color chaseColor = Color.yellow;
    [SerializeField] private Color boostColor = Color.red;

    private Rigidbody _rb;
    private DroneState _currentState;
    private Vector3 _startPosition;
    private Vector3 _patrolDestination;
    private Coroutine _boostCycleCoroutine;

    // --- ITimeSlowable Fields ---
    private bool _isSlowed = false;
    private Vector3 _savedVelocity;
    private Vector3 _savedAngularVelocity;
    private float _slowFactor;
    private Color _slowDownColor = Color.cyan;
    private Color _originalVisualColor;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.linearDamping = linearDrag;
        _startPosition = transform.position;
    }

    private void Start()
    {
        if (playerTarget == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) playerTarget = playerObject.transform;
            else { enabled = false; return; }
        }
        
        // Registro no Time Manipulation Manager
        TimeManipulationManager.Instance?.Register(this);
        
        SwitchState(DroneState.Patrolling);
    }

    private void OnDisable()
    {
        TimeManipulationManager.Instance?.Unregister(this);
    }

    private void FixedUpdate()
    {
        // Se o tempo está desacelerado, a lógica manual do ITimeSlowable assume
        if (_isSlowed)
        {
            HandleSlowMotionPhysics();
            return;
        }

        switch (_currentState)
        {
            case DroneState.Patrolling:
                MoveTowards(_patrolDestination, patrolForce);
                break;
            case DroneState.Chasing:
            case DroneState.Boosting:
                Vector3 targetPosition = playerTarget.position + Vector3.up * hoverHeight;
                float force = (_currentState == DroneState.Boosting) ? boostForce : chaseForce;
                MoveTowards(targetPosition, force);
                break;
        }
    }

    private void Update()
    {
        if (_isSlowed) return;

        // A lógica de transição de estados roda no Update
        switch (_currentState)
        {
            case DroneState.Patrolling:
                UpdatePatrolling();
                break;
            case DroneState.Chasing:
            case DroneState.Boosting:
                UpdateChasing();
                break;
        }
    }

    private void SwitchState(DroneState newState)
    {
        if (_currentState == newState) return;
        _currentState = newState;

        switch (_currentState)
        {
            case DroneState.Patrolling:
                if (_boostCycleCoroutine != null) StopCoroutine(_boostCycleCoroutine);
                _boostCycleCoroutine = null;
                SetNewPatrolDestination();
                if (droneRenderer != null) droneRenderer.material.color = normalColor;
                break;
            case DroneState.Chasing:
                if (_boostCycleCoroutine == null) _boostCycleCoroutine = StartCoroutine(BoostCycle());
                if (droneRenderer != null) droneRenderer.material.color = chaseColor;
                break;
            case DroneState.Boosting:
                if (droneRenderer != null) droneRenderer.material.color = boostColor;
                break;
        }
    }

    private void UpdatePatrolling()
    {
        if (Vector3.Distance(transform.position, playerTarget.position) <= detectionRadius)
        {
            SwitchState(DroneState.Chasing);
        }
        if (Vector3.Distance(transform.position, _patrolDestination) < 2f)
        {
            SetNewPatrolDestination();
        }
    }

    private void UpdateChasing()
    {
        if (Vector3.Distance(transform.position, playerTarget.position) > loseSightRadius)
        {
            SwitchState(DroneState.Patrolling);
        }
    }
    
    private void MoveTowards(Vector3 target, float force)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;
        
        // Aplica a força principal de movimento
        _rb.AddForce(directionToTarget * force);

        // Lógica de Hovering simples para manter a altitude
        float heightError = target.y - transform.position.y;
        _rb.AddForce(Vector3.up * heightError * hoverForce);
        
        // Rotaciona para encarar o jogador
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        _rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
    }

    private void SetNewPatrolDestination()
    {
        Vector3 randomPoint = _startPosition + Random.insideUnitSphere * patrolRadius;
        randomPoint.y = _startPosition.y + Random.Range(-patrolRadius / 4, patrolRadius / 4); // Variação de altura na patrulha
        _patrolDestination = randomPoint;
    }

    private IEnumerator BoostCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(boostCooldown);
            if (_currentState == DroneState.Chasing) SwitchState(DroneState.Boosting);
            yield return new WaitForSeconds(boostDuration);
            if (_currentState == DroneState.Boosting) SwitchState(DroneState.Chasing);
        }
    }
    
    #region ITimeSlowable Implementation

    public void SlowDown(float slowPercentage)
    {
        if (_isSlowed) return;
        _isSlowed = true;

        _savedVelocity = _rb.linearVelocity;
        _savedAngularVelocity = _rb.angularVelocity;
        _slowFactor = 1.0f - (slowPercentage / 100.0f);
        _rb.isKinematic = true;

        if (droneRenderer != null)
        {
            _originalVisualColor = droneRenderer.material.color;
            droneRenderer.material.color = _slowDownColor;
        }
    }

    public void RestoreNormalTime()
    {
        if (!_isSlowed) return;
        _isSlowed = false;

        _rb.isKinematic = false;
        _rb.linearVelocity = _savedVelocity;
        _rb.angularVelocity = _savedAngularVelocity;

        if (droneRenderer != null)
        {
            droneRenderer.material.color = _originalVisualColor;
        }
    }

    public void SetSlowDownColor(Color newColor)
    {
        _slowDownColor = newColor;
    }
    
    private void HandleSlowMotionPhysics()
    {
        if (_slowFactor <= 0f) return;
        
        Vector3 newPosition = _rb.position + (_savedVelocity * _slowFactor * Time.fixedDeltaTime);
        _rb.MovePosition(newPosition);

        Quaternion deltaRotation = Quaternion.Euler(_savedAngularVelocity * _slowFactor * Time.fixedDeltaTime);
        _rb.MoveRotation(_rb.rotation * deltaRotation);
    }
    
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseSightRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}