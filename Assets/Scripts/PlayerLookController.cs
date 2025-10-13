// Local: Assets/Scripts/PlayerLookController.cs

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerLookController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CinemachineInputAxisController cinemachineInputAxisController;
    [SerializeField] private CinemachineCamera freeLookCamera;
    private CinemachineBasicMultiChannelPerlin _cameraNoise;
    
    [Header("Sensibilidade")]
    [SerializeField] private float minSensitivityGainX = 0.5f;
    [SerializeField] private float maxSensitivityGainX = 10f;
    [SerializeField] private float minSensitivityGainY = 0.5f;
    [SerializeField] private float maxSensitivityGainY = 10f;

    [Header("FOV Dinâmico")]
    [SerializeField] private float maxFov = 70f;
    [SerializeField] private float fovMinSpeedThreshold = 15f;
    [SerializeField] private float fovMaxSpeedThreshold = 40f;
    [SerializeField] private float fovTransitionDuration = 0.5f;

    [Header("Camera Shake (Noise)")]
    [Tooltip("A intensidade máxima do tremor em altas velocidades (multiplicador).")]
    [SerializeField] private float maxShakeAmplitudeGain = 1.5f;
    [Tooltip("A rapidez máxima do tremor em altas velocidades (multiplicador).")]
    [SerializeField] private float maxShakeFrequencyGain = 1.5f;
    [Tooltip("Velocidade (m/s) a partir da qual o tremor começa.")]
    [SerializeField] private float shakeMinSpeedThreshold = 25f;
    [Tooltip("Velocidade (m/s) na qual o tremor atinge sua intensidade máxima.")]
    [SerializeField] private float shakeMaxSpeedThreshold = 60f;

    [Header("Outras Configurações")]
    [SerializeField] private float playerRotationSpeed = 10f;

    private float _baseFov;
    private Tweener _fovTween;
    private LensSettings _tempLens;

    void Awake()
    {
        if (freeLookCamera != null)
        {
            _cameraNoise = freeLookCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (_cameraNoise == null)
            {
                Debug.LogError("CinemachineBasicMultiChannelPerlin não encontrado no FreeLook Camera.");
            }
        }
    }

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

        _baseFov = freeLookCamera.Lens.FieldOfView;
        
        // Garante que o tremor comece desativado
        _cameraNoise.AmplitudeGain = 0;
        _cameraNoise.FrequencyGain = 0;

        ConnectCinemachineToInputManager();
        
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
        UpdateFov(currentHorizontalSpeed);
        UpdateCameraShake(currentHorizontalSpeed);
    }

    private void UpdateFov(float speed)
    {
        if (freeLookCamera == null) return;
        
        float normalizedSpeed = Mathf.InverseLerp(fovMinSpeedThreshold, fovMaxSpeedThreshold, speed);
        float targetFov = Mathf.Lerp(_baseFov, maxFov, normalizedSpeed);

        _fovTween?.Kill();
        _fovTween = DOTween.To(
            () => freeLookCamera.Lens.FieldOfView,
            x => SetCameraFOV(x),
            targetFov,
            fovTransitionDuration
        ).SetEase(Ease.OutQuad);
    }

    private void UpdateCameraShake(float speed)
    {
        if (freeLookCamera == null) return;

        float normalizedSpeed = Mathf.InverseLerp(shakeMinSpeedThreshold, shakeMaxSpeedThreshold, speed);
        
        // CORREÇÃO: Modifica AmplitudeGain e FrequencyGain diretamente no componente CinemachineCamera
        _cameraNoise.AmplitudeGain = Mathf.Lerp(0, maxShakeAmplitudeGain, normalizedSpeed);
        _cameraNoise.FrequencyGain = Mathf.Lerp(0, maxShakeFrequencyGain, normalizedSpeed);
    }
    
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
            if (controller.Name == "Look Orbit Y" || controller.Name == "Y")
            {
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
        
        // Reseta o Noise ao sair da cena para evitar que ele persista
        if (freeLookCamera != null)
        {
            _cameraNoise.AmplitudeGain = 0;
            _cameraNoise.FrequencyGain = 0;
        }
    }
}