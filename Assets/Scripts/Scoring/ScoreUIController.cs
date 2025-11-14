// Assets/Scripts/Scoring/ScoreUIController.cs
using TMPro;
using UnityEngine;

public class ScoreUIController : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private GameObject resultsPanel; // Desativado: VictoryPanel o substitui

    private void OnEnable()
    {
        GameFlowManager.Instance.OnLevelCompleted += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        GameFlowManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
    }

    private void HandleLevelCompleted(LevelCompletionData data)
    {
        Debug.Log("ScoreUIController: HandleLevelCompleted called - updating UI.");
        if (finalTimeText != null)
        {
            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(data.FinalTime);
            finalTimeText.text = $"{timeSpan:mm\\:ss\\.ff}";
        }

        if (rankText != null)
        {
            rankText.text = $"Rank: {data.FinalRank}";
        }

        // Removido: resultsPanel.SetActive(true); // Não ativar, pois VictoryPanel o substitui
    }

    public void UpdateTime(float newTime)
    {
        if (finalTimeText != null)
        {
            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(newTime);
            finalTimeText.text = $"{timeSpan:mm\\:ss\\.ff}";
        }
    }

    public void UpdateTimeAndRank(float newTime)
    {
        if (finalTimeText != null)
        {
            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(newTime);
            finalTimeText.text = $"{timeSpan:mm\\:ss\\.ff}";
        }

        if (rankText != null)
        {
            Rank rank = ScoreManager.Instance != null ? ScoreManager.Instance.GetRankForTime(newTime) : Rank.NA;
            rankText.text = $"Rank: {rank}";
        }
    }
}