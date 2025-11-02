// Local: Assets/Scripts/Scenario/LaserBarrier.cs

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LaserBarrier : MonoBehaviour, ITimeSlowable
{
    [Header("Componentes")]
    [Tooltip("O Mesh Renderer do visual do laser. O script desabilitará isso em vez do GameObject.")]
    [SerializeField] private Renderer laserRenderer;
    [SerializeField] private SpriteRenderer laserSpriteRenderer;

    private Collider _collider;

    [Header("Comportamento do Ciclo")]
    [Tooltip("Tempo em segundos que o laser fica ativo.")]
    [SerializeField] private float activeDuration = 5f;
    [Tooltip("Tempo em segundos que o laser fica inativo.")]
    [SerializeField] private float inactiveDuration = 3f;
    [Tooltip("Deslocamento inicial do ciclo em segundos. Útil para dessincronizar múltiplos lasers.")]
    [SerializeField] private float cycleOffset = 0f;
    private float _timer;
    private bool _isCurrentlyActive;

    [Header("Interação com o Jogador")]
    [Tooltip("A força com que o jogador é repelido ao tocar no laser ativo.")]
    [SerializeField] private float repulsionForce = 50f;
    [Tooltip("A força vertical extra aplicada para jogar o jogador para cima.")]
    [SerializeField] private float upwardForceMultiplier = 0.3f;
    [Tooltip("Se marcado, a velocidade do jogador será zerada antes de aplicar a repulsão.")]
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

        if (laserRenderer == null)
        {
            laserRenderer = GetComponentInChildren<Renderer>();
        }

        if (laserRenderer == null)
        {
            Debug.LogError("Nenhum Renderer foi encontrado para a LaserBarrier. O visual não funcionará.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        TimeManipulationManager.Instance?.Register(this);
        _timer = cycleOffset;
        UpdateLaserState();
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
            UpdateLaserState();
        }
    }

    private void UpdateLaserState()
    {
        SetLaserActive(_isCurrentlyActive);

        if (_isCurrentlyActive)
        {
            AudioManager.instance?.PlaySFX(reactivateSfx);
        }
        else
        {
            AudioManager.instance?.PlaySFX(deactivateSfx);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!_isCurrentlyActive || _isTimeSlowed) return;

        if (other.CompareTag(PlayerTag))
        {
            RepelPlayer(other.gameObject);
        }
    }

    private void RepelPlayer(GameObject playerObject)
    {
        var playerMovement = playerObject.GetComponent<PlayerMovementController>();
        if (playerMovement == null) return;

        Vector3 repulsionDirection = (playerObject.transform.position - transform.position);
        repulsionDirection.y = 0;
        repulsionDirection.Normalize();

        Vector3 finalDirection = (repulsionDirection + Vector3.up * upwardForceMultiplier).normalized;
        
        playerMovement.ApplyExternalForce(finalDirection, repulsionForce, resetPlayerMomentum);
        
        AudioManager.instance?.PlaySFX(repulsionSfx);
    }

    private void SetLaserActive(bool isActive)
    {
        if (laserRenderer != null)
        {
            laserRenderer.enabled = isActive;
            laserSpriteRenderer.enabled = isActive;
        }
        _collider.enabled = isActive;
    }

    #region ITimeSlowable Implementation

    public void SlowDown(float slowPercentage)
    {
        _isTimeSlowed = true;
    }

    public void RestoreNormalTime()
    {
        _isTimeSlowed = false;
    }
    
    public void SetSlowDownColor(Color newColor) { } 

    #endregion
}