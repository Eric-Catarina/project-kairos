// Local: Assets/Scripts/Core/GameFlowManager.cs

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public event Action OnGamePaused;
    public event Action OnGameResumed;

    private UIManager _uiManager;
    private CountdownUI _countdownUI;

    private bool _isPaused = false;
    private bool _isCountingDown = false;
    private Coroutine _countdownCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePressed += HandlePauseRequest;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePressed -= HandlePauseRequest;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSceneReferences();
    }

    private void FindSceneReferences()
    {
        _uiManager = FindObjectOfType<UIManager>(true);
        _countdownUI = FindObjectOfType<CountdownUI>(true);
    }

    private void HandlePauseRequest()
    {
        if (_isCountingDown)
        {
            PauseGame();
            return;
        }

        if (_isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        if (_isPaused) return;

        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
            _isCountingDown = false;
            if (_countdownUI != null && _countdownUI.panel != null) _countdownUI.panel.SetActive(false);
        }

        _isPaused = true;
        Time.timeScale = 0f;
        InputStateManager.Instance.SwitchState(InputState.UI);
        _uiManager?.ShowPanel(UIPanelType.Settings);
        
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (!_isPaused || _isCountingDown) return;

        _uiManager?.ClosePanel(UIPanelType.Settings);
        
        _countdownCoroutine = StartCoroutine(ResumeCountdown());
    }

    private IEnumerator ResumeCountdown()
    {
        _isCountingDown = true;

        if (_countdownUI == null || _countdownUI.panel == null || _countdownUI.text == null)
        {
            Time.timeScale = 1f;
            _isPaused = false;
            _isCountingDown = false;
            _countdownCoroutine = null;
            InputStateManager.Instance.SwitchState(InputState.Gameplay);
            OnGameResumed?.Invoke();
            yield break;
        }

        _countdownUI.panel.SetActive(true);

        _countdownUI.text.text = "3";
        yield return new WaitForSecondsRealtime(1f);

        _countdownUI.text.text = "2";
        yield return new WaitForSecondsRealtime(1f);

        _countdownUI.text.text = "1";
        yield return new WaitForSecondsRealtime(1f);

        _countdownUI.text.text = "VAI!";
        yield return new WaitForSecondsRealtime(0.5f);

        _countdownUI.panel.SetActive(false);

        Time.timeScale = 1f;
        _isPaused = false;
        _isCountingDown = false;
        _countdownCoroutine = null;
        InputStateManager.Instance.SwitchState(InputState.Gameplay);

        OnGameResumed?.Invoke();
    }
}