// Local: Assets/Scripts/MouseSensitivitySlider.cs

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MouseSensitivitySlider : MonoBehaviour
{
    // Enum para selecionar o eixo no Inspector
    public enum Axis { X, Y }

    [Tooltip("Define qual eixo de sensibilidade este slider irá controlar.")]
    [SerializeField] private Axis controlledAxis = Axis.X;
    
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogError("GameSettingsManager não encontrado na cena! O slider de sensibilidade não funcionará.");
            gameObject.SetActive(false);
            return;
        }

        // Configura o valor inicial e o listener com base no eixo selecionado
        switch (controlledAxis)
        {
            case Axis.X:
                _slider.value = GameSettingsManager.Instance.MouseSensitivityX;
                _slider.onValueChanged.AddListener(GameSettingsManager.Instance.SetMouseSensitivityX);
                break;
            case Axis.Y:
                _slider.value = GameSettingsManager.Instance.MouseSensitivityY;
                _slider.onValueChanged.AddListener(GameSettingsManager.Instance.SetMouseSensitivityY);
                break;
        }
    }

    private void OnDestroy()
    {
        // Remove o listener correto para evitar erros
        if (_slider != null && GameSettingsManager.Instance != null)
        {
            switch (controlledAxis)
            {
                case Axis.X:
                    _slider.onValueChanged.RemoveListener(GameSettingsManager.Instance.SetMouseSensitivityX);
                    break;
                case Axis.Y:
                    _slider.onValueChanged.RemoveListener(GameSettingsManager.Instance.SetMouseSensitivityY);
                    break;
            }
        }
    }
}