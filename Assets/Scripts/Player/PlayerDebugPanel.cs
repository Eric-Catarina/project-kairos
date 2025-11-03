
// Local: Assets/Scripts/UI/PlayerDebugPanel.cs

using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PlayerDebugPanel : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private TMP_Dropdown playerDropdown;

    private bool _isPopulating = false;

    private void OnEnable()
    {
        PlayerProfile.OnProfileChanged += HandleProfileChanged;
        if (playerDropdown != null)
        {
            playerDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        PopulateDropdown();
    }

    private void OnDisable()
    {
        PlayerProfile.OnProfileChanged -= HandleProfileChanged;
        if (playerDropdown != null)
        {
            playerDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }
    }
    
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
            .Select(p => new TMP_Dropdown.OptionData(p.profileName))
            .ToList();
            
        playerDropdown.AddOptions(options);

        int currentIndex = PlayerProfile.Instance.AllProfiles
            .FindIndex(p => p.profileId == PlayerProfile.Instance.CurrentProfile.profileId);
            
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

        string selectedPlayerId = PlayerProfile.Instance.AllProfiles[index].profileId;
        
        if (PlayerProfile.Instance.CurrentProfile.profileId != selectedPlayerId)
        {
            PlayerProfile.Instance.SwitchPlayer(selectedPlayerId);
            PlayFabAuthManager.Instance.Login();
        }
    }

    public void CreateNewPlayer()
    {
        if (PlayerProfile.Instance == null) return;
        
        PlayerProfile.Instance.CreateNewPlayer(true); 
        PlayFabAuthManager.Instance.Login();
    }
}