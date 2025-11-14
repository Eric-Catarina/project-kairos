using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image targetImage;
    [Space]
    [Header("Hover Settings")]
    [SerializeField] private Color hoverColor = Color.magenta;
    [SerializeField] private float hoverScaleFactor = 1.1f;
    [SerializeField] private float hoverDuration = 0.2f;

    [Header("Click Settings")]
    [SerializeField] private float punchScaleFactor = -0.1f; // Um valor negativo cria um efeito de "pressionar"
    [SerializeField] private float punchDuration = 0.1f;

    private Color normalColor;
    private Vector3 initialScale;
    private Sequence hoverSequence;
    private Sequence clickSequence;
    private Image selfImage;

    void Start()
    {
        // Garante que o targetImage foi atribuído para evitar erros
        if (targetImage == null)
        {
            Debug.LogError("Target Image não foi atribuída no Inspector!", this);
            return;
        }
        selfImage = GetComponent<Image>();

        // Armazena os valores iniciais
        normalColor = targetImage.color;
        initialScale = targetImage.transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Mata a animação anterior para evitar conflitos e executa a nova
        CreateHoverSequence(true).Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Mata a animação anterior e executa a de retorno
        CreateHoverSequence(false).Play();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Executa a animação de clique
        CreateClickSequence().Play();
    }

    // Cria a sequência de animação para o hover (entrar e sair)
    private Sequence CreateHoverSequence(bool isEntering)
    {
        // DOKill() mata qualquer tween em andamento no objeto, garantindo uma transição limpa
        if (hoverSequence != null && hoverSequence.IsActive())
        {
            hoverSequence.Kill();
        }

        hoverSequence = DOTween.Sequence();

        // Define a cor e escala de destino com base em se o mouse está entrando ou saindo
        Color targetColor = isEntering ? hoverColor : normalColor;
        Vector3 targetScale = isEntering ? initialScale * hoverScaleFactor : initialScale;

        // 'Join' faz com que ambas as animações rodem ao mesmo tempo
        hoverSequence.Join(targetImage.DOColor(targetColor, hoverDuration))
                     .Join(selfImage.transform.DOScale(targetScale, hoverDuration));

        
        // A MÁGICA ACONTECE AQUI:
        // Define a sequência para usar tempo não escalado, então funciona mesmo com Time.timeScale = 0
        hoverSequence.SetUpdate(UpdateType.Normal, true);

        return hoverSequence;
    }

    // Cria a sequência para o efeito de "punch" no clique
    private Sequence CreateClickSequence()
    {
        if (clickSequence != null && clickSequence.IsActive())
        {
            clickSequence.Kill();
        }

        clickSequence = DOTween.Sequence();
        
        clickSequence.Append(targetImage.transform.DOPunchScale(Vector3.one * punchScaleFactor, punchDuration, 0))
                     .SetUpdate(UpdateType.Normal, true);

        return clickSequence;
    }
}