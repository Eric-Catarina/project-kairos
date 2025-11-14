using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
public GameSettings settings = new GameSettings();
public List<UserProfile> profiles = new List<UserProfile>();
public string activeProfileId = "";
}

[Serializable]
public class GameSettings
{
public float masterVolume = 1f;
public float musicVolume = 1f;
public float sfxVolume = 1f;
public bool isMasterMuted = false;
public bool isMusicMuted = false;
public bool isSfxMuted = false;
public float mouseSensitivityX = 0.5f;
public float mouseSensitivityY = 0.5f;
public bool invertMouseX = false;
public bool invertMouseY = false;

public float motionBlurIntensity = 0.5f;
public bool checkpointsEnabled = true;

}

[Serializable]
public class UserProfile : ISerializationCallbackReceiver
{
public string profileId;
public string profileName;


[NonSerialized]
public Dictionary<string, LevelRecord> levelRecords = new Dictionary<string, LevelRecord>();

[SerializeField] private List<string> _levelIds = new List<string>();
[SerializeField] private List<LevelRecord> _records = new List<LevelRecord>();

public void OnBeforeSerialize()
{
    _levelIds.Clear();
    _records.Clear();
    foreach (var kvp in levelRecords)
    {
        _levelIds.Add(kvp.Key);
        _records.Add(kvp.Value);
    }
}

public void OnAfterDeserialize()
{
    levelRecords = new Dictionary<string, LevelRecord>();
    for (int i = 0; i < _levelIds.Count; i++)
    {
        levelRecords.Add(_levelIds[i], _records[i]);
    }
}

  

}

[Serializable]
public class LevelRecord
{
public float bestTime = float.MaxValue;
public Rank bestRank = Rank.NA;
public int totalDeaths = 0;
}