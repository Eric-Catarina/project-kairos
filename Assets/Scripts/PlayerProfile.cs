// Local: Assets/Scripts/Scoring/PlayerProfile.cs

using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance { get; private set; }
    public static event Action OnProfileChanged;

    // Propriedades agora são atalhos para os dados no SaveManager
    public UserProfile CurrentProfile => SaveManager.Instance.GetActiveUserProfile();
    public List<UserProfile> AllProfiles => SaveManager.Instance.Data.profiles;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CreateNewPlayer(bool setActive = false)
    {
        SaveManager.Instance.CreateNewProfile(setActive);
        OnProfileChanged?.Invoke();
    }

    public void SwitchPlayer(string playerId)
    {
        var profileToSwitch = AllProfiles.Find(p => p.profileId == playerId);
        if (profileToSwitch != null && (CurrentProfile == null || CurrentProfile.profileId != playerId))
        {
            SaveManager.Instance.Data.activeProfileId = playerId;
            SaveManager.Instance.SaveGame();
            OnProfileChanged?.Invoke();
        }
    }

    public void UpdateCurrentPlayerName(string newName)
    {
        if (CurrentProfile == null || string.IsNullOrWhiteSpace(newName)) return;

        CurrentProfile.profileName = newName;
        SaveManager.Instance.SaveGame();

        if (PlayFabAuthManager.Instance != null && PlayFabAuthManager.Instance.IsLoggedIn())
        {
            PlayFabAuthManager.Instance.UpdateDisplayName(newName);
        }
        OnProfileChanged?.Invoke();
    }
}