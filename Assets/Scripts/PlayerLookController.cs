// Local: Assets/Scripts/PlayerLookController.cs

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla a orientação de movimento do jogador e a sensibilidade da câmera.
/// Ouve o GameSettingsManager para atualizações de sensibilidade.
/// </summary>
public class PlayerLookController : MonoBehaviour
{
    [Header("Configurações de Sensibilidade")]
    [Tooltip("A 'gain' mínima da câmera quando o slider está em 0.")]
    [SerializeField] private float minSensitivityGain = 0.5f;
    [Tooltip("A 'gain' máxima da câmera quando o slider está em 1.")]
    [SerializeField] private float maxSensitivityGain = 10f;
    
    [Header("Outras Configurações")]
    [SerializeField] private float playerRotationSpeed = 10f;

    [Header("Referências")]
    [Tooltip("O modelo visual do jogador que deve rotacionar.")]
    [SerializeField] private Transform playerModel;
    [Tooltip("Um objeto vazio filho do Player que define a direção do movimento.")]
    [SerializeField] private Transform orientation;
    [Tooltip("Referência à câmera principal ou à câmera virtual do Cinemachine.")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Referência ao controlador de eixos da câmera virtual.")]
    [SerializeField] private CinemachineInputAxisController cinemachineInputAxisController;

    private void OnEnable()
    {
        GameSettingsManager.OnMouseSensitivityChanged += HandleMouseSensitivityChanged;
    }

    private void OnDisable()
    {
        GameSettingsManager.OnMouseSensitivityChanged -= HandleMouseSensitivityChanged;
    }

    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager não encontrado! A câmera não pode ser conectada.");
            return;
        }

        ConnectCinemachineToInputManager();
        
        // Aplica a sensibilidade inicial salva ao iniciar o jogo.
        if (GameSettingsManager.Instance != null)
        {
            HandleMouseSensitivityChanged(GameSettingsManager.Instance.MouseSensitivity);
        }
    }

    private void Update()
    {
        HandlePlayerModelRotation();
    }
    
    /// <summary>
    /// Manipula o evento de mudança de sensibilidade vindo do GameSettingsManager.
    /// </summary>
    /// <param name="normalizedValue">O valor da sensibilidade de 0 a 1.</param>
    private void HandleMouseSensitivityChanged(float normalizedValue)
    {
        // Mapeia o valor normalizado (0-1) para o intervalo de ganho desejado (min-max).
        float newGain = Mathf.Lerp(minSensitivityGain, maxSensitivityGain, normalizedValue);

        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            if (controller.Name == "Look Orbit X")
            {
                controller.Input.Gain = newGain;
            }
            if (controller.Name == "Look Orbit Y")
            {
                // O eixo Y geralmente é invertido.
                controller.Input.Gain = -newGain;
            }
        }
    }

    private void ConnectCinemachineToInputManager()
    {
        InputAction lookAction = InputManager.Instance.PlayerControls.Player.Look;
        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            InputActionReference lookActionReference = InputActionReference.Create(lookAction);
            controller.Input.InputAction = lookActionReference;
        }
        Debug.Log("<color=cyan>CinemachineInputAxisController foi conectado com sucesso ao InputManager.</color>");
    }
    
    private void HandlePlayerModelRotation()
    {
        if (cameraTransform == null || orientation == null || playerModel == null) return;

        Vector3 viewDirection = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        if (viewDirection != Vector3.zero)
        {
            orientation.forward = viewDirection;
        }

        playerModel.forward = Vector3.Slerp(playerModel.forward, orientation.forward, playerRotationSpeed * Time.deltaTime);
    }
}