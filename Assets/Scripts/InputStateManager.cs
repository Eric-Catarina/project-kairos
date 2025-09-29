// Local: Assets/Scripts/InputStateManager.cs

using UnityEngine;

[System.Serializable]
public enum InputState
{
    Gameplay,
    UI
}

/// <summary>
/// Gerencia o estado do input, alternando entre os Action Maps (Gameplay, UI)
/// e controlando o estado do cursor do mouse.
/// Implementa o padrão Singleton para acesso global.
/// </summary>
public class InputStateManager : MonoBehaviour
{
    public static InputStateManager Instance { get; private set; }

    private PlayerControls _playerControls;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Initialize(PlayerControls playerControls)
    {
        _playerControls = playerControls;
    }

    /// <summary>
    /// O método central para alternar o contexto de input do jogo.
    /// </summary>
    /// <param name="newState">O novo estado para o qual o input deve mudar.</param>
    public void SwitchState(InputState newState)
    {
        if (_playerControls == null) return;


        switch (newState)
        {
            case InputState.Gameplay:
                // Ativa os controles de gameplay
                _playerControls.Player.Enable();
                _playerControls.UI.Disable();

                // Esconde e trava o cursor no centro da tela
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case InputState.UI:
                // Ativa os controles de UI e desativa os de gameplay
                _playerControls.UI.Enable();
                _playerControls.Player.Disable();

                // Libera e mostra o cursor para interagir com a UI
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }

    public void SwitchToGameplay()
    {
        SwitchState(InputState.Gameplay);
    }
    public void SwitchToUI()
    {
        SwitchState(InputState.UI);
    }
}