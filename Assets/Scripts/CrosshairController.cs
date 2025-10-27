// Assets/Scripts/UI/CrosshairController.cs

using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(UIFollowWorldObject))]
public class CrosshairController : MonoBehaviour
{
    [Header("Configurações da Animação")]
    [SerializeField] private float targetScaleMultiplier = 1.2f;
    [SerializeField] private float scaleDuration = 0.15f;
    [SerializeField] private float punchStrength = 0.15f;
    [SerializeField] private float punchDuration = 0.2f;

    private GrapplingHookController _grapplingHookController;
    private UIFollowWorldObject _uiFollow;
    private RectTransform _crosshairRect;
    private Vector3 _originalScale;

    private void Awake()
    {
        // Pega as referências automaticamente
        _crosshairRect = GetComponent<RectTransform>();
        _uiFollow = GetComponent<UIFollowWorldObject>();
        _originalScale = _crosshairRect.localScale;
        
        // Encontra o hook controller na cena
        _grapplingHookController = FindObjectOfType<GrapplingHookController>();
    }

    private void OnEnable()
    {
        if (_grapplingHookController != null)
        {
            // Se inscreve no novo evento
            _grapplingHookController.OnPredictionTargetChanged += HandlePredictionTargetChanged;
        }
        
        // Garante que a retícula comece invisível
        HandlePredictionTargetChanged(null); 
    }

    private void OnDisable()
    {
        if (_grapplingHookController != null)
        {
            _grapplingHookController.OnPredictionTargetChanged -= HandlePredictionTargetChanged;
        }
        _crosshairRect.DOKill();
    }

    private void HandlePredictionTargetChanged(Transform newTarget)
    {
        // A lógica inteira está aqui:
        // 1. Passa o novo alvo (que pode ser null) para o script que segue.
        _uiFollow.SetTarget(newTarget);
        
        // 2. Anima a escala com base na existência de um alvo.
        AnimateCrosshair(newTarget != null);
    }

    private void AnimateCrosshair(bool isTargetAcquired)
    {
        _crosshairRect.DOKill(true);
        Vector3 targetScale = isTargetAcquired ? _originalScale * targetScaleMultiplier : _originalScale;

        var tween = _crosshairRect.DOScale(targetScale, scaleDuration);
        
        if (isTargetAcquired)
        {
            tween.SetEase(Ease.OutBack);
            tween.OnComplete(() =>
            {
                if(this != null) // Garante que o objeto não foi destruído
                    _crosshairRect.DOPunchScale(Vector3.one * punchStrength, punchDuration, 10, 1);
            });
        }
        else
        {
            tween.SetEase(Ease.OutQuad);
        }
    }
}