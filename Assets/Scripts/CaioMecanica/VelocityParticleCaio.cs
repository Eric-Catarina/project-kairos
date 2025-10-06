// Local: Assets/Scripts/VelocityParticleController.cs

using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class VelocityParticleCaio : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Referência ao controlador de movimento do jogador para ouvir os eventos de velocidade.")]
    private PlayerMovimentCaio playerMovementController;

    [Header("Configurações do Efeito")]
    [Tooltip("A velocidade horizontal mínima (em m/s) para o efeito começar a aparecer.")]
    [SerializeField] private float minSpeedThreshold = 41.67f; // Equivalente a 150 km/h

    [Tooltip("A velocidade horizontal máxima (em m/s) onde o efeito atinge sua intensidade total.")]
    [SerializeField] private float maxSpeedThreshold = 83.33f; // Equivalente a 300 km/h

    [Tooltip("A opacidade (alpha) mínima do efeito quando a velocidade mínima é atingida.")]
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.1f;

    [Tooltip("A opacidade (alpha) máxima do efeito quando a velocidade máxima é atingida.")]
    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 1.0f;

    private ParticleSystem _particleSystem;
    private ParticleSystem.MainModule _mainModule;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _mainModule = _particleSystem.main;

        playerMovementController = FindObjectOfType<PlayerMovimentCaio>();
        
        if (playerMovementController == null)
        {
            Debug.LogError("PlayerMovementController não foi atribuído no VelocityParticleController.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (playerMovementController != null)
        {
            playerMovementController.OnHorizontalVelocityChanged += HandleVelocityChanged;
        }
    }

    private void OnDisable()
    {
        if (playerMovementController != null)
        {
            playerMovementController.OnHorizontalVelocityChanged -= HandleVelocityChanged;
        }
    }

    /// <summary>
    /// Manipula o evento de mudança de velocidade, ajustando a visibilidade e a opacidade das partículas.
    /// </summary>
    /// <param name="currentHorizontalSpeed">A velocidade horizontal atual do jogador em m/s.</param>
    private void HandleVelocityChanged(float currentHorizontalSpeed)
    {
        if (currentHorizontalSpeed >= minSpeedThreshold / 3.6f)
        {
            if (!_particleSystem.isPlaying)
            {
                _particleSystem.Play();
            }
            
            UpdateParticleAlpha(currentHorizontalSpeed);
        }
        else
        {
            if (_particleSystem.isPlaying)
            {
                _particleSystem.Stop();
            }
        }
    }

    /// <summary>
    /// Calcula e aplica a nova opacidade (alpha) à cor inicial das partículas.
    /// </summary>
    /// <param name="speed">A velocidade atual do jogador.</param>
    private void UpdateParticleAlpha(float speed)
    {
        // Calcula um valor normalizado (0 a 1) com base na velocidade atual entre o mínimo e o máximo.
        float normalizedSpeed = Mathf.InverseLerp(minSpeedThreshold / 3.6f, maxSpeedThreshold/ 3.6f, speed);

        // Mapeia o valor normalizado para o intervalo de alpha desejado (ex: 0.1 a 1.0).
        float newAlpha = Mathf.Lerp(minAlpha, maxAlpha, normalizedSpeed);

        // Obtém a cor inicial, atualiza seu alpha e a reaplica.
        Color startColor = _mainModule.startColor.color;
        startColor.a = newAlpha;
        _mainModule.startColor = startColor;
    }
}