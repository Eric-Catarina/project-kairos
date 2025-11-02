// Local: Assets/Scripts/GameSettingsManager.cs

using System;
using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    public static event Action<float> OnMouseSensitivityXChanged;
    public static event Action<float> OnMouseSensitivityYChanged;
    public static event Action<bool> OnInvertXChanged;
    public static event Action<bool> OnInvertYChanged;

    private GameSettings _settings;

    // Propriedades agora leem diretamente do objeto de configurações
    public float MouseSensitivityX => _settings.mouseSensitivityX;
    public float MouseSensitivityY => _settings.mouseSensitivityY;
    public bool InvertMouseX => _settings.invertMouseX;
    public bool InvertMouseY => _settings.invertMouseY;
    public float MotionBlurIntensity => _settings.motionBlurIntensity;

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
        // Pega a referência para as configurações do SaveManager
        _settings = SaveManager.Instance.GetSettings();
    }

    // Métodos agora modificam o objeto de dados e depois salvam
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
        // Um evento poderia ser disparado aqui se algum sistema precisar ouvir
    }
}