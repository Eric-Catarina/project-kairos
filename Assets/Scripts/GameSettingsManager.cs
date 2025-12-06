using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    public static event Action<float> OnMouseSensitivityXChanged;
    public static event Action<float> OnMouseSensitivityYChanged;
    public static event Action<bool> OnInvertXChanged;
    public static event Action<bool> OnInvertYChanged;
    public static event Action<bool> OnCheckpointsEnabledChanged;

    private GameSettings _settings;

    [Header("Motion Blur Settings")]
    [SerializeField] private float minBlurIntensity = 0f;
    [SerializeField] private float maxBlurIntensity = 0.02f;

    public float MouseSensitivityX => _settings != null ? _settings.mouseSensitivityX : 0.5f;
    public float MouseSensitivityY => _settings != null ? _settings.mouseSensitivityY : 0.5f;
    public bool InvertMouseX => _settings != null && _settings.invertMouseX;
    public bool InvertMouseY => _settings != null && _settings.invertMouseY;
    public float MotionBlurIntensity => _settings != null ? _settings.motionBlurIntensity : 0f;
    public bool CheckpointsEnabled => _settings != null && _settings.checkpointsEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Garante que pegamos as configurações se ainda não tivermos
        if (_settings == null && SaveManager.Instance != null)
        {
            _settings = SaveManager.Instance.GetSettings();
        }
        
        // Aplica o blur imediatamente ao iniciar o jogo
        ApplyMotionBlurToScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reaplica o blur sempre que mudar de fase
        ApplyMotionBlurToScene();
    }

    public void SetMouseSensitivityX(float normalizedValue)
    {
        if (EnsureSettingsLoaded())
        {
            float clampedValue = Mathf.Clamp01(normalizedValue);
            _settings.mouseSensitivityX = clampedValue;
            SaveManager.Instance.SaveGame();
            OnMouseSensitivityXChanged?.Invoke(clampedValue);
        }
    }
    
    public void SetMouseSensitivityY(float normalizedValue)
    {
        if (EnsureSettingsLoaded())
        {
            float clampedValue = Mathf.Clamp01(normalizedValue);
            _settings.mouseSensitivityY = clampedValue;
            SaveManager.Instance.SaveGame();
            OnMouseSensitivityYChanged?.Invoke(clampedValue);
        }
    }

    public void SetInvertX(bool isInverted)
    {
        if (EnsureSettingsLoaded())
        {
            _settings.invertMouseX = isInverted;
            SaveManager.Instance.SaveGame();
            OnInvertXChanged?.Invoke(isInverted);
        }
    }

    public void SetInvertY(bool isInverted)
    {
        if (EnsureSettingsLoaded())
        {
            _settings.invertMouseY = isInverted;
            SaveManager.Instance.SaveGame();
            OnInvertYChanged?.Invoke(isInverted);
        }
    }
    
    public void SetMotionBlur(float normalizedValue)
    {
        if (EnsureSettingsLoaded())
        {
            _settings.motionBlurIntensity = Mathf.Clamp01(normalizedValue);
            SaveManager.Instance.SaveGame();
            
            // Aplica imediatamente ao alterar o valor
            ApplyMotionBlurToScene();
        }
    }
    
    public void SetCheckpointsEnabled(bool isEnabled)
    {
        if (EnsureSettingsLoaded())
        {
            _settings.checkpointsEnabled = isEnabled;
            SaveManager.Instance.SaveGame();
            OnCheckpointsEnabledChanged?.Invoke(isEnabled);
        }
    }

    private bool EnsureSettingsLoaded()
    {
        if (_settings == null)
        {
            if (SaveManager.Instance != null)
            {
                _settings = SaveManager.Instance.GetSettings();
            }
        }
        return _settings != null;
    }

    private void ApplyMotionBlurToScene()
    {
        // Correção do NullReferenceException:
        // Se OnSceneLoaded rodar antes do Start ou antes do SaveManager estar pronto,
        // tentamos carregar. Se falhar, abortamos (o Start vai rodar depois e aplicar corretamente).
        if (!EnsureSettingsLoaded()) return;

        float targetIntensity = Mathf.Lerp(minBlurIntensity, maxBlurIntensity, _settings.motionBlurIntensity);
        
        // Encontra todos os volumes na cena (incluindo TimeStop volumes)
        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
        
        foreach (var volume in volumes)
        {
            if (volume.profile != null && volume.profile.TryGet(out MotionBlur motionBlur))
            {
                motionBlur.clamp.Override(targetIntensity);
                motionBlur.intensity.Override(1f); // Garante que a intensidade base esteja ligada para o clamp funcionar
            }
        }
    }
}