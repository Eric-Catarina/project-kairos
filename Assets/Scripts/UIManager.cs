// Local: Assets/Scripts/UI/UIManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public event Action<UIPanelType, bool> OnPanelStateChanged; // bool: true para mostrado, false para escondido

    private GameObject activePanel;

    private readonly Dictionary<UIPanelType, UIPanel> _registeredPanels = new Dictionary<UIPanelType, UIPanel>();
    
    [Header("Debug Info")]
    [SerializeField] private List<string> _registeredPanelNames = new List<string>();

    private bool _isInitialized = false;

    // ... (Awake, OnEnable, OnDisable, OnSceneLoaded, FindAndRegisterAllPanels, RegisterPanel, UnregisterPanel permanecem os mesmos) ...
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (!_isInitialized)
        {
            FindAndRegisterAllPanels();
        }
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndRegisterAllPanels();
    }

    private void FindAndRegisterAllPanels()
    {
        _registeredPanels.Clear();
        UIPanel[] allPanels = FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (UIPanel panel in allPanels)
        {
            RegisterPanel(panel);
        }
        _isInitialized = true;
        UpdateDebugList();
    }

    public void RegisterPanel(UIPanel panel)
    {
        if (panel == null) return;
        if (!_registeredPanels.ContainsKey(panel.PanelType)) { _registeredPanels.Add(panel.PanelType, panel); }
        else { _registeredPanels[panel.PanelType] = panel; }
        UpdateDebugList();
    }

    public void UnregisterPanel(UIPanel panel)
    {
        if (panel != null && _registeredPanels.ContainsKey(panel.PanelType))
        {
            _registeredPanels.Remove(panel.PanelType);
            UpdateDebugList();
        }
    }
    
    public void ShowPanel(UIPanelType panelType)
    {
        if (_registeredPanels.TryGetValue(panelType, out UIPanel panel) && panel != null)
        {
            panel.Show();
            OnPanelStateChanged?.Invoke(panelType, true); // Dispara o evento
        }
    }

    public void ClosePanel(UIPanelType panelType)
    {
        if (_registeredPanels.TryGetValue(panelType, out UIPanel panel) && panel != null)
        {
            panel.Hide();
            OnPanelStateChanged?.Invoke(panelType, false); // Dispara o evento
        }
    }

    public bool IsPanelActive(UIPanelType panelType)
    {
        if (_registeredPanels.TryGetValue(panelType, out UIPanel panel) && panel != null)
        {
            return panel.gameObject.activeInHierarchy;
        }
        return false;
    }
    
    private void UpdateDebugList()
    {
        _registeredPanelNames.Clear();
        foreach (var pair in _registeredPanels)
        {
            //_registeredPanelNames.Add($"[{pair.Key}] -> {pair.Value?.gameObject.name ?? "REFERÊNCIA NULA"}");
        }
    }

    public void OpenSettingsPanel()
    {
        ShowPanel(UIPanelType.Settings);
    }

    public void CloseSettingsPanel()
    {
        ClosePanel(UIPanelType.Settings);
    }

    public void SetActivePanel(GameObject newPanel)
    {
        if (activePanel != null)
            activePanel.SetActive(false);

        newPanel.SetActive(true);
        activePanel = newPanel;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }
}