// Local: Assets/Scripts/UI/UIPanel.cs

using UnityEngine;

[RequireComponent(typeof(UIJuice))]
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

    protected virtual void Awake()
    {
        // Tenta se registrar o mais cedo possível
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RegisterPanel(this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UnregisterPanel(this);
        }
    }
    
    public virtual void Show()
    {
        // *** MUDANÇA CRÍTICA ***
        // Garante que o GameObject esteja ativo antes de iniciar a animação.
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        UiJuice.PlayAnimation();
    }

    public virtual void Hide()
    {
        UiJuice.PlayReverseAnimation();
    }
}