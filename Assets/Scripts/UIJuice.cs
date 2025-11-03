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

    // Novo campo para a escala inicial da animação TV Scan (ponto pequeno)
    [SerializeField] private Vector3 tvScanStartScale = new Vector3(0.01f, 0.01f, 1f);

    [Header("Comportamento")]
    [SerializeField] protected bool playOnEnable = false; // Irrelevante agora, pois sempre roda a TV Scan

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
        // Sempre roda a animação TV Scan quando o painel é ativado (OnEnable).
        PlayTVScanAnimation();
    }

    public virtual void PlayAnimation()
    {
        // A ativação do GameObject agora é responsabilidade do UIPanel.
        // Isso garante que o objeto esteja ativo antes desta função ser chamada.
        // if (!gameObject.activeInHierarchy) return;
        gameObject.SetActive(true);
        KillExistingSequence();
        CreateForwardSequence();

        sequence?.OnStart(() => OnPlayStart?.Invoke());
        sequence?.OnComplete(() => OnPlayComplete?.Invoke());
        sequence?.Play();
    }

    // Novo método para a animação TV Scan (abertura estética) - roda automaticamente no OnEnable
    public virtual void PlayTVScanAnimation()
    {
        gameObject.SetActive(true);
        KillExistingSequence();
        CreateTVScanSequence();

        sequence?.OnStart(() => OnPlayStart?.Invoke());
        sequence?.OnComplete(() => OnPlayComplete?.Invoke());
        sequence?.Play();
    }

    // Novo método para o reverse da animação TV Scan (fechamento)
    public virtual void PlayReverseTVScanAnimation()
    {
        if (!gameObject.activeInHierarchy) return;

        KillExistingSequence();
        CreateReverseTVScanSequence();

        sequence?.OnStart(() => OnReverseStart?.Invoke());
        sequence?.OnComplete(() => OnReverseComplete?.Invoke());
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

    // Novo método para criar a sequência TV Scan
    private void CreateTVScanSequence()
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 0f;
        rectTransform.localScale = tvScanStartScale; // Começa como um ponto pequeno

        sequence = DOTween.Sequence().SetUpdate(true);

        if (delay > 0)
        {
            sequence.AppendInterval(delay);
        }

        // Parte 1: Escalar horizontalmente para formar uma linha reta (X de 0.01 para 1, Y permanece pequeno)
        sequence.Append(rectTransform.DOScaleX(1f, duration / 2f).SetEase(easeType));
        sequence.Join(canvasGroup.DOFade(0.5f, duration / 2f).SetEase(easeType)); // Fade parcial

        // Parte 2: Escalar verticalmente para o tamanho normal (Y de 0.01 para 1, X permanece 1)
        sequence.Append(rectTransform.DOScaleY(1f, duration / 2f).SetEase(easeType));
        sequence.Join(canvasGroup.DOFade(1f, duration / 2f).SetEase(easeType)); // Fade completo
        sequence.Pause();
    }

    // Novo método para criar a sequência reverse TV Scan
    private void CreateReverseTVScanSequence()
    {
        canvasGroup.blocksRaycasts = false;

        sequence = DOTween.Sequence().SetUpdate(true);

        // Parte 1: Escalar verticalmente para formar uma linha reta (Y de 1 para 0.01, X permanece 1)
        sequence.Append(rectTransform.DOScaleY(tvScanStartScale.y, duration / 2f).SetEase(Ease.InBack));
        sequence.Join(canvasGroup.DOFade(0.5f, duration / 2f).SetEase(Ease.InBack)); // Fade parcial

        // Parte 2: Escalar horizontalmente para um ponto (X de 1 para 0.01, Y permanece pequeno)
        sequence.Append(rectTransform.DOScaleX(tvScanStartScale.x, duration / 2f).SetEase(Ease.InBack));
        sequence.Join(canvasGroup.DOFade(0f, duration / 2f).SetEase(Ease.InBack)); // Fade completo
        sequence.OnComplete(() => gameObject.SetActive(false)); // Desativa o painel no final
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
