// Local: Assets/Scripts/UI/PlayerDebugPanel.cs

using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerDebugPanel : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private TMP_Dropdown playerDropdown;

    private bool _isPopulating = false;

    private void OnEnable()
    {
        PlayerProfile.OnProfileChanged += PopulateDropdown;
        playerDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        PopulateDropdown();
    }

    private void OnDisable()
    {
        PlayerProfile.OnProfileChanged -= PopulateDropdown;
        playerDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }
    
    private void PopulateDropdown()
    {
        if (PlayerProfile.Instance == null) return;
        
        _isPopulating = true;
        
        playerDropdown.ClearOptions();
        
        var options = PlayerProfile.Instance.AllProfiles
            .Select(p => new TMP_Dropdown.OptionData(p.PlayerName))
            .ToList();
            
        playerDropdown.AddOptions(options);

        int currentIndex = PlayerProfile.Instance.AllProfiles
            .FindIndex(p => p.PlayerId == PlayerProfile.Instance.CurrentProfile.PlayerId);
            
        if (currentIndex != -1)
        {
            playerDropdown.value = currentIndex;
        }
        
        playerDropdown.RefreshShownValue();
        _isPopulating = false;
    }

    private void OnDropdownValueChanged(int index)
    {
        if (_isPopulating || PlayerProfile.Instance == null) return;

        string selectedPlayerId = PlayerProfile.Instance.AllProfiles[index].PlayerId;
        
        // Evita recarregar se o mesmo perfil for selecionado
        if (PlayerProfile.Instance.CurrentProfile.PlayerId != selectedPlayerId)
        {
            PlayerProfile.Instance.SwitchPlayer(selectedPlayerId);
            PlayFabAuthManager.Instance.Login();
        }
    }

    // Função pública para ser chamada pelo botão na UI
    public void CreateNewPlayer()
    {
        if (PlayerProfile.Instance == null) return;
        
        PlayerProfile.Instance.CreateNewPlayer(true); // Cria e define como ativo
        PlayFabAuthManager.Instance.Login();
    }
}