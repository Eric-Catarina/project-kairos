using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[Serializable]
public enum GameScene
{
    ART_BonesClimbGrayBox,
    BackGroundTest,
    BasicMovement,
    BonesClimbGrayBox,
    BonesStartGrayBox,
    CarLevel,
    ART_BonesClimbGrayBoxCopia,
    BonesClimbGrayBoxCopia,
    BonesStartGrayBoxCopia,
    MenuCaioAUDIO,
    EricAnimations,
    EricGraybox,
    FineTuningMovement,
    LevelDesignCaio,
    MainMenu,
    Menu,
    MenuCopiaPeu,
    PrefabsTest,
    TimeStop,
    Tutorial,
    VictorAgarrar,
    VictorGraybox
}

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
    //[Header("Configurações de Música por Cena")]
    //[SerializeField] private SceneMusic[] sceneMusics; 

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
    }

    private void Start()
    {
        GameScene currentSceneEnum = GetSceneEnum(SceneManager.GetActiveScene().name);
        PlaySceneMusic(currentSceneEnum);
        //PlaySceneMusic(SceneManager.GetActiveScene().name);
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

    public void MusicVolume(float volume) => musicSource.volume = volume * masterSource.volume;
    public void AmbientVolume(float volume) => ambientSource.volume = volume * masterSource.volume;
    public void SFXVolume(float volume) => sfxSource.volume = volume * masterSource.volume;

    public void MasterVolume(float volume) => audioMixer.SetFloat("MasterVolume", volume);

    public void ToggleMaster()
    {
        masterSource.mute = !masterSource.mute;
        musicSource.mute = masterSource.mute;
        sfxSource.mute = masterSource.mute;
    }

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

    /*private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaySceneMusic(scene.name);
    }

    private void PlaySceneMusic(string sceneName)
    {
        foreach (var sceneMusic in sceneMusics)
        {
            if (sceneMusic.sceneName == sceneName)
            {
                PlayMusic(sceneMusic.musicName);
                return;
            }
        }

        Debug.Log($"Nenhuma música atribuída para a cena: {sceneName}");
    }*/

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameScene sceneEnum = GetSceneEnum(scene.name);
        PlaySceneMusic(sceneEnum);
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

        Debug.Log($"Nenhuma música atribuída para a cena: {sceneEnum}");
    }

    /// <summary>
    /// Converte o nome da cena atual em um valor do enum GameScene.
    /// </summary>
    private GameScene GetSceneEnum(string sceneName)
    {
        if (Enum.TryParse(sceneName, out GameScene parsedEnum))
            return parsedEnum;

        Debug.LogWarning($"Cena '{sceneName}' não encontrada no enum GameScene. Retornando MainMenu por padrão.");
        return GameScene.MainMenu;
    }
}
