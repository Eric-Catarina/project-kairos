// Local: Assets/Scripts/Scoring/LeaderboardManager.cs

using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private ILeaderboardService _service;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeService();
    }

    private void InitializeService()
    {
        _service = new PlayFabLeaderboardService();
    }

    public Task<bool> SubmitScoreAsync(ScoreEntry score)
    {
        return _service.SubmitScoreAsync(score);
    }

    public Task<List<ScoreEntry>> GetLeaderboardAsync(string levelId, int count)
    {
        return _service.GetLeaderboardAsync(levelId, count);
    }
    
    // Nova função exposta
    public Task<LeaderboardResult> GetLeaderboardWithPlayerAsync(string levelId, int topCount)
    {
        return _service.GetLeaderboardWithPlayerAsync(levelId, topCount);
    }
}