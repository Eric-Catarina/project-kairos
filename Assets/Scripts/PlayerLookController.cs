// Local: Assets/Scripts/PlayerLookController.cs

using UnityEngine;

/// <summary>
/// Controla a orientação de movimento e a rotação visual do jogador com base na câmera.
/// A câmera em si é controlada diretamente pelo Cinemachine.
/// </summary>
public class PlayerLookController : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Referências")]
    [Tooltip("O modelo visual do jogador que deve rotacionar.")]
    [SerializeField] private Transform playerModel;
    [Tooltip("Um objeto vazio filho do Player que define a direção do movimento.")]
    [SerializeField] private Transform orientation;
    [Tooltip("Referência à câmera principal ou à câmera virtual do Cinemachine.")]
    [SerializeField] private Transform cameraTransform;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (cameraTransform == null || orientation == null || playerModel == null) return;

        // A orientação de movimento espelha a direção da câmera no plano horizontal.
        Vector3 viewDirection = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        if (viewDirection != Vector3.zero)
        {
            orientation.forward = viewDirection;
        }

        // O modelo do jogador gira suavemente para se alinhar com a orientação.
        playerModel.forward = Vector3.Slerp(playerModel.forward, orientation.forward, rotationSpeed * Time.deltaTime);
    }
}