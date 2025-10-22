// Local: Assets/Scripts/UI/InvertMouseToggle.cs

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class InvertMouseToggle : MonoBehaviour
{
    public enum AxisToInvert { X, Y }

    [Header("Configuração do Eixo")]
    [Tooltip("Define qual eixo este toggle irá controlar.")]
    [SerializeField] private AxisToInvert axisToControl;
    
    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    private void Start()
    {
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogError("GameSettingsManager não foi encontrado. O toggle de inversão será desativado.", this);
            _toggle.interactable = false;
            return;
        }

        LoadInitialValue();
    }

    private void LoadInitialValue()
    {
        switch (axisToControl)
        {
            case AxisToInvert.X:
                _toggle.isOn = GameSettingsManager.Instance.InvertMouseX;
                break;
            case AxisToInvert.Y:
                _toggle.isOn = GameSettingsManager.Instance.InvertMouseY;
                break;
        }
    }

    private void OnToggleValueChanged(bool value)
    {
        if (GameSettingsManager.Instance == null) return;

        switch (axisToControl)
        {
            case AxisToInvert.X:
                GameSettingsManager.Instance.SetInvertX(value);
                break;
            case AxisToInvert.Y:
                GameSettingsManager.Instance.SetInvertY(value);
                break;
        }
    }
}