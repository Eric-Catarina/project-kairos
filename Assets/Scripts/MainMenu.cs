using UnityEngine;

[RequireComponent(typeof(UIPanel))]
public class MainMenu : MonoBehaviour
{
    public UIPanel uIPanel;
    void Start()
    {
        if (InputStateManager.Instance == null)
        {
            Debug.LogError("InputStateManager não encontrado na cena.");
            return;
        }
        InputStateManager.Instance.SwitchState(InputState.UI);
        UIManager.Instance.RegisterPanel(uIPanel);
        UIManager.Instance.ShowPanel(UIPanelType.MainMenu);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
