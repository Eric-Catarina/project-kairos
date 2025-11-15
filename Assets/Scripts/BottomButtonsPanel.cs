
using UnityEngine.UI;
using UnityEngine;

public class BottomButtonsPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  [Header("Referências dos Botões")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button resumeButton;

    private void OnEnable()
    {
        InitializeButtons();
    }

    void Start()
    {
        InitializeButtons();
    }
    private void InitializeButtons()
    {
        mainMenuButton?.onClick.RemoveAllListeners();
        resumeButton?.onClick.RemoveAllListeners();
        restartButton?.onClick.RemoveAllListeners();


        // Adiciona os listeners via código, garantindo que a referência ao Singleton seja sempre a mais recente
        if (SceneManagerLogic.Instance != null)
        {
            mainMenuButton?.onClick.AddListener(SceneManagerLogic.Instance.LoadMainMenu);
            restartButton?.onClick.AddListener(CheckpointManager.Instance.HardResetLevel);
            resumeButton?.onClick.AddListener(GameFlowManager.Instance.ResumeGame);
        }
        else
        {
            Debug.LogError("SceneManagerLogic.Instance não foi encontrado. Os botões do PostGamePanel não funcionarão.");
        }
    }

}
