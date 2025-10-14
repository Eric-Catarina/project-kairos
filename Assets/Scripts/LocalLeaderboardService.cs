// Local: Assets/Scripts/Scoring/LocalLeaderboardService.cs

using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class LocalLeaderboardService : ILeaderboardService
{
    private readonly string _filePath;

    public LocalLeaderboardService()
    {
        _filePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");
    }

    public async Task<List<ScoreEntry>> GetLeaderboardAsync(string levelId, int count)
    {
        LeaderboardData data = await ReadDataAsync();
        
        List<ScoreEntry> leaderboard = data.scores
            .Where(s => s.levelId == levelId)
            .OrderBy(s => s.scoreTime)
            .Take(count)
            .ToList();
            
        return leaderboard;
    }

    public Task<LeaderboardResult> GetLeaderboardWithPlayerAsync(string levelId, int topCount)
    {
        throw new System.NotImplementedException();
    }

    public async Task<bool> SubmitScoreAsync(ScoreEntry score)
    {
        if (score.scoreTime <= 0)
        {
            Debug.LogWarning("Tentativa de submeter pontuação inválida.");
            return false;
        }
        
        LeaderboardData data = await ReadDataAsync();
        data.scores.Add(score);
        await WriteDataAsync(data);
        
        return true;
    }

    private async Task<LeaderboardData> ReadDataAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new LeaderboardData();
        }

        using (StreamReader reader = new StreamReader(_filePath))
        {
            string json = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(json))
            {
                return new LeaderboardData();
            }
            return JsonUtility.FromJson<LeaderboardData>(json);
        }
    }

    private async Task WriteDataAsync(LeaderboardData data)
    {
        string json = JsonUtility.ToJson(data, true);
        using (StreamWriter writer = new StreamWriter(_filePath, false))
        {
            await writer.WriteAsync(json);
        }
    }
}