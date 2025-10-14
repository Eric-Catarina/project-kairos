// Local: Assets/Scripts/Scoring/ILeaderboardService.cs

using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILeaderboardService
{
    Task<bool> SubmitScoreAsync(ScoreEntry score);
    
    // Retorna a lista completa para compatibilidade, mas a nova função é preferível
    Task<List<ScoreEntry>> GetLeaderboardAsync(string levelId, int count);
    
    // Nova função mais robusta
    Task<LeaderboardResult> GetLeaderboardWithPlayerAsync(string levelId, int topCount);
}