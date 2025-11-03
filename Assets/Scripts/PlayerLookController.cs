// Local: Assets/Scripts/PlayerLookController.cs

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerLookController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private GrapplingHookController grapplingHookController;
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
    [SerializeField] private float maxShakeAmplitudeGain = 1.5f;
    [SerializeField] private float maxShakeFrequencyGain = 1.5f;
    [SerializeField] private float shakeMinSpeedThreshold = 25f;
    [SerializeField] private float shakeMaxSpeedThreshold = 60f;

    [Header("Rotação do Modelo")]
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
        GameSettingsManager.OnInvertXChanged += HandleInvertXChanged;
        GameSettingsManager.OnInvertYChanged += HandleInvertYChanged;

        if (playerMovementController != null)
        {
            playerMovementController.OnHorizontalVelocityChanged += HandleVelocityChanged;
        }
    }

    private void OnDisable()
    {
        GameSettingsManager.OnMouseSensitivityXChanged -= HandleMouseSensitivityXChanged;
        GameSettingsManager.OnMouseSensitivityYChanged -= HandleMouseSensitivityYChanged;
        GameSettingsManager.OnInvertXChanged -= HandleInvertXChanged;
        GameSettingsManager.OnInvertYChanged -= HandleInvertYChanged;

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
        _cameraNoise.AmplitudeGain = 0;
        _cameraNoise.FrequencyGain = 0;

        ConnectCinemachineToInputManager();

        if (GameSettingsManager.Instance != null)
        {
            UpdateXAxisSettings();
            UpdateYAxisSettings();
        }
    }

    private void Update()
    {
        if (grapplingHookController != null && grapplingHookController.IsGrappling)
        {
            HandleGrappleModelRotation();
        }
        else
        {
            HandleStandardModelRotation();
        }
    }

    private void HandleVelocityChanged(float currentHorizontalSpeed)
    {
        UpdateFov(currentHorizontalSpeed);
        UpdateCameraShake(currentHorizontalSpeed);
    }

    private void HandleInvertXChanged(bool inverted) => UpdateXAxisSettings();
    private void HandleInvertYChanged(bool inverted) => UpdateYAxisSettings();
    private void HandleMouseSensitivityXChanged(float normalizedValue) => UpdateXAxisSettings();
    private void HandleMouseSensitivityYChanged(float normalizedValue) => UpdateYAxisSettings();

    private void UpdateXAxisSettings()
    {
        if (cinemachineInputAxisController == null || GameSettingsManager.Instance == null) return;

        float sensitivity = GameSettingsManager.Instance.MouseSensitivityX;
        bool isInverted = GameSettingsManager.Instance.InvertMouseX;

        float baseGain = Mathf.Lerp(minSensitivityGainX, maxSensitivityGainX, sensitivity);
        float finalGain = isInverted ? -baseGain : baseGain;

        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            if (controller.Name == "Look Orbit X" || controller.Name == "X")
            {
                controller.Input.Gain = finalGain;
            }
        }
    }

    private void UpdateYAxisSettings()
    {
        if (cinemachineInputAxisController == null || GameSettingsManager.Instance == null) return;

        float sensitivity = GameSettingsManager.Instance.MouseSensitivityY;
        bool isInverted = GameSettingsManager.Instance.InvertMouseY;

        float baseGain = Mathf.Lerp(minSensitivityGainY, maxSensitivityGainY, sensitivity);
        float finalGain = isInverted ? -baseGain : baseGain;

        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            if (controller.Name == "Look Orbit Y" || controller.Name == "Y")
            {
                controller.Input.Gain = -finalGain;
            }
        }
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

        _cameraNoise.AmplitudeGain = Mathf.Lerp(0, maxShakeAmplitudeGain, normalizedSpeed);
        _cameraNoise.FrequencyGain = Mathf.Lerp(0, maxShakeFrequencyGain, normalizedSpeed);
    }

    private void SetCameraFOV(float fov)
    {
        _tempLens = freeLookCamera.Lens;
        _tempLens.FieldOfView = fov;
        freeLookCamera.Lens = _tempLens;
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

    private void HandleStandardModelRotation()
    {
        if (cameraTransform == null || orientation == null || playerModel == null || playerMovementController == null) return;

        Vector3 viewDirection = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        if (viewDirection != Vector3.zero)
        {
            orientation.forward = viewDirection;
        }

        Vector3 horizontalVelocity = new Vector3(playerMovementController.Rb.linearVelocity.x, 0f, playerMovementController.Rb.linearVelocity.z);

        if (horizontalVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized);
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, playerRotationSpeed * Time.deltaTime);
        }
    }

    private void HandleGrappleModelRotation()
    {
        if (playerModel == null || cameraTransform == null || grapplingHookController == null) return;

        Vector3 directionToGrapple = (grapplingHookController.GrapplePoint - playerModel.position).normalized;

        if (directionToGrapple == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(directionToGrapple, cameraTransform.up);

        playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, playerRotationSpeed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        _fovTween?.Kill();

        if (freeLookCamera != null && _cameraNoise != null)
        {
            _cameraNoise.AmplitudeGain = 0;
            _cameraNoise.FrequencyGain = 0;
        }
    }
}