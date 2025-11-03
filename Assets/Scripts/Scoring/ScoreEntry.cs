// Local: Assets/Scripts/Scoring/ScoreEntry.cs

using System;
using System.Collections.Generic; // Adicionado para a lista em LeaderboardData

[Serializable]
public class ScoreEntry
{
    public string playerId;
    public string playerName;
    public float scoreTime;
    public string levelId;
    public long timestamp;
    public int Position; // Posição no ranking

    public ScoreEntry(string playerId, string playerName, float scoreTime, string levelId)
    {
        this.playerId = playerId;
        this.playerName = playerName;
        this.scoreTime = scoreTime;
        this.levelId = levelId;
        this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

[Serializable]
public class LeaderboardData
{
    public List<ScoreEntry> scores = new List<ScoreEntry>();
}