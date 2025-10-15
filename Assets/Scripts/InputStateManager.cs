// Local: Assets/Scripts/InputStateManager.cs

using UnityEngine;

[System.Serializable]
public enum InputState
{
    Gameplay,
    UI
}

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

    public void SwitchState(InputState newState)
    {
        if (_playerControls == null) return;

        switch (newState)
        {
            case InputState.Gameplay:
                _playerControls.UI.Disable();
                _playerControls.Player.Enable();
                SetCursorState(false);
                break;

            case InputState.UI:
                _playerControls.Player.Disable();
                _playerControls.UI.Enable();
                SetCursorState(true);
                break;
        }
    }

    /// <summary>
    /// Centraliza o controle do estado do cursor.
    /// </summary>
    /// <param name="visible">True para o cursor de UI, False para o cursor de gameplay.</param>
    private void SetCursorState(bool visible)
    {
        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    public void SwitchToGameplay() => SwitchState(InputState.Gameplay);
    public void SwitchToUI() => SwitchState(InputState.UI);
}