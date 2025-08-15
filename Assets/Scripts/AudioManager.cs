using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Músicas")]
    [Tooltip("Lista de músicas de fundo disponíveis.")]
    public List<AudioClip> backgroundMusicClips = new List<AudioClip>(); // Lista de músicas
    public AudioSource bgmAudioSource;

    [Header("Efeitos Sonoros")]
    public AudioSource sfxAudioSource;
    [Tooltip("Lista dos clipes de áudio dos efeitos sonoros.")]
    public List<AudioClip> soundEffectClips;

    private const float PITCH_VARIATION_PERCENTAGE = 0.10f; // 10%

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Exemplo: Inicia a música do menu ao carregar a cena inicial
        PlayBackgroundMusic(0); // Assume que a música do menu é o primeiro clipe na lista
    }

    /// <summary>
    /// Toca uma música de fundo específica da lista.
    /// </summary>
    /// <param name="musicIndex">O índice da música na lista 'backgroundMusicClips'.</param>
    public void PlayBackgroundMusic(int musicIndex)
    {
        if (bgmAudioSource == null || backgroundMusicClips.Count == 0)
        {
            Debug.LogWarning("AudioManager: bgmAudioSource não atribuído ou lista de músicas vazia.");
            return;
        }

        if (musicIndex < 0 || musicIndex >= backgroundMusicClips.Count || backgroundMusicClips[musicIndex] == null)
        {
            Debug.LogWarning($"AudioManager: Índice de música inválido ({musicIndex}).");
            return;
        }

        bgmAudioSource.clip = backgroundMusicClips[musicIndex];
        bgmAudioSource.loop = true;
        ApplyRandomPitch(bgmAudioSource);
        bgmAudioSource.Play();
    }

    /// <summary>
    /// Para a música de fundo.
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    /// <summary>
    /// Toca um efeito sonoro específico da lista.
    /// </summary>
    /// <param name="sfxIndex">O índice do efeito sonoro na lista 'soundEffectClips'.</param>
    /// <param name="volumeScale">Escala de volume opcional para este efeito sonoro (padrão é 1.0f).</param>
    public void PlaySoundEffect(int sfxIndex, float volumeScale = 1.0f)
    {
        if (sfxAudioSource == null)
        {
            Debug.LogWarning("AudioManager: sfxAudioSource não foi atribuído. Efeito sonoro não pode ser tocado.");
            return;
        }

        if (sfxIndex < 0 || sfxIndex >= soundEffectClips.Count || soundEffectClips[sfxIndex] == null)
        {
            Debug.LogWarning($"AudioManager: Índice de efeito sonoro inválido ({sfxIndex}) ou clipe de áudio não atribuído.");
            return;
        }

        ApplyRandomPitch(sfxAudioSource);
        sfxAudioSource.PlayOneShot(soundEffectClips[sfxIndex], volumeScale);
    }

    public void PlayConfettiSound()
    {
        if (sfxAudioSource == null)
        {
            Debug.LogWarning("AudioManager: sfxAudioSource não foi atribuído. Efeito sonoro de confete não pode ser tocado.");
            return;
        }

        AudioClip confettiClip = soundEffectClips.Find(clip => clip.name.Contains("Confetti"));
        if (confettiClip != null)
        {
            ApplyRandomPitch(sfxAudioSource);
            sfxAudioSource.PlayOneShot(confettiClip);
        }
        else
        {
            Debug.LogWarning("AudioManager: Clip de confete não encontrado na lista de efeitos sonoros.");
        }
    }

    public void PlaySoundEffectClip(AudioClip clip, float volumeScale = 1.0f)
  {
    if (sfxAudioSource == null)
    {
      Debug.LogWarning("AudioManager: sfxAudioSource não foi atribuído. Efeito sonoro não pode ser tocado.");
      return;
    }

    if (clip == null)
    {
      Debug.LogWarning("AudioManager: Clip de áudio é nulo. Efeito sonoro não pode ser tocado.");
      return;
    }

    ApplyRandomPitch(sfxAudioSource);
    sfxAudioSource.PlayOneShot(clip, volumeScale);
  }

    private void ApplyRandomPitch(AudioSource audioSource)
  {
    float randomPitch = Random.Range(1f - PITCH_VARIATION_PERCENTAGE, 1f + PITCH_VARIATION_PERCENTAGE);
    audioSource.pitch = randomPitch;
  }
}
