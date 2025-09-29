using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        if (InputStateManager.Instance == null)
        {
            Debug.LogError("InputStateManager não encontrado na cena.");
            return;
        }
        InputStateManager.Instance.SwitchState(InputState.UI);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
