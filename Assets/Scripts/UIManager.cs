using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private readonly Dictionary<UIPanelType, UIPanel> registeredPanels = new Dictionary<UIPanelType, UIPanel>();
    private readonly Stack<UIPanel> panelStack = new Stack<UIPanel>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        FindAndRegisterAllPanels();
    }

    private void OnEnable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnPausePressed += HandlePauseToggle;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnPausePressed -= HandlePauseToggle;
    }

    private void FindAndRegisterAllPanels()
    {
        UIPanel[] allPanels = FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (UIPanel panel in allPanels)
        {
            RegisterPanel(panel);
        }
    }

    public void RegisterPanel(UIPanel panel)
    {
        if (!registeredPanels.ContainsKey(panel.PanelType))
        {
            registeredPanels.Add(panel.PanelType, panel);
            panel.gameObject.SetActive(false);
        }
    }

    public void ShowPanel(UIPanelType panelType)
    {
        if (!registeredPanels.TryGetValue(panelType, out UIPanel panelToShow))
        {
            Debug.LogWarning($"Painel do tipo {panelType} não foi registrado.");
            return;
        }

        if (panelStack.Count > 0)
        {
            panelStack.Peek().Hide();
        }

        panelToShow.Show();
        panelStack.Push(panelToShow);
        UpdateInputState();
    }

    public void CloseCurrentPanel()
    {
        if (panelStack.Count == 0) return;

        UIPanel panelToClose = panelStack.Pop();
        panelToClose.Hide();

        if (panelStack.Count > 0)
        {
            panelStack.Peek().Show();
        }

        UpdateInputState();
	Time.timeScale = 1;
    }

    private void HandlePauseToggle()
    {
        if (panelStack.Count > 0 && panelStack.Peek().PanelType == UIPanelType.Settings) // Supondo que Settings é o painel de pausa
        {
            CloseCurrentPanel();
            Time.timeScale = 1;
        }
        else if (panelStack.Count == 0)
        {
            ShowPanel(UIPanelType.Settings);
            Time.timeScale = 0;
        }
    }

    public void CloseCurrentPanelAndUnpause()
    {
        CloseCurrentPanel();
        Time.timeScale = 1; // Corrige o erro e garante que despausa.
    }

    private void UpdateInputState()
    {
        if (panelStack.Count > 0)
        {
            InputStateManager.Instance.SwitchState(InputState.UI);
        }
        else
        {
            InputStateManager.Instance.SwitchState(InputState.Gameplay);
        }
    }
}