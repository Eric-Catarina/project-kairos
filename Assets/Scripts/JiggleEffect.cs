using UnityEngine;
using DG.Tweening;

public class JiggleEffect : MonoBehaviour
{
    [Header("Configuração do Jiggle")]
    public float scaleAmount = 0.05f;       // Quanto a escala oscila
    public float positionAmount = 0.05f;    // Quanto a posição oscila
    public float duration = 0.3f;           // Duração de cada fase

    private Vector3 originalScale;
    private Vector3 originalPos;

    void Start()
    {
        originalScale = transform.localScale;
        originalPos = transform.localPosition;

        StartJiggle();
    }

    void StartJiggle()
    {
        Sequence seq = DOTween.Sequence().SetLoops(-1, LoopType.Yoyo);

        seq.Append(transform.DOScale(originalScale + new Vector3(scaleAmount, -scaleAmount, 0), duration).SetEase(Ease.InOutSine));
        seq.Join(transform.DOLocalMove(originalPos + new Vector3(positionAmount, positionAmount, 0), duration).SetEase(Ease.InOutSine));

        seq.Append(transform.DOScale(originalScale + new Vector3(-scaleAmount, scaleAmount, 0), duration).SetEase(Ease.InOutSine));
        seq.Join(transform.DOLocalMove(originalPos + new Vector3(-positionAmount, -positionAmount, 0), duration).SetEase(Ease.InOutSine));
    }
}