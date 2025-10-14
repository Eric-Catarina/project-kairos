// Local: Assets/Scripts/UI/PlayerDebugPanel.cs

using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))] // Garante que está em um objeto de UI
public class PlayerDebugPanel : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private TMP_Dropdown playerDropdown;

    private bool _isPopulating = false;

    private void OnEnable()
    {
        // Se inscreve nos eventos quando o painel fica ativo
        PlayerProfile.OnProfileChanged += HandleProfileChanged;
        if (playerDropdown != null)
        {
            playerDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        
        // Garante que a lista esteja atualizada assim que o painel for aberto
        PopulateDropdown();
    }

    private void OnDisable()
    {
        // Se desinscreve para evitar erros quando o painel está inativo
        PlayerProfile.OnProfileChanged -= HandleProfileChanged;
        if (playerDropdown != null)
        {
            playerDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }
    }
    
    // Este método é chamado pelo evento OnProfileChanged
    private void HandleProfileChanged()
    {
        PopulateDropdown();
    }

    private void PopulateDropdown()
    {
        if (playerDropdown == null || PlayerProfile.Instance == null) return;
        
        _isPopulating = true;
        
        playerDropdown.ClearOptions();
        
        var options = PlayerProfile.Instance.AllProfiles
            .Select(p => new TMP_Dropdown.OptionData(p.PlayerName))
            .ToList();
            
        playerDropdown.AddOptions(options);

        // Seleciona o item do dropdown que corresponde ao perfil ativo
        int currentIndex = PlayerProfile.Instance.AllProfiles
            .FindIndex(p => p.PlayerId == PlayerProfile.Instance.CurrentProfile.PlayerId);
            
        if (currentIndex != -1)
        {
            playerDropdown.value = currentIndex;
        }
        
        // Força a atualização do texto principal do dropdown
        playerDropdown.RefreshShownValue();
        
        _isPopulating = false;
    }

    private void OnDropdownValueChanged(int index)
    {
        // Bloqueio para evitar chamadas recursivas enquanto a UI está sendo populada
        if (_isPopulating || PlayerProfile.Instance == null) return;

        // Pega o ID do perfil selecionado na lista
        string selectedPlayerId = PlayerProfile.Instance.AllProfiles[index].PlayerId;
        
        // Só executa a lógica de troca se um perfil diferente for selecionado
        if (PlayerProfile.Instance.CurrentProfile.PlayerId != selectedPlayerId)
        {
            PlayerProfile.Instance.SwitchPlayer(selectedPlayerId);
            PlayFabAuthManager.Instance.Login(); // Força o login com a nova conta
        }
    }

    // Função pública para ser chamada pelo botão "Create New Player" na UI
    public void CreateNewPlayer()
    {
        if (PlayerProfile.Instance == null) return;
        
        // Cria o novo jogador e já o define como ativo.
        // O evento OnProfileChanged será disparado, e o PopulateDropdown será chamado automaticamente.
        PlayerProfile.Instance.CreateNewPlayer(true); 
        PlayFabAuthManager.Instance.Login();
    }
}