// Local: Assets/Scripts/UI/UIPanel.cs

using UnityEngine;

[RequireComponent(typeof(UIJuice))]
[System.Serializable]
public abstract class UIPanel : MonoBehaviour
{
    [SerializeField] private UIPanelType panelType;
    public UIPanelType PanelType => panelType;

    private UIJuice _uiJuice;
    
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

    protected virtual void OnEnable()
    {
        // Esta chamada é mais relevante para painéis instanciados em tempo de execução.
        // O UIManager já cuida dos painéis que existem na cena no carregamento.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RegisterPanel(this);
        }
    }

    protected virtual void OnDestroy()
    {
        // Garante que, se o painel for destruído, ele seja removido do dicionário do manager.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UnregisterPanel(this);
        }
    }
    
    public virtual void Show()
    {
        UiJuice.SetActiveAndPlay();
    }

    public virtual void Hide()
    {
        UiJuice.PlayReverseAnimation();
    }
}