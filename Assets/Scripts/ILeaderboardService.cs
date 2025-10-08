// Local: Assets/Scripts/Scoring/ILeaderboardService.cs

using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILeaderboardService
{
    Task<bool> SubmitScoreAsync(ScoreEntry score);
    Task<List<ScoreEntry>> GetLeaderboardAsync(string levelId, int count);
}