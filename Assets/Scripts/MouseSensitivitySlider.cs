// Local: Assets/Scripts/UI/MouseSensitivitySlider.cs

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MouseSensitivitySlider : MonoBehaviour
{
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        // Garante que o GameSettingsManager exista.
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogError("GameSettingsManager não encontrado na cena! O slider de sensibilidade não funcionará.");
            gameObject.SetActive(false);
            return;
        }

        // Define o valor inicial do slider com base na configuração salva.
        _slider.value = GameSettingsManager.Instance.MouseSensitivity;

        // Adiciona um listener para chamar o GameSettingsManager quando o valor do slider mudar.
        _slider.onValueChanged.AddListener(GameSettingsManager.Instance.SetMouseSensitivity);
    }

    private void OnDestroy()
    {
        // Remove o listener para evitar erros se o objeto for destruído.
        if (_slider != null)
        {
            _slider.onValueChanged.RemoveListener(GameSettingsManager.Instance.SetMouseSensitivity);
        }
    }
}