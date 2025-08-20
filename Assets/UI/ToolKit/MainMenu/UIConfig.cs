using UnityEngine.UIElements;
using UnityEngine;

public class UIConfig : MonoBehaviour
{
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Button>("new-game-button").clicked += () => Debug.Log("New Game clicked");
        root.Q<Button>("continue-button").clicked += () => Debug.Log("Continue clicked");
        root.Q<Button>("settings-button").clicked += () => Debug.Log("Settings clicked");
        root.Q<Button>("exit-button").clicked += () => Application.Quit();
    }
}
