// Local: Assets/Scripts/Scenario/LaserBarrier.cs

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LaserBarrier : MonoBehaviour, ITimeSlowable
{
    [Header("Componentes")]
    [Tooltip("O Mesh Renderer do visual do laser. O script desabilitará isso em vez do GameObject.")]
    [SerializeField] private Renderer laserRenderer;

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

    private Collider _collider;
    private bool _isDeactivatedByTime = false;
    private const string PlayerTag = "Player";

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;

        // Fallback: se o renderer não for atribuído, tenta encontrá-lo em um objeto filho
        if (laserRenderer == null)
        {
            laserRenderer = GetComponentInChildren<Renderer>();
        }

        if (laserRenderer == null)
        {
            Debug.LogError("Nenhum Renderer foi encontrado para a LaserBarrier. O visual não funcionará.", this);
        }
    }

    private void Start()
    {
        SetLaserActive(true);
        TimeManipulationManager.Instance?.Register(this);
    }

    private void OnDisable()
    {
        TimeManipulationManager.Instance?.Unregister(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isDeactivatedByTime) return;

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
        // *** MUDANÇA CRÍTICA AQUI ***
        // Habilita/desabilita os componentes, não o GameObject inteiro.
        if (laserRenderer != null)
        {
            laserRenderer.enabled = isActive;
        }
        _collider.enabled = isActive;
    }

    #region ITimeSlowable Implementation

    public void SlowDown(float slowPercentage)
    {
        if (!_isDeactivatedByTime)
        {
            _isDeactivatedByTime = true;
            SetLaserActive(false);
            AudioManager.instance?.PlaySFX(deactivateSfx);
        }
    }

    public void RestoreNormalTime()
    {
        if (_isDeactivatedByTime)
        {
            _isDeactivatedByTime = false;
            SetLaserActive(true);
            AudioManager.instance?.PlaySFX(reactivateSfx);
        }
    }
    
    public void SetSlowDownColor(Color newColor) { } 

    #endregion
}