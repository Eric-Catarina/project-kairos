// Local: Assets/Scripts/UI/PlayerNameInputController.cs

using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class PlayerNameInputController : MonoBehaviour
{
    private TMP_InputField _nameInputField;

    private void Awake()
    {
        _nameInputField = GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        _nameInputField.onEndEdit.AddListener(UpdatePlayerName);
        PlayerProfile.OnProfileChanged += UpdateFieldText;
        UpdateFieldText();
    }

    private void OnDisable()
    {
        _nameInputField.onEndEdit.RemoveListener(UpdatePlayerName);
        PlayerProfile.OnProfileChanged -= UpdateFieldText;
    }
    
    private void UpdateFieldText()
    {
        if (PlayerProfile.Instance?.CurrentProfile != null)
        {
            _nameInputField.text = PlayerProfile.Instance.CurrentProfile.profileName;
        }
    }

    private void UpdatePlayerName(string newName)
    {
        if (PlayerProfile.Instance == null) return;
        
        if (PlayerProfile.Instance.CurrentProfile != null && PlayerProfile.Instance.CurrentProfile.profileName == newName)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(newName))
        {
            PlayerProfile.Instance.UpdateCurrentPlayerName(newName);
        }
        else
        {
            UpdateFieldText();
        }
    }
}

