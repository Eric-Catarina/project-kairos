using UnityEngine;

public class MainMenu : UIPanel
{
    void Start()
    {
        if (InputStateManager.Instance == null)
        {
            Debug.LogError("InputStateManager não encontrado na cena.");
            return;
        }
        InputStateManager.Instance.SwitchState(InputState.UI);
        UIManager.Instance.ShowPanel(UIPanelType.MainMenu);

    }

}
