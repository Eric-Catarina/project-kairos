using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class CheckpointsToggle : MonoBehaviour
{
    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogError("GameSettingsManager não encontrado. O Toggle de Checkpoints será desativado.", this);
            _toggle.interactable = false;
            return;
        }

        LoadInitialState();
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        GameSettingsManager.OnCheckpointsEnabledChanged += HandleSettingChanged;
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.OnCheckpointsEnabledChanged -= HandleSettingChanged;
        }
    }

    private void LoadInitialState()
    {
        _toggle.SetIsOnWithoutNotify(GameSettingsManager.Instance.CheckpointsEnabled);
    }

    private void OnToggleValueChanged(bool value)
    {
        GameSettingsManager.Instance.SetCheckpointsEnabled(value);
    }

    private void HandleSettingChanged(bool isEnabled)
    {
        _toggle.SetIsOnWithoutNotify(isEnabled);
    }
}