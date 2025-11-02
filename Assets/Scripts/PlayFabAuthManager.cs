// Local: Assets/Scripts/Core/PlayFabAuthManager.cs

using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayFabAuthManager : MonoBehaviour
{
    public static PlayFabAuthManager Instance { get; private set; }
    
    public string PlayFabId { get; private set; }
    private string _loggedInCustomId;

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

    private void Start()
    {
        if (PlayerProfile.Instance != null)
        {
            Login();
        }
        else
        {
            Debug.LogError("PlayerProfile não encontrado! O login no PlayFab falhará.");
        }
    }

    public void Login()
    {
        if (IsLoggedIn() && _loggedInCustomId == PlayerProfile.Instance.CurrentProfile.profileId)
        {
            Debug.Log("<color=green>Já está logado com a conta correta.</color>");
            return;
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = PlayerProfile.Instance.CurrentProfile.profileId,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        PlayFabId = result.PlayFabId;
        _loggedInCustomId = PlayerProfile.Instance.CurrentProfile.profileId;
        Debug.Log($"<color=green>Login no PlayFab bem-sucedido! PlayFab ID: {PlayFabId}</color>");
        
        string currentPlayFabName = result.InfoResultPayload?.PlayerProfile?.DisplayName;
        string localPlayerName = PlayerProfile.Instance.CurrentProfile.profileName;

        if (currentPlayFabName != localPlayerName)
        {
            UpdateDisplayName(localPlayerName);
        }
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Falha no login do PlayFab: " + error.GenerateErrorReport());
    }

    public void UpdateDisplayName(string displayName)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, 
            (res) => {
                Debug.Log($"Nome de exibição no PlayFab atualizado para: {res.DisplayName}");
            },
            (err) => {
                Debug.LogError("Falha ao atualizar nome de exibição: " + err.GenerateErrorReport());
            }
        );
    }

    public bool IsLoggedIn()
    {
        return PlayFabClientAPI.IsClientLoggedIn();
    }
}