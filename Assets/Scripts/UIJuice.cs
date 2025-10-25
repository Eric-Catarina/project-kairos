// Local: Assets/Scripts/UI/UIJuice.cs

using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UIJuice : MonoBehaviour
{
    [Header("Configurações de Animação")]
    [SerializeField] protected float duration = 0.5f;
    [SerializeField] protected float delay = 0f;
    [SerializeField] protected Ease easeType = Ease.OutBack;
    [SerializeField] private Vector3 startScale = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Comportamento")]
    [SerializeField] protected bool playOnEnable = false;

    [Header("Eventos de Animação")]
    public UnityEvent OnPlayStart;
    public UnityEvent OnPlayComplete;
    public UnityEvent OnReverseStart;
    public UnityEvent OnReverseComplete;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Sequence sequence;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    protected virtual void OnEnable()
    {
        if (playOnEnable)
        {
            PlayAnimation();
        }
    }

    public virtual void PlayAnimation()
    {
        // A ativação do GameObject agora é responsabilidade do UIPanel.
        // Isso garante que o objeto esteja ativo antes desta função ser chamada.
        if (!gameObject.activeInHierarchy) return;

        KillExistingSequence();
        CreateForwardSequence();

        sequence?.OnStart(() => OnPlayStart?.Invoke());
        sequence?.OnComplete(() => OnPlayComplete?.Invoke());
        sequence?.Play();
    }

    public virtual void PlayReverseAnimation()
    {
        if (!gameObject.activeInHierarchy) return;

        KillExistingSequence();
        CreateReverseSequence();

        sequence?.OnStart(() => OnReverseStart?.Invoke());
        sequence?.OnComplete(() => OnReverseComplete?.Invoke());
        sequence?.Play();
    }

    private void CreateForwardSequence()
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 0f;
        rectTransform.localScale = startScale;

        sequence = DOTween.Sequence().SetUpdate(true);

        if (delay > 0)
        {
            sequence.AppendInterval(delay);
        }

        sequence.Append(canvasGroup.DOFade(1f, duration).SetEase(easeType));
        sequence.Join(rectTransform.DOScale(Vector3.one, duration).SetEase(easeType));
        sequence.Pause();
    }
    
    private void CreateReverseSequence()
    {
        canvasGroup.blocksRaycasts = false;

        sequence = DOTween.Sequence().SetUpdate(true);
        
        sequence.Append(canvasGroup.DOFade(0f, duration).SetEase(Ease.InBack));
        sequence.Join(rectTransform.DOScale(startScale, duration).SetEase(Ease.InBack));
        sequence.OnComplete(() => gameObject.SetActive(false));
        sequence.Pause();
    }

    private void KillExistingSequence()
    {
        sequence?.Kill();
    }

    protected virtual void OnDestroy()
    {
        KillExistingSequence();
    }
}