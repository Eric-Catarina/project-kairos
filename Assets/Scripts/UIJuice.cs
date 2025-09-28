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
    [Tooltip("Disparado quando a animação de entrada começa.")]
    public UnityEvent OnPlayStart;

    [Tooltip("Disparado quando a animação de entrada é concluída.")]
    public UnityEvent OnPlayComplete;

    [Tooltip("Disparado quando a animação de saída começa.")]
    public UnityEvent OnReverseStart;

    [Tooltip("Disparado quando a animação de saída é concluída.")]
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

    public void SetActiveAndPlay()
    {
        gameObject.SetActive(true);
        PlayAnimation();
    }

    public virtual void PlayAnimation()
    {
        KillExistingSequence();
        CreateForwardSequence();

        if (sequence != null)
        {
            sequence.OnStart(() => OnPlayStart?.Invoke());
            sequence.OnComplete(() => OnPlayComplete?.Invoke());
            sequence.Play();
        }
    }

    public virtual void PlayReverseAnimation()
    {
        KillExistingSequence();
        CreateReverseSequence();

        if (sequence != null)
        {
            sequence.OnStart(() => OnReverseStart?.Invoke());
            sequence.OnComplete(() => OnReverseComplete?.Invoke());
            sequence.Play();
        }
    }

    private void CreateForwardSequence()
    {
        gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;

        canvasGroup.alpha = 0f;
        rectTransform.localScale = startScale;

        sequence = DOTween.Sequence();

        if (delay > 0)
        {
            sequence.AppendInterval(delay);
        }

        sequence.Append(canvasGroup.DOFade(1f, duration).SetEase(easeType));
        sequence.Join(rectTransform.DOScale(Vector3.one, duration).SetEase(easeType));

        sequence.SetUpdate(true); // <-- CORREÇÃO APLICADA AQUI
        sequence.Pause();
    }

    private void CreateReverseSequence()
    {

        canvasGroup.blocksRaycasts = false;

        sequence = DOTween.Sequence();

        sequence.Append(canvasGroup.DOFade(0f, duration).SetEase(Ease.InBack));
        sequence.Join(rectTransform.DOScale(startScale, duration).SetEase(Ease.InBack));
        sequence.OnComplete(() => gameObject.SetActive(false)); // Desativa o objeto ao final

        sequence.SetUpdate(true); 
        sequence.Pause();
    }


    private void KillExistingSequence()
    {
        if (sequence != null && sequence.IsActive())
        {
            sequence.Kill();
        }
    }

    protected virtual void OnDestroy()
    {
        KillExistingSequence();
    }
}