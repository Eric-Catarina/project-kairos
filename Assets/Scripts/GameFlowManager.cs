// Local: Assets/Scripts/Core/GameFlowManager.cs

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { MainMenu, Playing, Paused, LevelFinished }

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    public static event Action<float> OnLevelCompleted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;

    private UIManager _uiManager;
    private CountdownUI _countdownUI;
    
    private bool _isCountingDown = false;
    private Coroutine _countdownCoroutine;
    private bool _isSettingsPanelOpen = false;

    private bool IsInMainMenu => SceneManager.GetActiveScene().buildIndex == 0;

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
        InputManager.Instance.OnPausePressed += HandlePauseRequest;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnPausePressed -= HandlePauseRequest;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (_uiManager != null) _uiManager.OnPanelStateChanged -= HandlePanelStateChanged;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSceneReferences();
        _isSettingsPanelOpen = false;
        
        if (IsInMainMenu)
        {
            CurrentState = GameState.MainMenu;
            Time.timeScale = 1f;
        }
        else
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    private void FindSceneReferences()
    {
        if (_uiManager != null) _uiManager.OnPanelStateChanged -= HandlePanelStateChanged;
        _uiManager = FindObjectOfType<UIManager>(true);
        _countdownUI = FindObjectOfType<CountdownUI>(true);
        if (_uiManager != null) _uiManager.OnPanelStateChanged += HandlePanelStateChanged;
    }
    
    private void HandlePanelStateChanged(UIPanelType type, bool isOpen)
    {
        if (type == UIPanelType.Settings)
        {
            _isSettingsPanelOpen = isOpen;
        }
    }

    public void CompleteLevel()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.LevelFinished;
        Time.timeScale = 0f;
        InputStateManager.Instance.SwitchState(InputState.PostGame);

        ScoreManager.Instance.StopTimerAndGetResults(out float finalTime, out Rank finalRank);
        
        OnLevelCompleted?.Invoke(finalTime);
    }

    private void HandlePauseRequest()
    {
        if (_uiManager == null || CurrentState == GameState.LevelFinished) return;

        if (IsInMainMenu)
        {
            if (_isSettingsPanelOpen) _uiManager.ClosePanel(UIPanelType.Settings);
            else _uiManager.ShowPanel(UIPanelType.Settings);
        }
        else 
        {
            if (_isCountingDown)
            {
                StopResumeCountdown();
                PauseGame();
                return;
            }

            if (CurrentState == GameState.Paused)
            {
                _uiManager.ClosePanel(UIPanelType.Settings);
                ResumeGame();
            }
            else if (CurrentState == GameState.Playing)
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        InputStateManager.Instance.SwitchState(InputState.UI);
        _uiManager?.ShowPanel(UIPanelType.Settings);
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused || _isCountingDown) return;
        
        _countdownCoroutine = StartCoroutine(ResumeCountdown());
    }

    private void StopResumeCountdown()
    {
        if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
        _isCountingDown = false;
        if (_countdownUI?.panel != null) _countdownUI.panel.SetActive(false);
    }

    private IEnumerator ResumeCountdown()
    {
        _isCountingDown = true;
        InputStateManager.Instance.SwitchState(InputState.UI);
        if (_countdownUI == null || _countdownUI.panel == null || _countdownUI.text == null)
        {
            FinishResume();
            yield break;
        }
        _countdownUI.panel.SetActive(true);
        _countdownUI.text.text = "3";
        yield return new WaitForSecondsRealtime(1f);
        _countdownUI.text.text = "2";
        yield return new WaitForSecondsRealtime(1f);
        _countdownUI.text.text = "1";
        yield return new WaitForSecondsRealtime(1f);
        _countdownUI.panel.SetActive(false);
        FinishResume();
    }

    private void FinishResume()
    {
        CurrentState = GameState.Playing;
        _isCountingDown = false;
        _countdownCoroutine = null;
        Time.timeScale = 1f;
        InputStateManager.Instance.SwitchState(InputState.Gameplay);
        OnGameResumed?.Invoke();
    }
}