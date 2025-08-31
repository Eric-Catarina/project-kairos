// Local: Assets/Scripts/Scoring/ScoreUIController.cs

using TMPro;
using UnityEngine;

/// <summary>
/// Controla a exibição da UI de resultados do final do nível.
/// Ouve o evento OnLevelCompleted do ScoreManager para se atualizar.
/// </summary>
public class ScoreUIController : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private GameObject resultsPanel; // O painel pai que contém os textos

    private void OnEnable()
    {
        // Inscreve-se no evento para ser notificado quando o nível terminar.
        ScoreManager.OnLevelCompleted += HandleLevelCompleted;
    }



    private void OnDisable()
    {
        // Remove a inscrição para evitar erros e memory leaks.
        ScoreManager.OnLevelCompleted -= HandleLevelCompleted;
    }

    private void Start()
    {
        // Garante que o painel esteja escondido no início.
        if (resultsPanel != null)
        {
            // resultsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Método chamado pelo evento OnLevelCompleted.
    /// Atualiza os textos e exibe o painel de resultados.
    /// </summary>
    /// <param name="finalTime">O tempo final do jogador.</param>
    /// <param name="finalRank">O ranque final do jogador.</param>
    private void HandleLevelCompleted(float finalTime, Rank finalRank)
    {
        if (finalTimeText != null)
        {
            // Formata o tempo para ter duas casas decimais (ex: 34.56s).
            finalTimeText.text = $"Tempo: {finalTime:F2}s";
        }

        if (rankText != null)
        {
            rankText.text = $"Ranque: {finalRank}";
        }
        
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
        }
    }
}