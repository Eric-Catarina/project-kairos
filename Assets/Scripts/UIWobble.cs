// Assets/Scripts/UI/UIWobbleEffect.cs
using UnityEngine;
using DG.Tweening; // Não se esqueça de importar o DOTween

public class UIWobbleEffect : MonoBehaviour
{
    [Header("Configuração do Movimento")]
    [Tooltip("Distância máxima que o elemento se moverá no eixo X (esquerda/direita).")]
    public float moveDistanceX = 10f;

    [Tooltip("Distância máxima que o elemento se moverá no eixo Y (cima/baixo).")]
    public float moveDistanceY = 15f;

    [Header("Configuração de Tempo e Aleatoriedade")]
    [Tooltip("A duração base para cada movimento de 'onda'.")]
    public float baseDuration = 1.5f;

    [Tooltip("Controla o quão aleatório será o movimento. 0 = sem aleatoriedade (onda perfeita), 1 = muito aleatório.")]
    [Range(0f, 1f)]
    public float randomness = 0.5f;

    [Header("Controle")]
    [Tooltip("Se marcado, o efeito começa a tocar assim que o objeto for ativado.")]
    public bool playOnAwake = true;

    // --- Variáveis Privadas ---
    private Vector3 originalPosition;
    private Sequence mainSequence;

    private void Awake()
    {
        // Guarda a posição inicial para que o efeito seja sempre relativo a ela
        originalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        // Reinicia o efeito se o objeto for reativado
        if (playOnAwake)
        {
            Play();
        }
    }

    private void OnDisable()
    {
        // Para a animação quando o objeto é desativado para evitar erros
        Stop();
    }

    /// <summary>
    /// Inicia o efeito de ondulação.
    /// </summary>
    public void Play()
    {
        // Garante que não haja sequências antigas rodando
        if (mainSequence != null && mainSequence.IsActive())
        {
            mainSequence.Kill();
        }

        // Inicia o primeiro movimento
        StartWobble();
    }

    /// <summary>
    /// Para o efeito e retorna o elemento à sua posição original.
    /// </summary>
    public void Stop()
    {
        mainSequence?.Kill();
        // Opcional: Anima suavemente de volta à posição original
        transform.DOLocalMove(originalPosition, 0.2f).SetEase(Ease.OutCubic);
    }
    
    /// <summary>
    /// O coração do efeito. Esta função se chama recursivamente
    /// ao final de cada movimento para criar um loop infinito e aleatório.
    /// </summary>
    private void StartWobble()
    {
        // Calcula parâmetros aleatórios para ESTE movimento específico
        float randomDurationFactor = Random.Range(1f - randomness, 1f + randomness);
        float currentDuration = baseDuration * randomDurationFactor;

        // Calcula um ponto alvo aleatório dentro dos limites definidos
        float targetX = originalPosition.x + Random.Range(-moveDistanceX, moveDistanceX);
        float targetY = originalPosition.y + Random.Range(-moveDistanceY, moveDistanceY);

        // Cria a sequência de animação
        mainSequence = DOTween.Sequence(true).SetUpdate(UpdateType.Normal, true);
        mainSequence.Append(transform.DOLocalMove(new Vector3(targetX, targetY, originalPosition.z), currentDuration)
                .SetEase(Ease.InOutSine)) // Ease.InOutSine cria a sensação suave de "onda"
            .OnComplete(StartWobble); // Magia! Ao completar, chama a si mesma para o próximo movimento.
    }
}