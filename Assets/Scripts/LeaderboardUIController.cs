// Local: Assets/Scripts/UI/LeaderboardUIController.cs
using UnityEngine;
using System.Threading.Tasks;

public class LeaderboardUIController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private LevelData levelData;
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject loadingIndicator;

    public async void ShowLeaderboard()
    {
        if (levelData == null || LeaderboardManager.Instance == null)
        {
            Debug.LogError("LevelData ou LeaderboardManager não configurados!");
            return;
        }

        ClearLeaderboard();
        Debug.Log("Carregando leaderboard...");
        loadingIndicator.SetActive(true);

        var scores = await LeaderboardManager.Instance.GetLeaderboardAsync(levelData.GetFullLevelId(), 50);

        loadingIndicator.SetActive(false);

        for (int i = 0; i < scores.Count; i++)
        {
            GameObject entryGO = Instantiate(scoreEntryPrefab, contentParent);
            ScoreUIEntry entryUI = entryGO.GetComponent<ScoreUIEntry>();
            entryUI.Populate(i + 1, scores[i]);
        }
        gameObject.SetActive(true);
    }

    private void ClearLeaderboard()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    void Start()
    {
    }
}