// Local: Assets/Scripts/UI/PlayerDropdownController.cs

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class PlayerDropdownController : MonoBehaviour
{
    private TMP_Dropdown _playerDropdown;
    private bool _isPopulating = false;

    private void Awake()
    {
        _playerDropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        PlayerProfile.OnProfileChanged += HandleProfileChanged;
        _playerDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        PopulateDropdown();
    }

    private void OnDisable()
    {
        PlayerProfile.OnProfileChanged -= HandleProfileChanged;
        _playerDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }
    
    private void HandleProfileChanged()
    {
        PopulateDropdown();
    }

    private void PopulateDropdown()
    {
        if (PlayerProfile.Instance == null) return;
        
        _isPopulating = true;
        _playerDropdown.ClearOptions();
        
        var options = PlayerProfile.Instance.AllProfiles
            .Select(p => new TMP_Dropdown.OptionData(p.profileName))
            .ToList();
        _playerDropdown.AddOptions(options);

        int currentIndex = PlayerProfile.Instance.AllProfiles
            .FindIndex(p => p.profileId == PlayerProfile.Instance.CurrentProfile.profileId);
            
        if (currentIndex != -1)
        {
            _playerDropdown.value = currentIndex;
        }
        
        _playerDropdown.RefreshShownValue();
        _isPopulating = false;
    }

    private void OnDropdownValueChanged(int index)
    {
        if (_isPopulating || PlayerProfile.Instance == null) return;

        string selectedPlayerId = PlayerProfile.Instance.AllProfiles[index].profileId;
        PlayerProfile.Instance.SwitchPlayer(selectedPlayerId);
        PlayFabAuthManager.Instance.Login();
    }
}