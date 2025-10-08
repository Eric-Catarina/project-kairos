// Local: Assets/Scripts/UI/UIManager.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private readonly Dictionary<UIPanelType, UIPanel> registeredPanels = new Dictionary<UIPanelType, UIPanel>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => FindAndRegisterAllPanels();

    private void FindAndRegisterAllPanels()
    {
        registeredPanels.Clear();
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
            if (panel.gameObject.activeInHierarchy && panel.PanelType != UIPanelType.MainMenu)
            {
                panel.gameObject.SetActive(false);
            }
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
        if (registeredPanels.TryGetValue(panelType, out UIPanel panel))
        {
            panel.Show();
        }
    }

    public void ClosePanel(UIPanelType panelType)
    {
        if (registeredPanels.TryGetValue(panelType, out UIPanel panel))
        {
            panel.Hide();
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
}