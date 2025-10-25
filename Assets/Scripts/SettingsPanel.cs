// Local: Assets/Scripts/UI/SettingsPanel.cs

using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : UIPanel
{
    [Header("Referências Internas")]
    [Tooltip("O botão 'Voltar' ou 'Fechar' dentro deste painel.")]
    [SerializeField] private Button backButton;
    [Header("Referências Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [Header("Referências Toggles")]
    [SerializeField] private Toggle masterToggle;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;

    private void Awake()
    {
        SetupButtonListeners();
	SetupVolumeListeners();
	SetupToggleListeners();
    }

    private void SetupVolumeListeners()
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError("AudioManager.instance não foi encontrado. Os sliders de volume não funcionarão.");
            return;
        }

        masterSlider?.onValueChanged.RemoveAllListeners();
        musicSlider?.onValueChanged.RemoveAllListeners();
        sfxSlider?.onValueChanged.RemoveAllListeners();

        masterSlider?.onValueChanged.AddListener(AudioManager.instance.MasterVolume);
        musicSlider?.onValueChanged.AddListener(AudioManager.instance.MusicVolume);
        sfxSlider?.onValueChanged.AddListener(AudioManager.instance.SFXVolume);
    }

    private void SetupToggleListeners()
    {
        if (AudioManager.instance == null) return;

        masterToggle?.onValueChanged.RemoveAllListeners();
        musicToggle?.onValueChanged.RemoveAllListeners();
        sfxToggle?.onValueChanged.RemoveAllListeners();

        masterToggle?.onValueChanged.AddListener(value => AudioManager.instance.SetMasterMute(!value));
        musicToggle?.onValueChanged.AddListener(value => AudioManager.instance.SetMusicMute(!value));
        sfxToggle?.onValueChanged.AddListener(value => AudioManager.instance.SetSFXMute(!value));
    }

    private void SetupButtonListeners()
    {
        // Remove listeners antigos do Inspector para evitar chamadas duplicadas
        backButton?.onClick.RemoveAllListeners();

        // Atribui o listener via código, garantindo a referência correta ao Singleton
        if (UIManager.Instance != null)
        {
            if (PanelType == UIPanelType.Settings)
            {
                backButton?.onClick.AddListener(UIManager.Instance.CloseSettingsPanel);
            }
            else
            {
                backButton?.onClick.AddListener(UIManager.Instance.CloseLevelSelectPanel);
            }
        }
        else
        {
            Debug.LogError("UIManager.Instance não foi encontrado. O botão 'Back' do SettingsPanel não funcionará.");
        }
    }
}