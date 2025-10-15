using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum InputState
{
    Gameplay,
    UI,
    PostGame // Novo estado para o final da fase
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
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Initialize(PlayerControls playerControls)
    {
        _playerControls = playerControls;
    }

    public void SwitchState(InputState newState)
    {
        if (_playerControls == null) return;

        // Desabilita todos os mapas de ação primeiro para um estado limpo
        _playerControls.Player.Disable();
        _playerControls.UI.Disable();
        _playerControls.PostGame.Disable();

        switch (newState)
        {
            case InputState.Gameplay:
                _playerControls.Player.Enable();
                SetCursorState(false);
                break;

            case InputState.UI:
                _playerControls.UI.Enable();
                SetCursorState(true);
                break;

            case InputState.PostGame:
                // Habilita tanto a UI (para os botões) quanto o PostGame (para a tecla R)
                _playerControls.UI.Enable();
                _playerControls.PostGame.Enable();
                SetCursorState(true);
                break;
        }
    }

    private void SetCursorState(bool visible)
    {
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sempre começa no estado de Gameplay ao carregar uma nova cena
        SwitchState(InputState.Gameplay);
    }
    

}