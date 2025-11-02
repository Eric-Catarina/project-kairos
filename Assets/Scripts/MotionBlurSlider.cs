// Local: Assets/Scripts/UI/MotionBlurSlider.cs
// (Removido PlayerPrefs, agora usa GameSettingsManager)

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MotionBlurSlider : MonoBehaviour
{
    [Header("Dependências")]
    private Volume globalVolume;

    [Header("Configurações de Clamp")]
    [SerializeField] private float minClamp = 0f;
    [SerializeField] private float maxClamp = 0.02f;

    private Slider _slider;
    private MotionBlur _motionBlur;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        globalVolume = FindFirstObjectByType<Volume>();
        InitializeMotionBlurReference();
    }

    private void Start()
    {
        if (_motionBlur == null)
        {
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

    private void InitializeMotionBlurReference()
    {
        if (globalVolume != null)
        {
            // Maneira mais segura e eficiente de pegar o componente do que iterar a lista
            if (!globalVolume.profile.TryGet(out _motionBlur))
            {
                // Opcional: Adiciona o componente se ele não existir no perfil
                // _motionBlur = globalVolume.profile.Add<MotionBlur>(false);
            }
        }
    }
    private void LoadSavedValue()
    {
        // Pega o valor do nosso novo sistema centralizado
        float savedNormalizedValue = GameSettingsManager.Instance.MotionBlurIntensity;
        
        _slider.SetValueWithoutNotify(savedNormalizedValue);
        ApplyMotionBlurClamp(savedNormalizedValue);
    }

    private void OnSliderValueChanged(float normalizedValue)
    {
        ApplyMotionBlurClamp(normalizedValue);
        // Notifica o GameSettingsManager para atualizar e salvar
        GameSettingsManager.Instance.SetMotionBlur(normalizedValue);
    }

    private void ApplyMotionBlurClamp(float normalizedValue)
    {
        if (_motionBlur != null)
        {
            float newValue = Mathf.Lerp(minClamp, maxClamp, normalizedValue);
            _motionBlur.clamp.Override(newValue);
        }
    }
}