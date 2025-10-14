// Local: Assets/Scripts/Scoring/PlayerProfile.cs

using UnityEngine;
using System;

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance { get; private set; }

    [Header("Debug")]
    [Tooltip("Defina um nome aqui para substituir o nome salvo durante o teste no Editor.")]
    [SerializeField] private string debugPlayerName;
    
    public string PlayerId { get; private set; }
    public string PlayerName { get; private set; }

    private const string PlayerIdKey = "PlayerId";
    private const string PlayerNameKey = "PlayerName";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadPlayerProfile();
    }

    private void LoadPlayerProfile()
    {
        PlayerId = PlayerPrefs.GetString(PlayerIdKey, Guid.NewGuid().ToString());
        
#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(debugPlayerName))
        {
            PlayerName = debugPlayerName;
        }
        else
        {
            PlayerName = PlayerPrefs.GetString(PlayerNameKey, $"Player{UnityEngine.Random.Range(1000, 9999)}");
        }
#else
        PlayerName = PlayerPrefs.GetString(PlayerNameKey, $"Player{UnityEngine.Random.Range(1000, 9999)}");
#endif
        
        PlayerPrefs.SetString(PlayerIdKey, PlayerId);
        PlayerPrefs.SetString(PlayerNameKey, PlayerName);
        PlayerPrefs.Save();
    }

    public void SetPlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;

        PlayerName = newName;
        PlayerPrefs.SetString(PlayerNameKey, PlayerName);
        PlayerPrefs.Save();
        
        // Se o sistema de autenticação já estiver pronto, atualiza o nome no PlayFab
        if (PlayFabAuthManager.Instance != null && !string.IsNullOrEmpty(PlayFabAuthManager.Instance.PlayFabId))
        {
            PlayFabAuthManager.Instance.UpdateDisplayName(newName);
        }
    }
}