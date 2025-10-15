// Local: Assets/Scripts/Scoring/PlayerProfile.cs

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class PlayerProfileData
{
    public string PlayerId;
    public string PlayerName;
}

[Serializable]
public class PlayerProfileList
{
    public List<PlayerProfileData> Profiles = new List<PlayerProfileData>();
}

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance { get; private set; }
    public static event Action OnProfileChanged;

    public PlayerProfileData CurrentProfile { get; private set; }
    public List<PlayerProfileData> AllProfiles { get; private set; } = new List<PlayerProfileData>();

    private const string AllProfilesKey = "AllPlayerProfiles";
    private const string LastProfileIdKey = "LastPlayerProfileId";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllProfiles();
        LoadLastUsedProfile();
    }

    private void LoadAllProfiles()
    {
        string json = PlayerPrefs.GetString(AllProfilesKey, "{}");
        PlayerProfileList list = JsonUtility.FromJson<PlayerProfileList>(json);
        AllProfiles = list.Profiles ?? new List<PlayerProfileData>();
    }

    private void LoadLastUsedProfile()
    {
        string lastUsedId = PlayerPrefs.GetString(LastProfileIdKey, null);
        CurrentProfile = AllProfiles.FirstOrDefault(p => p.PlayerId == lastUsedId);

        if (CurrentProfile == null)
        {
            CreateNewPlayer(true);
        }
    }

    public void CreateNewPlayer(bool setActive = false)
    {
        var newProfile = new PlayerProfileData
        {
            PlayerId = Guid.NewGuid().ToString(),
            PlayerName = $"Player{UnityEngine.Random.Range(1000, 9999)}"
        };
        
        AllProfiles.Add(newProfile); // Apenas ADICIONA à lista
        
        if (setActive)
        {
            SwitchPlayer(newProfile.PlayerId); // Troca para o novo perfil sem salvar (SwitchPlayer faz isso)
        }
        else
        {
            SaveChanges();
            OnProfileChanged?.Invoke();
        }
    }

    public void SwitchPlayer(string playerId)
    {
        var profileToSwitch = AllProfiles.FirstOrDefault(p => p.PlayerId == playerId);
        if (profileToSwitch != null && (CurrentProfile == null || CurrentProfile.PlayerId != playerId))
        {
            CurrentProfile = profileToSwitch;
            PlayerPrefs.SetString(LastProfileIdKey, CurrentProfile.PlayerId);
            SaveChanges(); // Salva a lista completa e o novo ID ativo
            OnProfileChanged?.Invoke();
        }
    }

    public void UpdateCurrentPlayerName(string newName)
    {
        if (CurrentProfile == null || string.IsNullOrWhiteSpace(newName)) return;

        CurrentProfile.PlayerName = newName;
        SaveChanges();

        if (PlayFabAuthManager.Instance != null && PlayFabAuthManager.Instance.IsLoggedIn())
        {
            PlayFabAuthManager.Instance.UpdateDisplayName(newName);
        }
        OnProfileChanged?.Invoke();
    }

    private void SaveChanges()
    {
        PlayerProfileList list = new PlayerProfileList { Profiles = AllProfiles };
        string json = JsonUtility.ToJson(list);
        PlayerPrefs.SetString(AllProfilesKey, json);
        PlayerPrefs.Save();
    }
}