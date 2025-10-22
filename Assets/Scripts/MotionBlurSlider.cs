// Local: Assets/Scripts/UI/MotionBlurSlider.cs

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MotionBlurSlider : MonoBehaviour
{
    [Header("Dependências")]
    [Tooltip("Arraste aqui o Volume Global que contém o override de Motion Blur.")]
    [SerializeField] private Volume globalVolume;

    [Header("Configurações de Clamp")]
    [SerializeField] private float minClamp = 0f;
    [SerializeField] private float maxClamp = 0.02f;

    private Slider _slider;
    private MotionBlur _motionBlur;
    private const string ClampPrefKey = "Settings_MotionBlurClamp";

    private void Awake()
    {
        _slider = GetComponent<Slider>();

        if (globalVolume == null)
        {
            // Tenta encontrar automaticamente caso esqueça de arrastar no inspector
            globalVolume = FindFirstObjectByType<Volume>();
        }

        InitializeMotionBlurReference();
    }

    private void Start()
    {
        if (_motionBlur == null)
        {
            Debug.LogWarning("Motion Blur não encontrado no Volume atribuído. O slider será desativado.", this);
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
        // Usa o valor atual do perfil como padrão caso não tenha nada salvo ainda
        float currentProfileValue = _motionBlur.clamp.value;
        float defaultNormalizedValue = Mathf.InverseLerp(minClamp, maxClamp, currentProfileValue);
        
        float savedNormalizedValue = PlayerPrefs.GetFloat(ClampPrefKey, defaultNormalizedValue);

        // Atualiza o slider sem disparar o evento onValueChanged para evitar salvamento redundante no Start
        _slider.SetValueWithoutNotify(savedNormalizedValue);
        
        // Garante que o efeito visual já comece correto
        ApplyMotionBlurClamp(savedNormalizedValue);
    }

    private void OnSliderValueChanged(float normalizedValue)
    {
        ApplyMotionBlurClamp(normalizedValue);
        SaveValue(normalizedValue);
    }

    private void ApplyMotionBlurClamp(float normalizedValue)
    {
        if (_motionBlur != null)
        {
            float newValue = Mathf.Lerp(minClamp, maxClamp, normalizedValue);
            // .Override garante que a propriedade seja ativada no volume
            _motionBlur.clamp.Override(newValue);
        }
    }

    private void SaveValue(float value)
    {
        PlayerPrefs.SetFloat(ClampPrefKey, value);
        PlayerPrefs.Save();
    }
}