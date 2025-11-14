using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ButtonColorSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color hoverColor = Color.magenta; // Cor rosa para quando o mouse estiver em cima
    private Color normalColor; // Cor azul original
    [SerializeField] private Image targetImage;
    [SerializeField] private float transitionDuration = 0.2f;
    [SerializeField] private float scaleUpFactor = 1.1f;

    void Start()
    {

        normalColor = targetImage.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetImage.color = hoverColor;
        // Muda para a cor rosa ao passar o mouse
        SmoothColorTransition(hoverColor, transitionDuration);
        SmoothScaleTransition(Vector3.one * scaleUpFactor, transitionDuration);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Retorna à cor azul original ao retirar o mouse
        targetImage.color = normalColor;
    }

    private void SmoothColorTransition(Color targetColor, float duration)
    {
        targetImage.DOColor(targetColor, duration);
    }
    private void SmoothScaleTransition(Vector3 targetScale, float duration)
    {
        targetImage.rectTransform.DOPunchScale(targetScale, duration);
    }
}