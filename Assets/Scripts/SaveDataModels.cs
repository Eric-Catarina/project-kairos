// Local: Assets/Scripts/Saving/SaveDataModels.cs

using System;
using System.Collections.Generic;
using UnityEngine;

// O contêiner principal para todos os dados que serão salvos.
[Serializable]
public class GameData
{
    public GameSettings settings = new GameSettings();
    public List<UserProfile> profiles = new List<UserProfile>();
    public string activeProfileId = ""; // ID do último perfil usado
}

// Todas as configurações de jogo que não dependem de um perfil de jogador.
[Serializable]
public class GameSettings
{
    // Audio
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public bool isMasterMuted = false;
    public bool isMusicMuted = false;
    public bool isSfxMuted = false;
    
    // Controles
    public float mouseSensitivityX = 0.5f;
    public float mouseSensitivityY = 0.5f;
    public bool invertMouseX = false;
    public bool invertMouseY = false;

    // Gráficos
    public float motionBlurIntensity = 0.5f; // Normalizado de 0 a 1
}

// Representa um perfil de jogador.
[Serializable]
public class UserProfile : ISerializationCallbackReceiver
{
    public string profileId;
    public string profileName;

    // Dicionários não são serializados diretamente pelo JsonUtility.
    // Usaremos esta lista como um intermediário para salvar e carregar.
    [NonSerialized]
    public Dictionary<string, LevelRecord> levelRecords = new Dictionary<string, LevelRecord>();

    // Campos que o JsonUtility pode serializar
    [SerializeField] private List<string> _levelIds = new List<string>();
    [SerializeField] private List<LevelRecord> _records = new List<LevelRecord>();

    // Antes de salvar, move os dados do dicionário para as listas.
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

    // Depois de carregar, move os dados das listas de volta para o dicionário.
    public void OnAfterDeserialize()
    {
        levelRecords = new Dictionary<string, LevelRecord>();
        for (int i = 0; i < _levelIds.Count; i++)
        {
            levelRecords.Add(_levelIds[i], _records[i]);
        }
    }
}

// Armazena o progresso de um jogador em um nível específico.
[Serializable]
public class LevelRecord
{
    public float bestTime = float.MaxValue;
    public Rank bestRank = Rank.None;
    // Futuramente: public bool hasAchievedNoDamage = false;
}