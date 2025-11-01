// Local: Assets/Scripts/UI/LeaderboardUIController.cs

using UnityEngine;

public class LeaderboardUIController : MonoBehaviour
{
    [Header("Referências")]
  private LevelData levelData;
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private Transform topScoresContentParent;
    [SerializeField] private GameObject loadingIndicator;

    [Header("UI do Jogador Local")]
    [SerializeField] private GameObject playerScoreContainer;
    [SerializeField] private ScoreUIEntry playerScoreUIEntry;
    
    private void OnEnable()
    {
        GameFlowManager.OnLevelCompleted += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        GameFlowManager.OnLevelCompleted -= HandleLevelCompleted;
    }
    private void Awake()
    {
        UpdateLevelData();
    }

    private void HandleLevelCompleted(float finalTime)
    {
        UpdateLevelData();
        PrepareForDisplay(finalTime);
    }
    
    public void PrepareForDisplay(float time)
    {
        ClearLeaderboard();
        gameObject.SetActive(true);
        if (playerScoreContainer != null) playerScoreContainer.SetActive(false);
        loadingIndicator.SetActive(true);

        if (playerScoreContainer != null && playerScoreUIEntry != null)
        {
            var tempEntry = new ScoreEntry(
                PlayFabAuthManager.Instance.PlayFabId,
                PlayerProfile.Instance.CurrentProfile.profileName,
                time,
                levelData.GetFullLevelId()
            );
            playerScoreUIEntry.Populate(0, tempEntry, true); 
            playerScoreContainer.SetActive(true);
        }
    }

    public async void ShowLeaderboard()
    {
        UpdateLevelData();
        if (levelData == null || LeaderboardManager.Instance == null || PlayFabAuthManager.Instance == null)
        {
            Debug.LogError("Dependências não configuradas para o Leaderboard!");
            loadingIndicator.SetActive(false);
            return;
        }

        var leaderboardResult = await LeaderboardManager.Instance.GetLeaderboardWithPlayerAsync(levelData.GetFullLevelId(), 10);
        loadingIndicator.SetActive(false);

        ClearLeaderboard();

        string localPlayerFabId = PlayFabAuthManager.Instance.PlayFabId;

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
        
        if (leaderboardResult?.PlayerEntry != null && playerScoreContainer != null && playerScoreUIEntry != null)
        {
            playerScoreUIEntry.Populate(leaderboardResult.PlayerEntry.Position, leaderboardResult.PlayerEntry, true);
            playerScoreContainer.SetActive(true);
        }
        else if (playerScoreContainer != null)
        {
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
    public void UpdateLevelData()
    {
        levelData = ScoreManager.Instance.GetCurrentLevelData();
    }
}