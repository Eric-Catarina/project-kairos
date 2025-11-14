using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : UIPanel
{
    [Header("Referências Internas")]
    [SerializeField] private Button backButton;
    [Header("Referências Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider;
    [Header("Referências Toggles")]
    [SerializeField] private Toggle masterToggle;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle ambientToggle;
    [SerializeField] private Toggle checkpointsToggle;

    private void Awake()
    {
        base.Awake();
        SetupButtonListeners();
        SetupVolumeListeners();
        SetupToggleListeners();
        InitializeSliders();
    }

    private void OnEnable()
    {
        LoadToggleStates();
    }

    private void LoadToggleStates()
    {
        if (GameSettingsManager.Instance != null && checkpointsToggle != null)
        {
            checkpointsToggle.isOn = GameSettingsManager.Instance.CheckpointsEnabled;
        }
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
        ambientSlider?.onValueChanged.RemoveAllListeners();

        masterSlider?.onValueChanged.AddListener(AudioManager.instance.MasterVolume);
        musicSlider?.onValueChanged.AddListener(AudioManager.instance.MusicVolume);
        sfxSlider?.onValueChanged.AddListener(AudioManager.instance.SFXVolume);
        ambientSlider?.onValueChanged.AddListener(AudioManager.instance.AmbientVolume);
    }

    private void SetupToggleListeners()
    {
        if (AudioManager.instance != null)
        {
            masterToggle?.onValueChanged.RemoveAllListeners();
            musicToggle?.onValueChanged.RemoveAllListeners();
            sfxToggle?.onValueChanged.RemoveAllListeners();
            ambientToggle?.onValueChanged.RemoveAllListeners();

            masterToggle?.onValueChanged.AddListener(value => AudioManager.instance.SetMasterMute(!value));
            musicToggle?.onValueChanged.AddListener(value => AudioManager.instance.SetMusicMute(!value));
            sfxToggle?.onValueChanged.AddListener(value => AudioManager.instance.SetSFXMute(!value));
            ambientToggle?.onValueChanged.AddListener(value => AudioManager.instance.SetAmbientMute(!value));
        }

        if (GameSettingsManager.Instance != null)
        {
            checkpointsToggle?.onValueChanged.RemoveAllListeners();
            checkpointsToggle?.onValueChanged.AddListener(GameSettingsManager.Instance.SetCheckpointsEnabled);
        }
    }

    private void SetupButtonListeners()
    {
        backButton?.onClick.RemoveAllListeners();

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
    
    private void InitializeSliders()
    {
        if (AudioManager.instance == null) return;

        if (masterSlider != null)
        {
            masterSlider.value = AudioManager.instance.GetMasterVolumeBase();
        }

        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.instance.GetMusicVolumeBase();
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.instance.GetSFXVolumeBase();
        }

        if (ambientSlider != null)
        {
            ambientSlider.value = AudioManager.instance.GetAmbientVolumeBase();
        }

        if (masterToggle != null)
        {
            masterToggle.isOn = !AudioManager.instance.masterSource.mute;
        }

        if (musicToggle != null)
        {
            musicToggle.isOn = !AudioManager.instance.musicSource.mute;
        }

        if (sfxToggle != null)
        {
            sfxToggle.isOn = !AudioManager.instance.sfxSource.mute;
        }

        if (ambientToggle != null)
        {
            ambientToggle.isOn = !AudioManager.instance.ambientSource.mute;
        }
    }
}