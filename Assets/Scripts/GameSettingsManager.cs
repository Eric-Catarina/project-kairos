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

    public float MouseSensitivityX
    {
        get => PlayerPrefs.GetFloat("MouseSensitivityX", 0.5f);
        private set => PlayerPrefs.SetFloat("MouseSensitivityX", value);
    }
    
    public float MouseSensitivityY
    {
        get => PlayerPrefs.GetFloat("MouseSensitivityY", 0.5f);
        private set => PlayerPrefs.SetFloat("MouseSensitivityY", value);
    }

    public bool InvertMouseX
    {
        get => PlayerPrefs.GetInt("InvertMouseX", 0) == 1;
        private set => PlayerPrefs.SetInt("InvertMouseX", value ? 1 : 0);
    }

    public bool InvertMouseY
    {
        get => PlayerPrefs.GetInt("InvertMouseY", 0) == 1;
        private set => PlayerPrefs.SetInt("InvertMouseY", value ? 1 : 0);
    }

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

    public void SetMouseSensitivityX(float normalizedValue)
    {
        float clampedValue = Mathf.Clamp01(normalizedValue);
        MouseSensitivityX = clampedValue;
        OnMouseSensitivityXChanged?.Invoke(clampedValue);
    }
    
    public void SetMouseSensitivityY(float normalizedValue)
    {
        float clampedValue = Mathf.Clamp01(normalizedValue);
        MouseSensitivityY = clampedValue;
        OnMouseSensitivityYChanged?.Invoke(clampedValue);
    }

    public void SetInvertX(bool isInverted)
    {
        InvertMouseX = isInverted;
        OnInvertXChanged?.Invoke(isInverted);
        PlayerPrefs.Save();
    }

    public void SetInvertY(bool isInverted)
    {
        InvertMouseY = isInverted;
        OnInvertYChanged?.Invoke(isInverted);
        PlayerPrefs.Save();
    }
}