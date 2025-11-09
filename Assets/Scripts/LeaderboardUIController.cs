// Assets/Scripts/UI/LeaderboardUIController.cs
using UnityEngine;
using UnityEngine.UI; // Novo: Para Button

public class LeaderboardUIController : MonoBehaviour
{
    [Header("Referências")]
    private LevelData levelData;
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private Transform topScoresContentParent;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private Button nextLevelButton; 

    [Header("UI do Jogador Local")]
    [SerializeField] private GameObject playerScoreContainer;
    [SerializeField] private ScoreUIEntry playerScoreUIEntry;
    
    private void Awake()
    {
        UpdateLevelData();
    }

    private void Start()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(GoToNextLevel); 
        }
    }

    private void OnEnable()
    {
        Debug.Log("LeaderboardUIController: OnEnable called - panel is being activated!");
        GameFlowManager.Instance.OnLevelCompleted += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        GameFlowManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
    }

    private void HandleLevelCompleted(LevelCompletionData data)
    {
        Debug.Log("LeaderboardUIController: HandleLevelCompleted called - updating level data only.");
        UpdateLevelData();
    }
    
    public async void ShowLeaderboard()
    {
        Debug.Log("LeaderboardUIController: ShowLeaderboard called - activating panel and fetching leaderboard.");
        gameObject.SetActive(true);
        Debug.Log($"LeaderboardUIController: Panel active after SetActive: {gameObject.activeSelf}");
        
        // Forçar visibilidade
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Debug.Log("LeaderboardUIController: CanvasGroup adjusted.");
        }
        else
        {
            Debug.Log("LeaderboardUIController: No CanvasGroup found.");
        }
        
        // Novo: Verificar se o transform pai está ativo
        if (transform.parent != null)
        {
            Debug.Log($"LeaderboardUIController: Parent active: {transform.parent.gameObject.activeSelf}");
            if (!transform.parent.gameObject.activeSelf)
            {
                transform.parent.gameObject.SetActive(true);
                Debug.Log("LeaderboardUIController: Parent activated.");
            }
        }
        
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

    // Novo: Método para ir à próxima fase
    private void GoToNextLevel()
    {
        Debug.Log("LeaderboardUIController: GoToNextLevel called - loading next level.");
        // Assumindo que há um método para carregar o próximo nível (ajuste se necessário)
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.CompleteLevel();
        }
        else
        {
            Debug.LogError("GameFlowManager not found for loading next level.");
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