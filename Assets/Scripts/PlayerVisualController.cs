// Local: Assets/Scripts/PlayerVisualController.cs

using UnityEngine;
using DG.Tweening;

/// <summary>
/// Gerencia todas as animações e efeitos visuais procedurais do jogador.
/// Ouve eventos do PlayerMovementController para acionar as animações corretas.
/// (Lógica de slide removida para corresponder às novas mecânicas)
/// </summary>
public class PlayerVisualController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Referência ao script de movimento para se inscrever nos eventos.")]
    [SerializeField] private PlayerMovementController playerMovementController;
    [Tooltip("O transform do modelo visual do jogador que será animado.")]
    [SerializeField] private Transform playerModel;

    private void OnEnable()
    {
        if (playerMovementController == null)
        {
            Debug.LogError("PlayerMovementController não está atribuído no PlayerVisualsController.");
            return;
        }

    }

    private void OnDisable()
    {
        if (playerMovementController == null) return;
        
    }

    /// <summary>
    /// Para e limpa todas as animações DOTween ativas neste objeto.
    /// </summary>
    private void KillAllTweens()
    {
        playerModel.DOKill();
    }

    private void OnDestroy()
    {
        KillAllTweens();
    }
}