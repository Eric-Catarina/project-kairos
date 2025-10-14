// Local: Assets/Scripts/Core/PlayFabAuthManager.cs

using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayFabAuthManager : MonoBehaviour
{
    public static PlayFabAuthManager Instance { get; private set; }
    
    public string PlayFabId { get; private set; }
    private string _loggedInCustomId; // Armazena o ID do jogador que está logado no momento

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
        // O login agora é acionado pelo PlayerProfile para garantir a ordem correta
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
        // Se já estamos logados com a conta correta, não faz nada
        if (IsLoggedIn() && _loggedInCustomId == PlayerProfile.Instance.CurrentProfile.PlayerId)
        {
            Debug.Log("<color=green>Já está logado com a conta correta.</color>");
            return;
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = PlayerProfile.Instance.CurrentProfile.PlayerId,
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
        _loggedInCustomId = PlayerProfile.Instance.CurrentProfile.PlayerId;
        Debug.Log($"<color=green>Login no PlayFab bem-sucedido! PlayFab ID: {PlayFabId}</color>");
        
        string currentPlayFabName = result.InfoResultPayload?.PlayerProfile?.DisplayName;
        string localPlayerName = PlayerProfile.Instance.CurrentProfile.PlayerName;

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
            (result) => {
                Debug.Log($"Nome de exibição no PlayFab atualizado para: {result.DisplayName}");
            },
            (error) => {
                Debug.LogError("Falha ao atualizar nome de exibição: " + error.GenerateErrorReport());
            }
        );
    }

    public bool IsLoggedIn()
    {
        return PlayFabClientAPI.IsClientLoggedIn();
    }
}