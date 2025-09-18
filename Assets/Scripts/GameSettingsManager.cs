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

    // Evento disparado quando a sensibilidade do mouse é alterada.
    // O float representa o novo valor normalizado (0 a 1).
    public static event Action<float> OnMouseSensitivityChanged;

    // Propriedade para armazenar o valor de sensibilidade atual (normalizado de 0 a 1).
    // Usamos PlayerPrefs para persistir a configuração entre as sessões de jogo.
    public float MouseSensitivity
    {
        get => PlayerPrefs.GetFloat("MouseSensitivity", 0.5f); // Valor padrão é 0.5
        private set => PlayerPrefs.SetFloat("MouseSensitivity", value);
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
    /// Define um novo valor para a sensibilidade do mouse.
    /// Este método é chamado pela UI (por exemplo, um slider).
    /// </summary>
    /// <param name="normalizedValue">O novo valor de sensibilidade, de 0.0 a 1.0.</param>
    public void SetMouseSensitivity(float normalizedValue)
    {
        // Garante que o valor esteja dentro do intervalo [0, 1]
        float clampedValue = Mathf.Clamp01(normalizedValue);
        
        MouseSensitivity = clampedValue;
        
        // Notifica todos os ouvintes sobre a mudança.
        OnMouseSensitivityChanged?.Invoke(clampedValue);
        
        Debug.Log($"Sensibilidade do mouse alterada para: {clampedValue}");
    }
}