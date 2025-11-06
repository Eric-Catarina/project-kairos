using System;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    public static event Action<Checkpoint> OnCheckpointActivated;

    [Header("Configuração")]
    [Tooltip("A ordem deste checkpoint na fase. O manager irá ordená-los com base neste número.")]
    public int orderIndex = 0;
    [Tooltip("O ponto exato para onde o jogador será teleportado. Se nulo, usará a posição deste objeto.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Efeitos Visuais e Sonoros")]
    [Tooltip("Objeto que representa o checkpoint no estado INATIVO. Será desativado ao tocar.")]
    [SerializeField] private GameObject inactiveVisual;
    [Tooltip("Objeto que representa o checkpoint no estado ATIVO. Será ativado ao tocar.")]
    [SerializeField] private GameObject activatedVisual;
    [SerializeField] private ParticleSystem activationEffect;
    [SerializeField] private string activationSfx = "CheckpointActivate";

    private Collider _collider;
    private bool _hasBeenActivated = false;
    
    public float ActivationTime { get; private set; } = -1f;
    public Transform SpawnPoint => spawnPoint != null ? spawnPoint : transform;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
        
        inactiveVisual?.SetActive(true);
        activatedVisual?.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasBeenActivated || !other.CompareTag("Player")) return;

        if (ScoreManager.Instance != null)
        {
            ActivationTime = ScoreManager.Instance.CurrentTime;
        }

        _hasBeenActivated = true;
        OnCheckpointActivated?.Invoke(this);
        TriggerActivationEffects();
    }

    private void TriggerActivationEffects()
    {
        _collider.enabled = false;

        if (activationEffect != null)
        {
            activationEffect.Play();
        }

        if (!string.IsNullOrEmpty(activationSfx) && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(activationSfx);
        }

        inactiveVisual?.SetActive(false);
        activatedVisual?.SetActive(true);
    }

    public void ResetState()
    {
        _hasBeenActivated = false;
        _collider.enabled = true;
        ActivationTime = -1f;
        
        inactiveVisual?.SetActive(true);
        activatedVisual?.SetActive(false);
    }
}