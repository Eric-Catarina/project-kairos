// Local: Assets/Scripts/UI/UIManager.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private readonly Dictionary<UIPanelType, UIPanel> registeredPanels = new Dictionary<UIPanelType, UIPanel>();
    
    [SerializeField, HideInInspector] private Stack<UIPanel> panelStack = new Stack<UIPanel>();
    [SerializeField] private List<UIPanel> panelStackView = new List<UIPanel>(); // Apenas para visualização no inspetor

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePressed += HandlePauseToggle;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePressed -= HandlePauseToggle;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndRegisterAllPanels();
    }

    private void FindAndRegisterAllPanels()
    {
        UIPanel[] allPanels = FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (UIPanel panel in allPanels)
        {
            RegisterPanel(panel);

        }
        SyncPanelStackView();
    }

    public void RegisterPanel(UIPanel panel)
    {
        if (!registeredPanels.ContainsKey(panel.PanelType))
        {
            registeredPanels.Add(panel.PanelType, panel);
            // panel.gameObject.SetActive(false);
        }
    }

    public void UnregisterPanel(UIPanel panel)
    {
        if (registeredPanels.ContainsKey(panel.PanelType))
        {
            registeredPanels.Remove(panel.PanelType);
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
        SyncPanelStackView();
        UpdateInputState();
    }

    public void CloseCurrentPanel()
    {
        if (panelStack.Count == 0) return;

        UIPanel panelToClose = panelStack.Pop();
        panelToClose.Hide();
        SyncPanelStackView();

        if (panelStack.Count > 0)
        {
            panelStack.Peek().Show();
        }

        UpdateInputState();
    }

    private void HandlePauseToggle()
    {
        if (panelStack.Count > 0 && panelStack.Peek().PanelType == UIPanelType.Settings)
        {
            CloseCurrentPanel();
        }
        else
        {
            ShowPanel(UIPanelType.Settings);
        }

    }

    private void UpdateInputState()
    {
        if (panelStack.Count > 0)
        {
            InputStateManager.Instance.SwitchState(InputState.UI);
        }
        else
        {
            Debug.Log("Switching to Gameplay state");
            InputStateManager.Instance.SwitchState(InputState.Gameplay);
        }
    }

    public void OpenSettingsPanel()
    {
        ShowPanel(UIPanelType.Settings);
    }
    
    public void CloseSettingsPanel()
    {
        
        if (panelStack.Count > 0 && panelStack.Peek().PanelType == UIPanelType.Settings)
        {
            CloseCurrentPanel();
        }
    }

    private void SyncPanelStackView()
    {
        panelStackView.Clear();
        panelStackView.AddRange(panelStack);
    }
}