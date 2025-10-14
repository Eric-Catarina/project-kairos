// Local: Assets/Scripts/UI/LeaderboardUIController.cs

using UnityEngine;

public class LeaderboardUIController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private LevelData levelData;
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private Transform topScoresContentParent;
    [SerializeField] private GameObject loadingIndicator;

    [Header("UI do Jogador Local")]
    [SerializeField] private GameObject playerScoreContainer;
    [SerializeField] private ScoreUIEntry playerScoreUIEntry;

    public async void ShowLeaderboard()
    {
        if (levelData == null || LeaderboardManager.Instance == null)
        {
            Debug.LogError("Dependências não configuradas!");
            return;
        }

        ClearLeaderboard();
        loadingIndicator.SetActive(true);
        if (playerScoreContainer != null) playerScoreContainer.SetActive(false);
        gameObject.SetActive(true);

        // Usando a nova função mais robusta
        var leaderboardResult = await LeaderboardManager.Instance.GetLeaderboardWithPlayerAsync(levelData.GetFullLevelId(), 10);

        loadingIndicator.SetActive(false);

        // Popula o Top 10
        if (leaderboardResult?.TopEntries != null)
        {
            foreach (var score in leaderboardResult.TopEntries)
            {
                GameObject entryGO = Instantiate(scoreEntryPrefab, topScoresContentParent);
                ScoreUIEntry entryUI = entryGO.GetComponent<ScoreUIEntry>();
                entryUI.Populate(score.Position, score);
            }
        }

        // Popula a entrada do jogador local
        if (leaderboardResult?.PlayerEntry != null && playerScoreContainer != null && playerScoreUIEntry != null)
        {
            playerScoreUIEntry.Populate(leaderboardResult.PlayerEntry.Position, leaderboardResult.PlayerEntry);
            playerScoreContainer.SetActive(true);
        }
        else
        {
             Debug.LogWarning("Nenhuma pontuação encontrada para o jogador local neste leaderboard.");
        }
    }

    private void ClearLeaderboard()
    {
        foreach (Transform child in topScoresContentParent)
        {
            Destroy(child.gameObject);
        }
    }
}