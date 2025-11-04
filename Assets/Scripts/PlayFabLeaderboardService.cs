// Local: Assets/Scripts/Scoring/PlayFabLeaderboardService.cs

using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

public class LeaderboardResult
{
    public List<ScoreEntry> TopEntries { get; set; } = new List<ScoreEntry>();
    public ScoreEntry PlayerEntry { get; set; }
}

public class PlayFabLeaderboardService : ILeaderboardService
{
    private const int SCORE_PRECISION_MULTIPLIER = 1000;

    public async Task<LeaderboardResult> GetLeaderboardWithPlayerAsync(string levelId, int topCount)
    {
        var topRequest = new GetLeaderboardRequest { StatisticName = levelId, StartPosition = 0, MaxResultsCount = topCount };
        var playerRequest = new GetLeaderboardAroundPlayerRequest { StatisticName = levelId, PlayFabId = PlayFabAuthManager.Instance.PlayFabId, MaxResultsCount = 1 };

        var topTask = GetLeaderboardAsync(topRequest);
        var playerTask = GetLeaderboardAroundPlayerAsync(playerRequest);

        try
        {
            await Task.WhenAll(topTask, playerTask);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao buscar leaderboards: {e.Message}");
            return new LeaderboardResult();
        }

        var result = new LeaderboardResult();

        if (topTask.Result?.Leaderboard != null)
        {
            result.TopEntries = topTask.Result.Leaderboard
                .Select(entry => ConvertPlayFabEntryToScoreEntry(entry, levelId))
                .ToList();
        }

        if (playerTask.Result?.Leaderboard != null && playerTask.Result.Leaderboard.Count > 0)
        {
            result.PlayerEntry = ConvertPlayFabEntryToScoreEntry(playerTask.Result.Leaderboard[0], levelId);
        }

        return result;
    }

    public Task<bool> SubmitScoreAsync(ScoreEntry score)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        if (score.scoreTime <= 0)
        {
            Debug.LogWarning($"Tentativa de submeter pontuação inválida (<= 0). Tempo: {score.scoreTime}");
            tcs.SetResult(false);
            return tcs.Task;
        }
        
        int finalValue = (int)(-score.scoreTime * SCORE_PRECISION_MULTIPLIER);

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> { new StatisticUpdate { StatisticName = score.levelId, Value = finalValue } }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, 
            (result) => tcs.SetResult(true), 
            (error) => {
                Debug.LogError("Falha ao submeter pontuação: " + error.GenerateErrorReport());
                tcs.SetResult(false);
            }
        );
        return tcs.Task;
    }

    private Task<GetLeaderboardResult> GetLeaderboardAsync(GetLeaderboardRequest request)
    {
        var tcs = new TaskCompletionSource<GetLeaderboardResult>();
        PlayFabClientAPI.GetLeaderboard(request, tcs.SetResult, error => tcs.SetException(new System.Exception(error.GenerateErrorReport())));
        return tcs.Task;
    }

    private Task<GetLeaderboardAroundPlayerResult> GetLeaderboardAroundPlayerAsync(GetLeaderboardAroundPlayerRequest request)
    {
        var tcs = new TaskCompletionSource<GetLeaderboardAroundPlayerResult>();
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, tcs.SetResult, error => tcs.SetException(new System.Exception(error.GenerateErrorReport())));
        return tcs.Task;
    }

    private ScoreEntry ConvertPlayFabEntryToScoreEntry(PlayerLeaderboardEntry playfabEntry, string levelId)
    {
        float time = (float)-playfabEntry.StatValue / SCORE_PRECISION_MULTIPLIER;

        var scoreEntry = new ScoreEntry(
            playfabEntry.PlayFabId,
            playfabEntry.DisplayName ?? "Player",
            time,
            levelId
        );
        scoreEntry.Position = playfabEntry.Position + 1;
        return scoreEntry;
    }
    
    public async Task<List<ScoreEntry>> GetLeaderboardAsync(string levelId, int count)
    {
        var result = await GetLeaderboardWithPlayerAsync(levelId, count);
        var allEntries = new List<ScoreEntry>(result.TopEntries);
        if (result.PlayerEntry != null && !allEntries.Any(e => e.playerId == result.PlayerEntry.playerId))
        {
            allEntries.Add(result.PlayerEntry);
        }
        return allEntries;
    }
}