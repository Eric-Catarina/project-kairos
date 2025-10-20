// Local: Assets/Scripts/UI/PostGamePanel.cs

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIJuice))]
public class PostGamePanel : MonoBehaviour
{
    [Header("Referências dos Botões")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button nextLevelButton;

    private UIJuice _uiJuice;

    private void Awake()
    {
        _uiJuice = GetComponent<UIJuice>();
        
        // Remove quaisquer listeners antigos configurados no Inspector para evitar chamadas duplas
        restartButton?.onClick.RemoveAllListeners();
        mainMenuButton?.onClick.RemoveAllListeners();
        nextLevelButton?.onClick.RemoveAllListeners();

        // Adiciona os listeners via código, garantindo que a referência ao Singleton seja sempre a mais recente
        if (SceneManagerLogic.Instance != null)
        {
            restartButton?.onClick.AddListener(SceneManagerLogic.Instance.RestartScene);
            mainMenuButton?.onClick.AddListener(SceneManagerLogic.Instance.LoadMainMenu);
            nextLevelButton?.onClick.AddListener(SceneManagerLogic.Instance.LoadNextScene);
        }
        else
        {
            Debug.LogError("SceneManagerLogic.Instance não foi encontrado. Os botões do PostGamePanel não funcionarão.");
        }
    }


}