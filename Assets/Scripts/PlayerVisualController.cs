// Local: Assets/Scripts/PlayerVisualsController.cs

using UnityEngine;
using DG.Tweening;
using System;

/// <summary>
/// Gerencia todas as animações e efeitos visuais procedurais do jogador.
/// Ouve eventos do PlayerMovementController para acionar as animações corretas.
/// </summary>
public class PlayerVisualsController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Referência ao script de movimento para se inscrever nos eventos.")]
    [SerializeField] private PlayerMovementController playerMovementController;
    [Tooltip("O transform do modelo visual do jogador que será animado.")]
    [SerializeField] private Transform playerModel;

    [Header("Configurações de Animação do Deslize")]
    [SerializeField] private Vector3 slideRotation = new Vector3(-40f, 0f, 0f);
    [SerializeField] private float slideAnimDuration = 0.2f;
    [SerializeField] private float slideShakeStrength = 3f;
    [SerializeField] private int slideShakeVibrato = 10;
    
    [Header("Configurações de Animação do Boost")]
    [SerializeField] private Vector3 boostJumpRotation = new Vector3(40f, 0f, 0f);
    [SerializeField] private float boostAnimDuration = 0.5f;

    private Tween _slideShakeTween;

    private void OnEnable()
    {
        if (playerMovementController == null)
        {
            Debug.LogError("PlayerMovementController não está atribuído no PlayerVisualsController.");
            return;
        }

        // Inscreve os métodos nos eventos do controlador de movimento
        playerMovementController.OnSlideStart += HandleSlideStart;
        playerMovementController.OnSlideEnd += HandleSlideEnd;
        playerMovementController.OnBoostJump += HandleBoostJump;
    }

    private void OnDisable()
    {
        if (playerMovementController == null) return;
        
        // Remove a inscrição para evitar memory leaks
        playerMovementController.OnSlideStart -= HandleSlideStart;
        playerMovementController.OnSlideEnd -= HandleSlideEnd;
        playerMovementController.OnBoostJump -= HandleBoostJump;
    }

    /// <summary>
    /// Chamado quando o evento OnSlideStart é disparado.
    /// Inicia a animação de entrada no deslize.
    /// </summary>
    private void HandleSlideStart()
    {
        KillAllTweens(); // Garante que animações anteriores sejam paradas

        // Anima a rotação para a posição de deslize
        playerModel.transform.DOLocalRotate(slideRotation, slideAnimDuration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            // Após completar a rotação, inicia o tremor
            StartSlideShake();
        });

        // Inicia a animação de tremor que dura enquanto o slide estiver ativo
        // O loop infinito é controlado matando o tween quando o slide acaba

    }
    private void StartSlideShake()
    {
_slideShakeTween = playerModel.DOShakeRotation(duration: 1f, strength: slideShakeStrength, vibrato: slideShakeVibrato)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// Chamado quando o evento OnSlideEnd é disparado.
    /// Retorna o personagem à rotação padrão.
    /// </summary>
    private void HandleSlideEnd()
    {
        KillAllTweens();
        playerModel.DOLocalRotate(Vector3.zero, slideAnimDuration).SetEase(Ease.OutQuad);
    }
    
    /// <summary>
    /// Chamado quando o evento OnBoostJump é disparado.
    /// Executa a animação de pulo com boost.
    /// </summary>
    private void HandleBoostJump()
    {
        KillAllTweens();

        // Cria uma sequência para executar múltiplas animações em conjunto
        Sequence boostSequence = DOTween.Sequence();
        
        // Animação 1: Inclina o personagem para frente
        boostSequence.Append(playerModel.DOLocalRotate(boostJumpRotation, boostAnimDuration * 0.5f).SetEase(Ease.OutQuad));
        
        // Animação 2: Gira o personagem 360 graus no eixo Y ao mesmo tempo
        boostSequence.Join(playerModel.DOLocalRotate(new Vector3(0, 360, 0), boostAnimDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
        
        // Animação 3: Retorna à rotação padrão após o boost
        boostSequence.Append(playerModel.DOLocalRotate(Vector3.zero, boostAnimDuration * 0.5f).SetEase(Ease.InQuad));
    }
    
    /// <summary>
    /// Para e limpa todas as animações DOTween ativas neste objeto.
    /// </summary>
    private void KillAllTweens()
    {
        if (_slideShakeTween != null && _slideShakeTween.IsActive())
        {
            _slideShakeTween.Kill();
        }
        playerModel.DOKill();
    }

    private void OnDestroy()
    {
        // Garante que os tweens sejam mortos se o objeto for destruído
        KillAllTweens();
    }
}