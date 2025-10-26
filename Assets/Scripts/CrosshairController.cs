// Assets/Scripts/UI/CrosshairController.cs

using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class CrosshairController : MonoBehaviour
{
    [Header("Configurações da Animação")]
    [Tooltip("O multiplicador de escala quando um alvo de gancho é válido.")]
    [SerializeField] private float targetScaleMultiplier = 1.2f;
    [Tooltip("A duração da animação de escala.")]
    [SerializeField] private float scaleDuration = 0.15f;
    [Tooltip("A força do efeito 'punch' na escala.")]
    [SerializeField] private float punchStrength = 0.15f;
    [Tooltip("A duração do efeito 'punch'.")]
    [SerializeField] private float punchDuration = 0.2f;
    [Tooltip("Vibração do punch.")]
    [SerializeField] private int punchVibrato = 10;
    [Tooltip("Elasticidade do punch.")]
    [SerializeField] private float punchElasticity = 1f;

    private GrapplingHookController _grapplingHookController;
    private RectTransform _crosshairRect;
    private Vector3 _originalScale;

    private void Awake()
    {
        _crosshairRect = GetComponent<RectTransform>();
        _originalScale = _crosshairRect.localScale;
        _grapplingHookController = FindObjectOfType<GrapplingHookController>();
    }

    private void OnEnable()
    {
        if (_grapplingHookController == null)
        {
            // Tenta encontrar novamente caso tenha sido instanciado depois
            _grapplingHookController = FindObjectOfType<GrapplingHookController>();
        }

        if (_grapplingHookController != null)
        {
            _grapplingHookController.OnValidGrappleTargetAcquired += HandleTargetAcquired;
            _grapplingHookController.OnValidGrappleTargetLost += HandleTargetLost;
        }
        
        // Reseta para o estado inicial ao ativar
        _crosshairRect.localScale = _originalScale;
    }

    private void OnDisable()
    {
        if (_grapplingHookController != null)
        {
            _grapplingHookController.OnValidGrappleTargetAcquired -= HandleTargetAcquired;
            _grapplingHookController.OnValidGrappleTargetLost -= HandleTargetLost;
        }
        _crosshairRect.DOKill();
    }

    private void HandleTargetAcquired()
    {
        AnimateCrosshair(_originalScale * targetScaleMultiplier, true);
    }

    private void HandleTargetLost()
    {
        AnimateCrosshair(_originalScale, false);
    }

    private void AnimateCrosshair(Vector3 targetScale, bool playPunch)
    {
        // 'true' completa a animação atual imediatamente, garantindo que a escala 
        // esteja em um estado conhecido antes de começar a próxima.
        _crosshairRect.DOKill(true); 

        if (playPunch)
        {
            // Sequência para Punch: primeiro escala para o alvo, e simultaneamente aplica o punch.
            // Como Punch é aditivo, ele funcionará bem sobre a escala base que está mudando.
            _crosshairRect.DOScale(targetScale, scaleDuration).SetEase(Ease.OutBack);
            _crosshairRect.DOPunchScale(Vector3.one * punchStrength, punchDuration, punchVibrato, punchElasticity);
        }
        else
        {
            // Apenas retorna suavemente ao tamanho original
            _crosshairRect.DOScale(targetScale, scaleDuration).SetEase(Ease.OutQuad);
        }
    }
}