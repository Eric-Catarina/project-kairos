// Local: Assets/Scripts/Core/PlayFabAuthManager.cs

using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabAuthManager : MonoBehaviour
{
    public static PlayFabAuthManager Instance { get; private set; }
    
    public string PlayFabId { get; private set; }

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
        // Garante que o PlayerProfile seja inicializado antes de tentar o login
        if (PlayerProfile.Instance != null)
        {
            Login();
        }
        else
        {
            Debug.LogError("PlayerProfile não encontrado! O login no PlayFab falhará.");
        }
    }

    private void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = PlayerProfile.Instance.PlayerId,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true // Pede para retornar o perfil do jogador
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        PlayFabId = result.PlayFabId;
        Debug.Log($"<color=green>Login no PlayFab bem-sucedido! PlayFab ID: {PlayFabId}</color>");
        
        // Sincroniza o nome de exibição do PlayFab com o nome atual do PlayerProfile
        // Se o nome de exibição já for o mesmo, a chamada não fará nada prejudicial.
        string currentPlayFabName = result.InfoResultPayload?.PlayerProfile?.DisplayName;
        string localPlayerName = PlayerProfile.Instance.PlayerName;

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
}