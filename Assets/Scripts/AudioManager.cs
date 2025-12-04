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
    private bool isSFXMuted = false;

    private bool musicWasMutedBeforeMaster = false;
    private bool ambientWasMutedBeforeMaster = false;
    private bool sfxWasMutedBeforeMaster = false;
    private bool isMasterMuted = false;

    private float lastSFXFeedbackTime;
    private const float feedbackCooldown = 0.25f;

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
        PlayerPrefs.SetInt("MasterMuteState", isMasterMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadVolumes()
    {
        masterVolumeBase = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolumeBase = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolumeBase = PlayerPrefs.GetFloat("SFXVolume", 1f);
        ambientVolumeBase = PlayerPrefs.GetFloat("AmbientVolume", 1f);

        MasterVolume(masterVolumeBase);

        bool masterMutedState = PlayerPrefs.GetInt("MasterMuteState", 0) == 1;
        SetMasterMute(masterMutedState);
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
            musicSource.volume = musicVolumeBase * masterSource.volume;
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
            ambientSource.volume = ambientVolumeBase * masterSource.volume;
            ambientSource.loop = true;
            ambientSource.Play();
            StartCoroutine(ApplyAmbientFixNextFrame());
        }
    }

    public void PlaySFX(string name)
    {
        if (isSFXMuted) return;
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

    public void ToggleMusic() => SetMusicMute(!musicSource.mute);
    public void ToggleSFX() => SetSFXMute(!isSFXMuted);
    public void ToggleAmbient() => SetAmbientMute(!ambientSource.mute);

    public void MasterVolume(float volume)
    {
        masterVolumeBase = volume;
        masterSource.volume = masterVolumeBase;
        float min = 0.0001f;
        float v = Mathf.Clamp(masterVolumeBase, 0, 1f);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolumeBase) * 20f);

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
        isMasterMuted = muteState;
        float newMixerVolume;

        if (muteState)
        {
            masterSource.volume = 0f;
            newMixerVolume = -80f;
        }
        else
        {
            masterSource.volume = masterVolumeBase;
            newMixerVolume = Mathf.Log10(Mathf.Max(0.0001f, masterVolumeBase)) * 20f;

        }

        audioMixer.SetFloat("MasterVolume", newMixerVolume);

        RecalculateMusicVolume();
        RecalculateSFXVolume();
        RecalculateAmbientVolume();

        SaveVolumes();
    }

    public void SetMusicMute(bool muteState)
    {
        musicSource.mute = muteState;
    }

    public void SetAmbientMute(bool muteState)
    {
        ambientSource.mute = muteState;
    }

    public void SetSFXMute(bool muteState)
    {
        isSFXMuted = muteState;
        sfxSource.mute = muteState;

        foreach (var source in loopingSources.Values)
        {
            if (source != null)
            {
                source.mute = muteState;
            }
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    public AudioSource PlayLoopingSFX(string name, float initialVolume = 1f)
    {
        if (loopingSources.TryGetValue(name, out AudioSource existingSource))
        {
            if (existingSource != null)
            {
                return existingSource;
            }
            else
            {
                loopingSources.Remove(name);
            }
        }
            

        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            GameObject obj = new GameObject("LoopingSFX_" + name);
            obj.transform.parent = transform;

            AudioSource newSource = obj.AddComponent<AudioSource>();

            if (sfxSource.outputAudioMixerGroup != null)
            {
                newSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
            }

            newSource.clip = clip;
            newSource.loop = true;
            newSource.playOnAwake = false;
            newSource.volume = initialVolume * sfxSource.volume;
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
        bool foundAmbient = false;

        foreach (var sceneAmbient in sceneAmbients)
        {
            if (sceneAmbient.scene == sceneEnum)
            {
                PlayAmbient(sceneAmbient.musicName);
                foundAmbient = true;
                return;
            }
        }

        if (!foundAmbient)
        {
            ambientSource.Stop();
            ambientSource.clip = null;
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
        if (isSFXMuted) return;

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


    private IEnumerator CleanupTemporarySource(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source != null)
        {
            Destroy(source);
        }
    }

    private IEnumerator ApplyAmbientFixNextFrame()
    {
        yield return null; // espera 1 frame
        RecalculateAmbientVolume();
    }

    /*public void PlayUnscaledSFX(string name)
    {
        if (isSFXMuted) return;

        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();

            tempSource.ignoreListenerPause = true;

            if (sfxSource.outputAudioMixerGroup != null)
                tempSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;

            tempSource.clip = clip;

            tempSource.volume = sfxVolumeBase * masterVolumeBase;

            tempSource.pitch = 1f;
            tempSource.Play();

            StartCoroutine(CleanupTemporarySource(tempSource, clip.length));
        }
        else
        {
            Debug.LogWarning("Unscaled Sound Not Found: " + name);
        }
    }*/

    public void PlayUnscaledSFX(string name)
    {
        if (isSFXMuted) return;

        // 1. Calcular o volume base antes
        float finalVolume = sfxVolumeBase * masterVolumeBase;

        // 2. Checagem de segurança: Se o volume for quase zero, não toca nada.
        // Isso resolve o problema de ouvir um "rzrzr" baixinho quando arrasta pro zero.
        if (finalVolume <= 0.001f) return;

        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();

            tempSource.ignoreListenerPause = true;

            if (sfxSource.outputAudioMixerGroup != null)
                tempSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;

            tempSource.clip = clip;

            // 3. Aplicando a redução de ~2dB (multiplicando por 0.8f)
            // Se quiser reduzir apenas 1dB, use 0.9f. Se quiser -3dB, use 0.7f.
            float unscaledReductionMultiplier = 0.6f;
            tempSource.volume = finalVolume * unscaledReductionMultiplier;

            tempSource.pitch = 1f;
            tempSource.Play();

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

        foreach (var source in loopingSources.Values)
        {
            if (source != null)
            {
                source.volume = sfxVolumeBase * masterSource.volume;
            }
        }
    }

    private void RecalculateAmbientVolume()
    {
        float min = 0.0001f;
        float v = Mathf.Clamp(ambientVolumeBase, min, 1f);
        audioMixer.SetFloat("AmbientVolume", Mathf.Log10(v) * 20f);
    }

    public float GetMasterVolumeBase() => masterVolumeBase;
    public float GetMusicVolumeBase() => musicVolumeBase;
    public float GetSFXVolumeBase() => sfxVolumeBase;
    public float GetAmbientVolumeBase() => ambientVolumeBase;


    public void UnregisterLoopingSource(string name)
    {
        if (loopingSources.ContainsKey(name))
        {
            loopingSources.Remove(name);
        }
    }
}