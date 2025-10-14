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
        PlayerProfile.OnProfileChanged += UpdateFieldText; // Se inscreve no evento
        UpdateFieldText(); // Atualiza o texto inicial
    }

    private void OnDisable()
    {
        _nameInputField.onEndEdit.RemoveListener(UpdatePlayerName);
        PlayerProfile.OnProfileChanged -= UpdateFieldText; // Se desinscreve do evento
    }
    
    private void UpdateFieldText()
    {
        if (PlayerProfile.Instance?.CurrentProfile != null)
        {
            _nameInputField.text = PlayerProfile.Instance.CurrentProfile.PlayerName;
        }
    }

    private void UpdatePlayerName(string newName)
    {
        if (PlayerProfile.Instance == null) return;
        
        if (PlayerProfile.Instance.CurrentProfile != null && PlayerProfile.Instance.CurrentProfile.PlayerName == newName)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(newName))
        {
            PlayerProfile.Instance.UpdateCurrentPlayerName(newName);
        }
        else
        {
            UpdateFieldText(); // Reverte para o nome atual se o campo ficar vazio
        }
    }
}