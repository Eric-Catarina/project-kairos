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
    [Header("Sensibilidade - Eixo X")]
    [Tooltip("A 'gain' mínima da câmera no eixo X quando o slider está em 0.")]
    [SerializeField] private float minSensitivityGainX = 0.5f;
    [Tooltip("A 'gain' máxima da câmera no eixo X quando o slider está em 1.")]
    [SerializeField] private float maxSensitivityGainX = 10f;

    [Header("Sensibilidade - Eixo Y")]
    [Tooltip("A 'gain' mínima da câmera no eixo Y quando o slider está em 0.")]
    [SerializeField] private float minSensitivityGainY = 0.5f;
    [Tooltip("A 'gain' máxima da câmera no eixo Y quando o slider está em 1.")]
    [SerializeField] private float maxSensitivityGainY = 10f;

    [Header("Outras Configurações")]
    [SerializeField] private float playerRotationSpeed = 10f;

    [Header("Referências")]
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CinemachineInputAxisController cinemachineInputAxisController;

    private void OnEnable()
    {
        // Inscreve-se nos novos eventos
        GameSettingsManager.OnMouseSensitivityXChanged += HandleMouseSensitivityXChanged;
        GameSettingsManager.OnMouseSensitivityYChanged += HandleMouseSensitivityYChanged;
    }

    private void OnDisable()
    {
        // Remove a inscrição dos novos eventos
        GameSettingsManager.OnMouseSensitivityXChanged -= HandleMouseSensitivityXChanged;
        GameSettingsManager.OnMouseSensitivityYChanged -= HandleMouseSensitivityYChanged;
    }

    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager não encontrado! A câmera não pode ser conectada.");
            return;
        }

        ConnectCinemachineToInputManager();
        
        // Aplica as sensibilidades iniciais salvas ao iniciar o jogo.
        if (GameSettingsManager.Instance != null)
        {
            HandleMouseSensitivityXChanged(GameSettingsManager.Instance.MouseSensitivityX);
            HandleMouseSensitivityYChanged(GameSettingsManager.Instance.MouseSensitivityY);
        }
    }

    private void Update()
    {
        HandlePlayerModelRotation();
    }
    
    /// <summary>
    /// Manipula o evento de mudança de sensibilidade para o eixo X.
    /// </summary>
    private void HandleMouseSensitivityXChanged(float normalizedValue)
    {
        float newGain = Mathf.Lerp(minSensitivityGainX, maxSensitivityGainX, normalizedValue);

        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            if (controller.Name == "Look Orbit X")
            {
                controller.Input.Gain = newGain;
            }
        }
    }

    /// <summary>
    /// Manipula o evento de mudança de sensibilidade para o eixo Y.
    /// </summary>
    private void HandleMouseSensitivityYChanged(float normalizedValue)
    {
        float newGain = Mathf.Lerp(minSensitivityGainY, maxSensitivityGainY, normalizedValue);
        
        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
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