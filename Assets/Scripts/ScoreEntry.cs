// Local: Assets/Scripts/Scoring/ScoreEntry.cs

using System;

[Serializable]
public class ScoreEntry
{
    public string playerId;
    public string playerName;
    public float scoreTime;
    public string levelId;
    public long timestamp;

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
    public System.Collections.Generic.List<ScoreEntry> scores = new System.Collections.Generic.List<ScoreEntry>();
}