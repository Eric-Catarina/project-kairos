// Local: Assets/Scripts/UI/UIPanel.cs

using UnityEngine;

[RequireComponent(typeof(UIJuice))]
public abstract class UIPanel : MonoBehaviour
{
    [SerializeField] private UIPanelType panelType;
    public UIPanelType PanelType => panelType;

    private UIJuice _uiJuice;

    // Usamos uma propriedade para "Lazy Initialization".
    // O código dentro do 'get' só é executado na primeira vez que UiJuice é acessado.
    protected UIJuice UiJuice
    {
        get
        {
            if (_uiJuice == null)
            {
                _uiJuice = GetComponent<UIJuice>();
            }
            return _uiJuice;
        }
    }

    /// <summary>
    /// Ativa o painel e inicia sua animação de entrada.
    /// </summary>
    public virtual void Show()
    {
        // Agora acessamos a propriedade UiJuice, que garante que a referência não seja nula.
        UiJuice.SetActiveAndPlay();
    }

    /// <summary>
    /// Inicia a animação de saída do painel. O objeto será desativado ao final da animação.
    /// </summary>
    public virtual void Hide()
    {
        // O mesmo aqui.
        UiJuice.PlayReverseAnimation();
    }
}