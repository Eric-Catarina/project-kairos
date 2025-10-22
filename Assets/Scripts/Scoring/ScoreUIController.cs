// Local: Assets/Scripts/Scoring/ScoreUIController.cs

using TMPro;
using UnityEngine;

public class ScoreUIController : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private GameObject resultsPanel;

    private void OnEnable()
    {
        GameFlowManager.OnLevelCompleted += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        GameFlowManager.OnLevelCompleted -= HandleLevelCompleted;
    }

    private void HandleLevelCompleted(float finalTime)
    {
        if (finalTimeText != null)
        {
            finalTimeText.text = $"Tempo: {finalTime:F2}s";
        }

        // O rank não é mais fornecido pelo evento; podemos calcular localmente se necessário
        if (rankText != null)
        {
            Rank rank = ScoreManager.Instance != null ? ScoreManager.Instance.GetRankForTime(finalTime) : Rank.None;
            rankText.text = $"Ranque: {rank}";
        }

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
        }
    }

    public void UpdateTime(float newTime)
    {
        if (finalTimeText != null)
        {
            finalTimeText.text = $"Tempo: {newTime:F2}s";
        }
    }
    
    public void UpdateTimeAndRank(float newTime)
    {
        if (finalTimeText != null)
        {
            finalTimeText.text = $"Tempo: {newTime:F2}s";
        }

        if (rankText != null)
        {
            Rank rank = ScoreManager.Instance != null ? ScoreManager.Instance.GetRankForTime(newTime) : Rank.None;
            rankText.text = $"Ranque: {rank}";
        }
    }
}