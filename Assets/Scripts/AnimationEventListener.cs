// Local: Assets/Scripts/VFX/AnimationEventListener.cs

using System;
using UnityEngine;

// Define uma estrutura para configurar cada efeito no Inspector.
[Serializable]
public struct AnimationEventEffect
{
    [Tooltip("O nome exato do evento na animação que vai acionar este efeito.")]
    public string eventName;
    
    [Tooltip("Efeito de partícula a ser instanciado.")]
    public GameObject particleEffectPrefab;

    [Tooltip("Sistema de partículas filho a ser ativado.")]
    public ParticleSystem childParticleSystem;

    [Tooltip("Nome do som a ser tocado.")]
    public string sfxName;
    
    [Tooltip("Componente a ser desativado (ex: um Box Collider).")]
    public GameObject componentToDisable;
}

public class AnimationEventListener : MonoBehaviour
{
    [Header("Configuração de Efeitos")]
    [Tooltip("Lista de todos os efeitos que este componente pode acionar via Animation Events.")]
    [SerializeField] private AnimationEventEffect[] effects;

    [Header("Efeitos Iniciais/Finais")]
    [Tooltip("Som a ser tocado no início da animação (requer um Animator com State Machine Behaviours).")]
    [SerializeField] private string sfxOnStateEnter;
    [Tooltip("Som a ser tocado no final da animação.")]
    [SerializeField] private string sfxOnStateExit;


    /// <summary>
    /// Este método público genérico será chamado pelos Animation Events.
    /// O nome do evento na animação deve corresponder a um 'eventName' na lista de efeitos.
    /// </summary>
    /// <param name="eventName">O nome do evento passado pelo Animation Event.</param>
    public void TriggerEvent(string eventName)
    {
        foreach (var effect in effects)
        {
            if (effect.eventName == eventName)
            {
                // Instancia prefab de partícula
                if (effect.particleEffectPrefab != null)
                {
                    Instantiate(effect.particleEffectPrefab, transform.position, transform.rotation);
                }

                // Ativa sistema de partículas filho
                if (effect.childParticleSystem != null)
                {
                    effect.childParticleSystem.Play();
                }

                // Toca som
                if (!string.IsNullOrEmpty(effect.sfxName) && AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySFX(effect.sfxName);
                }
                
                // Desativa um componente
                if (effect.componentToDisable != null)
                {
                    effect.componentToDisable.SetActive(false);
                }
                
                return; // Encontrou o evento, pode parar a busca
            }
        }
        
        Debug.LogWarning($"Animation Event com o nome '{eventName}' foi chamado, mas não encontrado na lista de efeitos de '{gameObject.name}'.", this);
    }

    // Métodos para serem chamados por State Machine Behaviours
    public void PlayEnterSfx()
    {
        if (!string.IsNullOrEmpty(sfxOnStateEnter) && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(sfxOnStateEnter);
        }
    }
    
    public void PlayExitSfx()
    {
        if (!string.IsNullOrEmpty(sfxOnStateExit) && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(sfxOnStateExit);
        }
    }
}