// Local: Assets/Scripts/PlayerLookController.cs

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

/// <summary>
/// Controla a orientação de movimento do jogador, a sensibilidade da câmera e o FOV dinâmico.
/// Compatível com Cinemachine 3.
/// </summary>
public class PlayerLookController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Referência ao componente CinemachineInputAxisController (usado no FreeLook do CM3).")]
    [SerializeField] private CinemachineInputAxisController cinemachineInputAxisController;
    [Tooltip("Referência à CinemachineCamera (CM3) configurada como FreeLook.")]
    [SerializeField] private CinemachineCamera freeLookCamera;
    
    [Header("Sensibilidade - Eixo X")]
    [SerializeField] private float minSensitivityGainX = 0.5f;
    [SerializeField] private float maxSensitivityGainX = 10f;

    [Header("Sensibilidade - Eixo Y")]
    [SerializeField] private float minSensitivityGainY = 0.5f;
    [SerializeField] private float maxSensitivityGainY = 10f;

    [Header("FOV Dinâmico (CM3)")]
    [Tooltip("O FOV vertical máximo que a câmera atingirá em alta velocidade.")]
    [SerializeField] private float maxFov = 70f; // FreeLook geralmente tem um FOV base menor
    [Tooltip("Velocidade (m/s) a partir da qual o FOV começa a aumentar.")]
    [SerializeField] private float minSpeedThreshold = 15f;
    [Tooltip("Velocidade (m/s) na qual o FOV atinge seu valor máximo.")]
    [SerializeField] private float maxSpeedThreshold = 40f;
    [Tooltip("A rapidez com que o FOV transita para o novo valor usando DOTween.")]
    [SerializeField] private float fovTransitionDuration = 0.5f;

    [Header("Outras Configurações")]
    [SerializeField] private float playerRotationSpeed = 10f;

    private float _baseFov;
    private Tweener _fovTween;
    private LensSettings _tempLens; // Struct para modificar a lente no CM3

    private void OnEnable()
    {
        GameSettingsManager.OnMouseSensitivityXChanged += HandleMouseSensitivityXChanged;
        GameSettingsManager.OnMouseSensitivityYChanged += HandleMouseSensitivityYChanged;
        
        if (playerMovementController != null)
        {
            playerMovementController.OnHorizontalVelocityChanged += HandleVelocityChanged;
        }
    }

    private void OnDisable()
    {
        GameSettingsManager.OnMouseSensitivityXChanged -= HandleMouseSensitivityXChanged;
        GameSettingsManager.OnMouseSensitivityYChanged -= HandleMouseSensitivityYChanged;

        if (playerMovementController != null)
        {
            playerMovementController.OnHorizontalVelocityChanged -= HandleVelocityChanged;
        }
    }

    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager não encontrado! A câmera não pode ser conectada.");
            return;
        }

        if (freeLookCamera == null)
        {
            Debug.LogError("CinemachineCamera (FreeLook) não atribuída no PlayerLookController.");
            enabled = false;
            return;
        }

        // No CM3, pegamos o FOV base da propriedade Lens
        _baseFov = freeLookCamera.Lens.FieldOfView;
        Debug.Log($"FOV base do FreeLook: {_baseFov}");

        ConnectCinemachineToInputManager();
        
        // Aplica as sensibilidades iniciais
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

    private void HandleVelocityChanged(float currentHorizontalSpeed)
    {
        if (freeLookCamera == null) return;

        // Calcula o FOV alvo baseado na velocidade
        float normalizedSpeed = Mathf.InverseLerp(minSpeedThreshold, maxSpeedThreshold, currentHorizontalSpeed);
        float targetFov = Mathf.Lerp(_baseFov, maxFov, normalizedSpeed);

        // Usa DOTween para interpolar suavemente o valor do FOV
        _fovTween?.Kill();
        _fovTween = DOTween.To(
            () => freeLookCamera.Lens.FieldOfView, // Getter (CM3)
            x => SetCameraFOV(x),                  // Setter customizado
            targetFov,
            fovTransitionDuration
        ).SetEase(Ease.OutQuad);
    }

    // Método auxiliar para setar o FOV no CM3, pois LensSettings é uma struct
    private void SetCameraFOV(float fov)
    {
        _tempLens = freeLookCamera.Lens;
        _tempLens.FieldOfView = fov;
        freeLookCamera.Lens = _tempLens;
    }
    
    private void HandleMouseSensitivityXChanged(float normalizedValue)
    {
        if (cinemachineInputAxisController == null) return;
        
        float newGain = Mathf.Lerp(minSensitivityGainX, maxSensitivityGainX, normalizedValue);

        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            // Verifica nomes comuns de eixos X em FreeLook
            if (controller.Name == "Look Orbit X" || controller.Name == "X")
            {
                controller.Input.Gain = newGain;
            }
        }
    }
    
    private void HandleMouseSensitivityYChanged(float normalizedValue)
    {
        if (cinemachineInputAxisController == null) return;

        float newGain = Mathf.Lerp(minSensitivityGainY, maxSensitivityGainY, normalizedValue);
        
        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            // Verifica nomes comuns de eixos Y em FreeLook
            if (controller.Name == "Look Orbit Y" || controller.Name == "Y")
            {
                // O eixo Y geralmente é invertido no FreeLook padrão
                controller.Input.Gain = -newGain;
            }
        }
    }

    private void ConnectCinemachineToInputManager()
    {
        if (cinemachineInputAxisController == null) return;

        InputAction lookAction = InputManager.Instance.PlayerControls.Player.Look;
        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            InputActionReference lookActionReference = InputActionReference.Create(lookAction);
            controller.Input.InputAction = lookActionReference;
        }
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

    private void OnDestroy()
    {
        _fovTween?.Kill();
    }
}