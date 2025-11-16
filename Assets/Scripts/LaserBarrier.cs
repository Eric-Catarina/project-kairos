using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LaserBarrier : MonoBehaviour, ITimeSlowable, IResettable
{
    [Header("Componentes")]
    [SerializeField] private Renderer laserRenderer;
    [SerializeField] private SpriteRenderer laserSpriteRenderer;
    private Collider _collider;

    [Header("Comportamento do Ciclo")]
    [SerializeField] private float activeDuration = 5f;
    [SerializeField] private float inactiveDuration = 3f;
    [SerializeField] private float cycleOffset = 0f;
    private float _timer;
    private bool _isCurrentlyActive;

    [Header("Interação com o Jogador")]
    [SerializeField] private float repulsionForce = 50f;
    [SerializeField] private float upwardForceMultiplier = 0.3f;
    [SerializeField] private bool resetPlayerMomentum = true;

    [Header("Efeitos")]
    [SerializeField] private string repulsionSfx = "LaserRepulsion";
    [SerializeField] private string deactivateSfx = "LaserDeactivate";
    [SerializeField] private string reactivateSfx = "LaserReactivate";

    private bool _isTimeSlowed = false;
    private const string PlayerTag = "Player";

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
        if (laserRenderer == null) laserRenderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        TimeManipulationManager.Instance?.Register(this);
        ResetState();
    }

    private void OnDisable()
    {
        TimeManipulationManager.Instance?.Unregister(this);
    }

    private void Update()
    {
        if (_isTimeSlowed) return;
        _timer += Time.deltaTime;
        float currentCycleDuration = _isCurrentlyActive ? activeDuration : inactiveDuration;
        if (_timer >= currentCycleDuration)
        {
            _timer = 0f;
            _isCurrentlyActive = !_isCurrentlyActive;
            UpdateLaserState(true);
        }
    }

    private void UpdateLaserState(bool playSfx)
    {
        SetLaserActive(_isCurrentlyActive);

        if (!playSfx) return;

        if (_isCurrentlyActive) AudioManager.instance?.PlaySFX(reactivateSfx);
        else AudioManager.instance?.PlaySFX(deactivateSfx);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!_isCurrentlyActive) return;
        if (other.CompareTag(PlayerTag)) RepelPlayer(other.gameObject);
    }

    private void RepelPlayer(GameObject playerObject)
    {
        var playerMovement = playerObject.GetComponent<PlayerMovementController>();
        if (playerMovement == null) return;

        playerMovement.TriggerShockEffect();
        
        Vector3 repulsionDirection = (playerObject.transform.position - transform.position);
        repulsionDirection.y = 0;
        repulsionDirection.Normalize();
        Vector3 finalDirection = (repulsionDirection + Vector3.up * upwardForceMultiplier).normalized;
        playerMovement.ApplyExternalForce(finalDirection, repulsionForce, resetPlayerMomentum);
        AudioManager.instance?.PlaySFX(repulsionSfx);
    }

    private void SetLaserActive(bool isActive)
    {
        if (laserRenderer != null) laserRenderer.enabled = isActive;
        if (laserSpriteRenderer != null) laserSpriteRenderer.enabled = isActive;
        _collider.enabled = isActive;
    }

    public void SlowDown(float slowPercentage) => _isTimeSlowed = true;
    public void RestoreNormalTime() => _isTimeSlowed = false;
    public void SetSlowDownColor(Color newColor) { }

    public void ResetState()
    {
        _timer = cycleOffset;
        _isCurrentlyActive = true;
        float effectiveTime = 0f;
        while(effectiveTime < cycleOffset)
        {
            float duration = _isCurrentlyActive ? activeDuration : inactiveDuration;
            if (effectiveTime + duration > cycleOffset)
            {
                _timer = cycleOffset - effectiveTime;
                break;
            }
            effectiveTime += duration;
            _isCurrentlyActive = !_isCurrentlyActive;
        }
        UpdateLaserState(false);
    }
}