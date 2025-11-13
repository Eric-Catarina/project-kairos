using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


[Serializable]
public class SceneMusic
{
    public GameScene scene;
    public string musicName;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] musicSounds, ambientSounds, sfxSounds;
    public AudioSource masterSource, musicSource, sfxSource, ambientSource;
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioSource> loopingSources = new Dictionary<string, AudioSource>();

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float sfxPitchVariation = 0.1f;

    [Header("Músicas por Cena (Enum)")]
    [SerializeField] private SceneMusic[] sceneMusics;

    [Header("Ambientes por Cena (Enum)")]
    [SerializeField] private SceneMusic[] sceneAmbients;

    private float lastSFXFeedbackTime;
    private const float feedbackCooldown = 0.15f;

    private void Awake()
    {
        AddSoundsToDictionary(sfxSounds);
        AddSoundsToDictionary(musicSounds);
        AddSoundsToDictionary(ambientSounds);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadVolumes();
    }



    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterSource.volume);
        PlayerPrefs.SetFloat("MusicVolume", musicSource.volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxSource.volume);
        PlayerPrefs.Save();
    }

    public void LoadVolumes()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        MasterVolume(master);
        MusicVolume(music);
        SFXVolume(sfx);
    }

    private void Start()
    {
        GameScene currentSceneEnum = GetSceneEnum(SceneManager.GetActiveScene().name);
        PlaySceneMusic(currentSceneEnum);
    }

    private void AddSoundsToDictionary(Sound[] soundArray)
    {
        foreach (Sound s in soundArray)
        {
            if (!sfxDictionary.ContainsKey(s.name))
            {
                sfxDictionary.Add(s.name, s.clip);
            }
        }
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, x => x.name == name);

        if (s == null)
        {
            Debug.LogWarning($"Sound Not Found: {name}");
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.volume = musicSource.volume * masterSource.volume;
            musicSource.Play();
        }
    }

    public void PlayAmbient(string name)
    {
        Sound s = Array.Find(ambientSounds, x => x.name == name);
        if (s == null)
        {
            Debug.LogWarning("Ambient sound not found: " + name);
        }
        else
        {
            ambientSource.clip = s.clip;
            ambientSource.volume = ambientSource.volume * masterSource.volume;
            ambientSource.loop = true;
            ambientSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            float randomPitch = UnityEngine.Random.Range(1f - sfxPitchVariation, 1f + sfxPitchVariation);
            sfxSource.pitch = randomPitch;
            sfxSource.PlayOneShot(clip, sfxSource.volume * masterSource.volume);
        }
        else
        {
            Debug.LogWarning("Sound Not Found: " + name);
        }
    }

    public void ToggleMusic() => musicSource.mute = !musicSource.mute;
    public void ToggleSFX() => sfxSource.mute = !sfxSource.mute;
    public void ToggleAmbient() => ambientSource.mute = !ambientSource.mute;

    public void MasterVolume(float volume)
    {
        masterSource.volume = volume;
        SaveVolumes();
    }

    public void MusicVolume(float volume)
    {
        musicSource.volume = volume * masterSource.volume;
        SaveVolumes();
    }

    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume * masterSource.volume;
        SaveVolumes();

        if (Time.unscaledTime - lastSFXFeedbackTime > feedbackCooldown)
        {
            PlaySFX("SFXFeedback");
            lastSFXFeedbackTime = Time.unscaledTime;
        }
    }

    public void SetMasterMute(bool muteState) 
    {
    	masterSource.mute = muteState;
    	musicSource.mute = muteState;
    	sfxSource.mute = muteState;
    }

    public void SetMusicMute(bool muteState) => musicSource.mute = muteState;

    public void SetAmbientMute(bool muteState) => ambientSource.mute = muteState;

    public void SetSFXMute(bool muteState) => sfxSource.mute = muteState;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    public AudioSource PlayLoopingSFX(string name, float initialVolume = 1f)
    {
        if (loopingSources.TryGetValue(name, out AudioSource existingSource))
            return existingSource;

        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            GameObject obj = new GameObject("LoopingSFX_" + name);
            obj.transform.parent = transform;

            AudioSource newSource = obj.AddComponent<AudioSource>();
            newSource.clip = clip;
            newSource.loop = true;
            newSource.playOnAwake = false;
            newSource.volume = initialVolume * sfxSource.volume * masterSource.volume;
            newSource.Play();

            loopingSources.Add(name, newSource);
            return newSource;
        }
        else
        {
            Debug.LogWarning("Looping Sound Not Found: " + name);
            return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameScene sceneEnum = GetSceneEnum(scene.name);
        PlaySceneMusic(sceneEnum);

        PlaySceneAmbient(sceneEnum);
    }

    private void PlaySceneMusic(GameScene sceneEnum)
    {
        foreach (var sceneMusic in sceneMusics)
        {
            if (sceneMusic.scene == sceneEnum)
            {
                PlayMusic(sceneMusic.musicName);
                return;
            }
        }
    }

    private void PlaySceneAmbient(GameScene sceneEnum)
    {
        foreach (var sceneAmbient in sceneAmbients)
        {
            if (sceneAmbient.scene == sceneEnum)
            {
                PlayAmbient(sceneAmbient.musicName); 
                return;
            }
        }

    }

    private GameScene GetSceneEnum(string sceneName)
    {
        if (Enum.TryParse(sceneName, out GameScene parsedEnum))
            return parsedEnum;

        return (GameScene)(-1);
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Pause();
    }

    public void PlaySFXInSource(string name, AudioSource source)
    {
        if (source == null || string.IsNullOrEmpty(name))
            return;

        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            source.clip = clip;
            source.loop = false;

            float finalVolume = sfxSource.volume * masterSource.volume;
            source.volume = finalVolume;

            float randomPitch = UnityEngine.Random.Range(1f - sfxPitchVariation, 1f + sfxPitchVariation);
            source.pitch = randomPitch;

            source.Play();
        }
        else
        {
            Debug.LogWarning("Sound Not Found for dedicated source: " + name);
        }
    }
}