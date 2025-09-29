// Local: Assets/Scripts/UI/SettingsPanel.cs

public class SettingsPanel : UIPanel
{

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        UIManager.Instance.RegisterPanel(this);
    }

    void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UnregisterPanel(this.PanelType);
        }
    }


}