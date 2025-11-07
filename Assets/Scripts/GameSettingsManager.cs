using System;
using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    public static event Action<float> OnMouseSensitivityXChanged;
    public static event Action<float> OnMouseSensitivityYChanged;
    public static event Action<bool> OnInvertXChanged;
    public static event Action<bool> OnInvertYChanged;
    public static event Action<bool> OnCheckpointsEnabledChanged;

    private GameSettings _settings;

    public float MouseSensitivityX => _settings.mouseSensitivityX;
    public float MouseSensitivityY => _settings.mouseSensitivityY;
    public bool InvertMouseX => _settings.invertMouseX;
    public bool InvertMouseY => _settings.invertMouseY;
    public float MotionBlurIntensity => _settings.motionBlurIntensity;
    public bool CheckpointsEnabled => _settings.checkpointsEnabled;

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

    private void Start()
    {
        _settings = SaveManager.Instance.GetSettings();
    }

    public void SetMouseSensitivityX(float normalizedValue)
    {
        float clampedValue = Mathf.Clamp01(normalizedValue);
        _settings.mouseSensitivityX = clampedValue;
        SaveManager.Instance.SaveGame();
        OnMouseSensitivityXChanged?.Invoke(clampedValue);
    }
    
    public void SetMouseSensitivityY(float normalizedValue)
    {
        float clampedValue = Mathf.Clamp01(normalizedValue);
        _settings.mouseSensitivityY = clampedValue;
        SaveManager.Instance.SaveGame();
        OnMouseSensitivityYChanged?.Invoke(clampedValue);
    }

    public void SetInvertX(bool isInverted)
    {
        _settings.invertMouseX = isInverted;
        SaveManager.Instance.SaveGame();
        OnInvertXChanged?.Invoke(isInverted);
    }

    public void SetInvertY(bool isInverted)
    {
        _settings.invertMouseY = isInverted;
        SaveManager.Instance.SaveGame();
        OnInvertYChanged?.Invoke(isInverted);
    }
    
    public void SetMotionBlur(float normalizedValue)
    {
        _settings.motionBlurIntensity = Mathf.Clamp01(normalizedValue);
        SaveManager.Instance.SaveGame();
    }
    
    public void SetCheckpointsEnabled(bool isEnabled)
    {
        _settings.checkpointsEnabled = isEnabled;
        SaveManager.Instance.SaveGame();
        OnCheckpointsEnabledChanged?.Invoke(isEnabled);
    }
}
