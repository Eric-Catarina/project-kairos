// Local: Assets/Scripts/VFX/AnimationTrigger.cs

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AnimationTrigger : MonoBehaviour
{
    public enum ActivationMode
    {
        [Tooltip("Ativa a animação quando o jogador entra no trigger.")]
        OnPlayerEnter,
        [Tooltip("Ativa a animação quando o jogador sai do trigger.")]
        OnPlayerExit,
        [Tooltip("Ativa a animação quando o jogador entra e reverte quando sai.")]
        OnEnterAndExit
    }

    [Header("Configuração de Ativação")]
    [Tooltip("O alvo que possui o Animator a ser controlado. Se nulo, usará este mesmo objeto.")]
    [SerializeField] private Animator targetAnimator;
    [Tooltip("Define como o trigger vai interagir com o jogador.")]
    [SerializeField] private ActivationMode activationMode = ActivationMode.OnPlayerEnter;
    [Tooltip("A tag do objeto que pode ativar este trigger (geralmente 'Player').")]
    [SerializeField] private string activatingTag = "Player";

    [Header("Parâmetros do Animator")]
    [Tooltip("O nome do parâmetro 'Trigger' a ser ativado no Animator.")]
    [SerializeField] private string triggerParameterName;
    [Tooltip("O nome do parâmetro 'Bool' a ser alterado no Animator (usado com OnEnterAndExit).")]
    [SerializeField] private string boolParameterName;
    
    [Header("Comportamento")]
    [Tooltip("Se marcado, o trigger só pode ser ativado uma única vez.")]
    [SerializeField] private bool activateOnlyOnce = true;

    private bool _hasBeenActivated = false;

    private void Awake()
    {
        // Se nenhum Animator for especificado, procura no próprio objeto.
        if (targetAnimator == null)
        {
            targetAnimator = GetComponent<Animator>();
        }

        if (targetAnimator == null)
        {
            Debug.LogError("Nenhum Animator encontrado ou atribuído no AnimationTrigger.", this);
            enabled = false;
            return;
        }

        // Garante que o collider é um trigger.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(activatingTag)) return;
        if (activateOnlyOnce && _hasBeenActivated) return;
        
        if (activationMode == ActivationMode.OnPlayerEnter || activationMode == ActivationMode.OnEnterAndExit)
        {
            ActivateAnimation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(activatingTag)) return;
        if (activateOnlyOnce && _hasBeenActivated) return;

        if (activationMode == ActivationMode.OnPlayerExit)
        {
            ActivateAnimation();
        }
        else if (activationMode == ActivationMode.OnEnterAndExit)
        {
            DeactivateAnimation();
        }
    }

    private void ActivateAnimation()
    {
        Debug.Log("AnimationTrigger: Ativando animação.", this);
        // Ativa um Trigger se o nome for fornecido
        if (!string.IsNullOrEmpty(triggerParameterName))
        {
            targetAnimator.SetTrigger(triggerParameterName);
        }

        // Define um Bool como 'true' se o nome for fornecido
        if (!string.IsNullOrEmpty(boolParameterName))
        {
            targetAnimator.SetBool(boolParameterName, true);
        }

        _hasBeenActivated = true;
    }

    private void DeactivateAnimation()
    {
        // Usado principalmente para reverter um parâmetro Bool
        if (!string.IsNullOrEmpty(boolParameterName))
        {
            targetAnimator.SetBool(boolParameterName, false);
        }
    }
}