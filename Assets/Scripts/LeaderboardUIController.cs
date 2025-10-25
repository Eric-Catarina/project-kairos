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
    
    private float? _lastRunTime;

    // Renomeado para maior clareza
    public void PrepareForDisplay(float time)
    {
        _lastRunTime = time;

        // Limpa a UI e mostra o painel imediatamente para exibir o tempo da corrida
        ClearLeaderboard();
        gameObject.SetActive(true);
        if (playerScoreContainer != null) playerScoreContainer.SetActive(false);
        loadingIndicator.SetActive(true);

        // Exibe o tempo da corrida atual instantaneamente
        if (_lastRunTime.HasValue && playerScoreContainer != null && playerScoreUIEntry != null)
        {
            var tempEntry = new ScoreEntry(
                PlayFabAuthManager.Instance.PlayFabId,
                PlayerProfile.Instance.CurrentProfile.PlayerName,
                _lastRunTime.Value,
                levelData.GetFullLevelId()
            );
            playerScoreUIEntry.Populate(0, tempEntry, true); 
            playerScoreContainer.SetActive(true);
        }
    }

    public async void ShowLeaderboard()
    {
        if (levelData == null || LeaderboardManager.Instance == null || PlayFabAuthManager.Instance == null)
        {
            Debug.LogError("Dependências não configuradas para o Leaderboard!");
            // Esconde o indicador de carregamento se falhar
            loadingIndicator.SetActive(false);
            return;
        }

        var leaderboardResult = await LeaderboardManager.Instance.GetLeaderboardWithPlayerAsync(levelData.GetFullLevelId(), 10);
        loadingIndicator.SetActive(false);
        _lastRunTime = null; // Limpa o tempo da corrida, pois os dados do servidor são a nova fonte da verdade

        // Limpa as entradas antigas novamente para garantir
        ClearLeaderboard();

        string localPlayerFabId = PlayFabAuthManager.Instance.PlayFabId;

        // Popula o Top 10
        if (leaderboardResult?.TopEntries != null)
        {
            foreach (var score in leaderboardResult.TopEntries)
            {
                GameObject entryGO = Instantiate(scoreEntryPrefab, topScoresContentParent);
                ScoreUIEntry entryUI = entryGO.GetComponent<ScoreUIEntry>();
                bool isLocalPlayer = score.playerId == localPlayerFabId;
                entryUI.Populate(score.Position, score, isLocalPlayer);
            }
        }
        
        // Exibe a melhor pontuação oficial do jogador vinda do servidor
        if (leaderboardResult?.PlayerEntry != null && playerScoreContainer != null && playerScoreUIEntry != null)
        {
            playerScoreUIEntry.Populate(leaderboardResult.PlayerEntry.Position, leaderboardResult.PlayerEntry, true);
            playerScoreContainer.SetActive(true);
        }
        else if (playerScoreContainer != null)
        {
            // Se o jogador não tem pontuação no servidor (primeira corrida), esconde o container
            playerScoreContainer.SetActive(false);
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