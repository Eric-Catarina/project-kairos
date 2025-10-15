// Local: Assets/Scripts/UI/MainMenu.cs

using UnityEngine;
using UnityEngine.UI;

// UIPanel já garante a presença de UIJuice
public class MainMenu : UIPanel
{
    [Header("Referências dos Botões")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button extraButton; // Assumindo que você terá uma lógica para ele
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    // A lógica de inicialização deve estar em Awake ou OnEnable para garantir a ordem de execução

    void OnEnable()
    {

        // Lógica que estava no Start, agora em OnEnable para ser mais robusta
        if (InputStateManager.Instance != null)
        {
            InputStateManager.Instance.SwitchState(InputState.UI);
        }
        else
        {
            Debug.LogError("InputStateManager não encontrado. O estado do input pode não estar correto.");
        }

        // O UIManager já cuida de registrar e mostrar o painel.
        // A chamada UIManager.Instance.ShowPanel(UIPanelType.MainMenu) pode ser feita pelo
        // seu script de inicialização de cena, se necessário, mas OnEnable é um bom lugar.
    }

    void Start()
    {
        InputStateManager.Instance?.SwitchState(InputState.UI);
        SetupButtonListeners();

    }

    private void SetupButtonListeners()
    {
        // Remove quaisquer listeners antigos do Inspector para evitar chamadas duplicadas
        startButton?.onClick.RemoveAllListeners();
        extraButton?.onClick.RemoveAllListeners();
        settingsButton?.onClick.RemoveAllListeners();
        exitButton?.onClick.RemoveAllListeners();

        // Atribui as funções via código, usando as instâncias atuais dos singletons
        if (SceneManagerLogic.Instance != null)
        {
            // O botão Start deve carregar o primeiro nível (ou o que você definir)
            startButton?.onClick.AddListener(() => SceneManagerLogic.Instance.LoadSceneByName("Hellcat")); // Exemplo: Carrega a cena "Level 1"
            exitButton?.onClick.AddListener(SceneManagerLogic.Instance.QuitGame);
        }
        else
        {
            Debug.LogError("SceneManagerLogic não encontrado! Os botões Start e Exit não funcionarão.");
        }
        
        if (UIManager.Instance != null)
        {
            settingsButton?.onClick.AddListener(UIManager.Instance.OpenSettingsPanel);
        }
        else
        {
             Debug.LogError("UIManager não encontrado! O botão Settings não funcionará.");
        }

        // Lógica para o botão Extra (Exemplo: abrir um painel de extras)
        extraButton?.onClick.AddListener(HandleExtraButtonClicked);
    }

    private void HandleExtraButtonClicked()
    {
        // Implemente aqui a lógica para o botão "Extra"
        Debug.Log("Botão Extra clicado! (Implemente a funcionalidade aqui)");
        // Exemplo: UIManager.Instance.ShowPanel(UIPanelType.Extras);
    }
}