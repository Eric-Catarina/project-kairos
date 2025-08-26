using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] musicSounds, ambientSounds, sfxSounds;
    public AudioSource masterSource, musicSource, sfxSource, ambientSource;
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    [SerializeField] private AudioMixer audioMixer;

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
        PlayMusic("");
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
            Debug.Log("Sound Not Found");
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
            Debug.Log("Ambient sound not found: " + name);
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
            sfxSource.PlayOneShot(clip, sfxSource.volume * masterSource.volume);
        }
        else
        {
            Debug.Log("Sound Not Found: " + name);
        }
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }

    public void ToggleAmbient()
    {
        ambientSource.mute = !ambientSource.mute;
    }

    public void MusicVolume(float volume)
    {
        musicSource.volume = volume * masterSource.volume;
    }

    public void AmbientVolume(float volume)
    {
        ambientSource.volume = volume * masterSource.volume;
    }

    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume * masterSource.volume;
    }

    public void MasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
    }

    public void ToggleMaster()
    {
        masterSource.mute = !masterSource.mute;
        musicSource.mute = masterSource.mute;
        sfxSource.mute = masterSource.mute;
    }
}//