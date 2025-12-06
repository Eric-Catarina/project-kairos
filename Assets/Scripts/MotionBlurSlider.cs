using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MotionBlurSlider : MonoBehaviour
{
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogError("GameSettingsManager não encontrado. O Slider de Motion Blur será desativado.");
            _slider.interactable = false;
            return;
        }

        LoadSavedValue();
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDestroy()
    {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void LoadSavedValue()
    {
        // Apenas atualiza a visualização do slider
        float savedNormalizedValue = GameSettingsManager.Instance.MotionBlurIntensity;
        _slider.SetValueWithoutNotify(savedNormalizedValue);
    }

    private void OnSliderValueChanged(float normalizedValue)
    {
        // Delega a lógica para o Manager
        GameSettingsManager.Instance.SetMotionBlur(normalizedValue);
    }
}