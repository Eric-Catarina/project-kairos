using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UIAutoPunchAnimator : MonoBehaviour
{
    [Header("Sprite Settings")]
    [Tooltip("O sprite padrão (estado de repouso/sem clique).")]
    [SerializeField] private Sprite idleSprite;
    
    [Tooltip("O sprite ativo (estado pressionado/clique).")]
    [SerializeField] private Sprite activeSprite;

    [Header("Timing Settings")]
    [Tooltip("Tempo em segundos entre cada animação.")]
    [SerializeField] private float interval = 2.0f;
    
    [Tooltip("Tempo em segundos que o sprite ativo permanece visível.")]
    [SerializeField] private float activeDuration = 0.1f;

    [Header("Animation Settings")]
    [Tooltip("A força e direção do efeito de Punch da escala.")]
    [SerializeField] private Vector3 punchScale = new Vector3(-0.2f, -0.2f, 0f);
    
    [Tooltip("Vibração do efeito de punch.")]
    [SerializeField] private int vibrato = 10;
    
    [Tooltip("Elasticidade do efeito de punch.")]
    [SerializeField] private float elasticity = 1f;

    // Evento para permitir que outros sistemas reajam (ex: tocar som de click)
    public event Action OnAnimationTriggered;

    private Image _targetImage;
    private Coroutine _animationCoroutine;
    private Vector3 _originalScale;

    private void Awake()
    {
        _targetImage = GetComponent<Image>();
        _originalScale = transform.localScale;

        // Validação básica
        if (idleSprite == null || activeSprite == null)
        {
            Debug.LogWarning($"[UIAutoPunchAnimator] Sprites não atribuídos no objeto {gameObject.name}. O script foi desativado.", this);
            enabled = false;
            return;
        }

        // Garante estado inicial
        _targetImage.sprite = idleSprite;
    }

    private void OnEnable()
    {
        // Reinicia a escala para evitar erros se o objeto foi desativado durante um tween
        transform.localScale = _originalScale;
        StartAnimationLoop();
    }

    private void OnDisable()
    {
        StopAnimationLoop();
        // Mata qualquer tween ativo neste transform para evitar erros de referência ou memory leaks
        transform.DOKill();
    }

    private void StartAnimationLoop()
    {
        if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
        _animationCoroutine = StartCoroutine(AnimationRoutine());
    }

    private void StopAnimationLoop()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }

    private IEnumerator AnimationRoutine()
    {
        // Pequeno delay inicial para não disparar imediatamente ao habilitar (opcional, remove sensação robótica)
        yield return new WaitForSeconds(0.2f);

        while (true)
        {
            yield return new WaitForSeconds(interval);

            PerformAction();

            // Espera a duração do estado "Ativo"
            yield return new WaitForSeconds(activeDuration);

            RevertAction();
        }
    }

    private void PerformAction()
    {
        // Troca o Sprite
        if (_targetImage != null)
        {
            _targetImage.sprite = activeSprite;
        }

        // Executa o Punch Scale via DOTween
        // Usamos DOKill antes para garantir que não haja sobreposição de animações
        transform.DOKill(true);
        transform.localScale = _originalScale; // Reseta antes de animar
        transform.DOPunchScale(punchScale, activeDuration * 2f, vibrato, elasticity); // *2f para dar tempo do punch resolver visualmente

        // Dispara evento
        OnAnimationTriggered?.Invoke();
    }

    private void RevertAction()
    {
        if (_targetImage != null)
        {
            _targetImage.sprite = idleSprite;
        }
    }
    
    // Método público caso queira disparar manualmente via código ou UnityEvent
    public void TriggerAnimationManual()
    {
        StopAnimationLoop();
        StartCoroutine(ManualTriggerRoutine());
    }

    private IEnumerator ManualTriggerRoutine()
    {
        PerformAction();
        yield return new WaitForSeconds(activeDuration);
        RevertAction();
        StartAnimationLoop(); // Retoma o loop automático
    }
}