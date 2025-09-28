// Local: Assets/Scripts/PowerUps/BasePowerUpRing.cs

using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class BasePowerUpRing : MonoBehaviour
{
    [Header("Configurações Base do Anel")]
    private string playerTag = "Player";
    [SerializeField] private bool disableOnUse = true;

    [Header("Efeitos")]
    private GameObject effectOnUse;
    private string sfxOnUse;

    public static Action OnPowerRingActivated;

    private void Awake()
    {
        // Garante que o collider seja um trigger
        var colliders = GetComponents<Collider>();
        foreach (var col in colliders)
        {
            if (!col.isTrigger)
            {
                col.isTrigger = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            OnPowerRingActivated?.Invoke();
            ApplyEffect(other.gameObject);
            TriggerEffects();
        }
    }

    /// <summary>
    /// Lógica específica do power-up a ser implementada pelas classes filhas.
    /// </summary>
    /// <param name="playerObject">O GameObject do jogador que ativou o anel.</param>
    protected abstract void ApplyEffect(GameObject playerObject);

    /// <summary>
    /// Aciona os efeitos visuais, sonoros e desativa o objeto, se configurado.
    /// </summary>
    private void TriggerEffects()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("PowerRing");
        }

        if (effectOnUse != null)
        {
            Instantiate(effectOnUse, transform.position, transform.rotation);
        }

        if (disableOnUse)
        {
            gameObject.SetActive(false);
        }
    }
}