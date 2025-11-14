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

    private float masterVolumeBase = 1f;
    private float musicVolumeBase = 1f;
    private float sfxVolumeBase = 1f;
    private float ambientVolumeBase = 1f;


    private float lastSFXFeedbackTime;
    private const float feedbackCooldown = 0.15f;

    private bool sfxWasMutedBeforePause = false;

    private void Awake()
    {
        AddSoundsToDictionary(sfxSounds);
        AddSoundsToDictionary(musicSounds);
        AddSoundsToDictionary(ambientSounds);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource.mute = false;
            ambientSource.mute = false;
            sfxSource.mute = false;
        }
        else
        {
            Destroy(gameObject);
        }

        LoadVolumes();
    }



    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeBase);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeBase);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeBase);
        PlayerPrefs.SetFloat("AmbientVolume", ambientVolumeBase);
        PlayerPrefs.Save();
    }

    public void LoadVolumes()
    {
        masterVolumeBase = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolumeBase = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolumeBase = PlayerPrefs.GetFloat("SFXVolume", 1f);
        ambientVolumeBase = PlayerPrefs.GetFloat("AmbientVolume", 1f);

        MasterVolume(masterVolumeBase);
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
        masterVolumeBase = volume;
        masterSource.volume = masterVolumeBase;

        RecalculateMusicVolume();
        RecalculateSFXVolume();
        RecalculateAmbientVolume();

        SaveVolumes();
    }

    public void MusicVolume(float volume)
    {
        musicVolumeBase = volume;
        RecalculateMusicVolume();
        SaveVolumes();
    }

    public void SFXVolume(float volume)
    {
        sfxVolumeBase = volume;
        RecalculateSFXVolume();
        SaveVolumes();

        if (Time.unscaledTime - lastSFXFeedbackTime > feedbackCooldown)
        {
            PlayUnscaledSFX("SFXFeedback");
            lastSFXFeedbackTime = Time.unscaledTime;
        }
    }

    public void AmbientVolume(float volume)
    {
        ambientVolumeBase = volume;
        RecalculateAmbientVolume();
        SaveVolumes();
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

    public void PauseAmbient()
    {
        if (ambientSource.isPlaying)
            ambientSource.Pause();
    }

    public void UnpauseAmbient()
    {
        if (ambientSource != null && !ambientSource.isPlaying && ambientSource.clip != null)
            ambientSource.UnPause();
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

    public void PauseAllSFX()
    {

        foreach (var source in loopingSources.Values)
        {
            if (source != null && source.isPlaying)
            {
                source.Pause();
            }
        }


        sfxWasMutedBeforePause = sfxSource.mute;


        sfxSource.mute = true;
    }

    public void UnpauseAllSFX()
    {

        foreach (var source in loopingSources.Values)
        {

            if (source != null && !source.isPlaying)
            {
                source.UnPause();
            }
        }

        sfxSource.mute = false;
    }

    /*public void PlaySFXForTransition(string name)
    {
        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();

            // Configura o mixer (se você usa)
            if (sfxSource.outputAudioMixerGroup != null)
                tempSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;

            tempSource.clip = clip;
            tempSource.volume = sfxSource.volume * masterSource.volume;
            tempSource.pitch = UnityEngine.Random.Range(1f - sfxPitchVariation, 1f + sfxPitchVariation);

            tempSource.Play();
            StartCoroutine(CleanupTemporarySource(tempSource, clip.length));
        }
        else
        {
            Debug.LogWarning("Sound Not Found for transition: " + name);
        }
    } */

    private IEnumerator CleanupTemporarySource(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source != null)
        {
            Destroy(source);
        }
    }

    public void PlayUnscaledSFX(string name)
    {
        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();

            tempSource.ignoreListenerPause = true;

            if (sfxSource.outputAudioMixerGroup != null)
                tempSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;

            tempSource.clip = clip;

            tempSource.volume = sfxSource.volume;

            tempSource.pitch = UnityEngine.Random.Range(1f - sfxPitchVariation, 1f + sfxPitchVariation);
            tempSource.Play();

            // 2. Limpa a fonte temporária após o clip terminar
            StartCoroutine(CleanupTemporarySource(tempSource, clip.length));
        }
        else
        {
            Debug.LogWarning("Unscaled Sound Not Found: " + name);
        }
    }


    private void RecalculateMusicVolume()
    {
        musicSource.volume = musicVolumeBase * masterSource.volume;
    }

    private void RecalculateSFXVolume()
    {
        sfxSource.volume = sfxVolumeBase * masterSource.volume;
    }

    private void RecalculateAmbientVolume()
    {
        ambientSource.volume = ambientVolumeBase * masterSource.volume;
    }

    public float GetMasterVolumeBase() => masterVolumeBase;
    public float GetMusicVolumeBase() => musicVolumeBase;
    public float GetSFXVolumeBase() => sfxVolumeBase;
    public float GetAmbientVolumeBase() => ambientVolumeBase;
}