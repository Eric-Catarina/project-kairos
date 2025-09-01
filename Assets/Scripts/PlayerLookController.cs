// Local: Assets/Scripts/PlayerLookController.cs

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem; // Adicione esta linha

/// <summary>
/// Controla a orientação de movimento do jogador e conecta a instância de input correta ao Cinemachine.
/// </summary>
public class PlayerLookController : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float playerRotationSpeed = 10f; // Aumentei um pouco para um feel melhor
    [SerializeField] private float mouseSensitivityX = 4f;
    [SerializeField] private float mouseSensitivityY = 4f;

    [Header("Referências")]
    [Tooltip("O modelo visual do jogador que deve rotacionar.")]
    [SerializeField] private Transform playerModel;
    [Tooltip("Um objeto vazio filho do Player que define a direção do movimento.")]
    [SerializeField] private Transform orientation;
    [Tooltip("Referência à câmera principal ou à câmera virtual do Cinemachine.")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Referência ao controlador de eixos da câmera virtual.")]
    [SerializeField] private CinemachineInputAxisController cinemachineInputAxisController;

    private void Start()
    {
        // Garante que o InputManager esteja pronto.
        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager não encontrado! A câmera não pode ser conectada.");
            return;
        }

        // --- PONTO CRÍTICO DA SOLUÇÃO ---
        // Conecta o Cinemachine à instância de PlayerControls gerenciada pelo InputManager.
        ConnectCinemachineToInputManager();
        // --- FIM DO PONTO CRÍTICO ---
    }

    private void Update()
    {
        HandlePlayerModelRotation();
    }

    /// <summary>
    /// Conecta os eixos do CinemachineInputAxisController diretamente às ações
    /// da instância PlayerControls que vive no nosso InputManager.
    /// Isso garante que, quando o InputStateManager desativa o mapa de ações 'Player',
    /// o Cinemachine para de receber input.
    /// </summary>
    private void ConnectCinemachineToInputManager()
    {
        // Obtém a ação 'Look' da nossa instância única de PlayerControls.
        InputAction lookAction = InputManager.Instance.PlayerControls.Player.Look;

        // Itera por cada controlador de eixo (X e Y) no componente do Cinemachine.
        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            // Cria uma referência para a ação de input.
            InputActionReference lookActionReference = InputActionReference.Create(lookAction);

            // Atribui a ação de input correta.
            controller.Input.InputAction = lookActionReference;

            // Configura a sensibilidade (exemplo de como fazer isso aqui).
            if (controller.Name == "Look Orbit X")
            {
                controller.Input.Gain = mouseSensitivityX;
            }
            if (controller.Name == "Look Orbit Y")
            {
                // O eixo Y geralmente é invertido.
                controller.Input.Gain = -mouseSensitivityY;
            }
        }
        
        Debug.Log("<color=cyan>CinemachineInputAxisController foi conectado com sucesso ao InputManager.</color>");
    }

    /// <summary>
    /// Gerencia a rotação do modelo do jogador para alinhar com a câmera.
    /// A rotação da câmera em si é 100% controlada pelo Cinemachine.
    /// </summary>
    private void HandlePlayerModelRotation()
    {
        if (cameraTransform == null || orientation == null || playerModel == null) return;

        // A orientação de movimento espelha a direção da câmera no plano horizontal.
        Vector3 viewDirection = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        if (viewDirection != Vector3.zero)
        {
            orientation.forward = viewDirection;
        }

        // O modelo do jogador gira suavemente para se alinhar com a orientação.
        // Usando Slerp para uma rotação mais suave.
        playerModel.forward = Vector3.Slerp(playerModel.forward, orientation.forward, playerRotationSpeed * Time.deltaTime);
    }
}