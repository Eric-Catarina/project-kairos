// Local: Assets/Scripts/UI/MotionBlurSlider.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Slider))]
public class MotionBlurSlider : MonoBehaviour
{
    [Header("Configurações de Clamp")]
    [SerializeField] private float minClamp = 0f;
    [SerializeField] private float maxClamp = 0.02f;

    private Slider _slider;
    private readonly List<MotionBlur> _motionBlurOverrides = new();

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        InitializeMotionBlurReferences();
    }

    private void Start()
    {
        if (_motionBlurOverrides.Count == 0)
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

    private void InitializeMotionBlurReferences()
    {
        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
        foreach (var volume in volumes)
        {
            if (volume.profile != null && volume.profile.TryGet(out MotionBlur motionBlur))
            {
                _motionBlurOverrides.Add(motionBlur);
            }
        }
    }

    private void LoadSavedValue()
    {
        float savedNormalizedValue = GameSettingsManager.Instance.MotionBlurIntensity;
        _slider.SetValueWithoutNotify(savedNormalizedValue);
        ApplyMotionBlurToAllVolumes(savedNormalizedValue);
    }

    private void OnSliderValueChanged(float normalizedValue)
    {
        ApplyMotionBlurToAllVolumes(normalizedValue);
        GameSettingsManager.Instance.SetMotionBlur(normalizedValue);
    }

    private void ApplyMotionBlurToAllVolumes(float normalizedValue)
    {
        float newValue = Mathf.Lerp(minClamp, maxClamp, normalizedValue);

        foreach (var motionBlur in _motionBlurOverrides)
        {
            if (motionBlur != null)
            {
                motionBlur.clamp.Override(newValue);
            }
        }
    }
}