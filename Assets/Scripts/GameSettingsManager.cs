// Local: Assets/Scripts/GameSettingsManager.cs

using System;
using UnityEngine;

/// <summary>
/// Gerencia as configurações globais do jogo, como sensibilidade do mouse.
/// Implementa o padrão Singleton para acesso centralizado e persiste entre as cenas.
/// Dispara eventos quando as configurações são alteradas para que outros sistemas possam reagir.
/// </summary>
public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    // Eventos disparados quando a sensibilidade de cada eixo é alterada.
    public static event Action<float> OnMouseSensitivityXChanged;
    public static event Action<float> OnMouseSensitivityYChanged;

    // Propriedades para armazenar a sensibilidade de cada eixo (0 a 1).
    // Usamos PlayerPrefs para persistir a configuração entre as sessões de jogo.
    public float MouseSensitivityX
    {
        get => PlayerPrefs.GetFloat("MouseSensitivityX", 0.5f); // Valor padrão é 0.5
        private set => PlayerPrefs.SetFloat("MouseSensitivityX", value);
    }
    
    public float MouseSensitivityY
    {
        get => PlayerPrefs.GetFloat("MouseSensitivityY", 0.5f); // Valor padrão é 0.5
        private set => PlayerPrefs.SetFloat("MouseSensitivityY", value);
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

    /// <summary>
    /// Define um novo valor para a sensibilidade do eixo X do mouse.
    /// </summary>
    /// <param name="normalizedValue">O novo valor de sensibilidade, de 0.0 a 1.0.</param>
    public void SetMouseSensitivityX(float normalizedValue)
    {
        float clampedValue = Mathf.Clamp01(normalizedValue);
        MouseSensitivityX = clampedValue;
        OnMouseSensitivityXChanged?.Invoke(clampedValue);
        Debug.Log($"Sensibilidade do mouse (Eixo X) alterada para: {clampedValue}");
    }
    
    /// <summary>
    /// Define um novo valor para a sensibilidade do eixo Y do mouse.
    /// </summary>
    /// <param name="normalizedValue">O novo valor de sensibilidade, de 0.0 a 1.0.</param>
    public void SetMouseSensitivityY(float normalizedValue)
    {
        float clampedValue = Mathf.Clamp01(normalizedValue);
        MouseSensitivityY = clampedValue;
        OnMouseSensitivityYChanged?.Invoke(clampedValue);
        Debug.Log($"Sensibilidade do mouse (Eixo Y) alterada para: {clampedValue}");
    }
}