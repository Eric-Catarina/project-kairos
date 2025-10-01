// Local: Assets/Scripts/Core/GameFlowManager.cs

using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public event Action OnGamePaused;
    public event Action OnGameResumed;

    [Header("Referências da UI")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

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
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePressed -= HandlePauseRequest;
        }
    }

    private void HandlePauseRequest()
    {
        // Se estivermos em contagem regressiva, pausar interrompe a contagem e volta ao menu de pausa.
        if (_isCountingDown)
        {
            PauseGame();
            return;
        }

        // Caso contrário, alterna normalmente entre pausado e despausado.
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

        // Se uma contagem regressiva estiver em andamento, pare-a.
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
            _isCountingDown = false;
            if (countdownPanel != null) countdownPanel.SetActive(false);
        }

        _isPaused = true;
        Time.timeScale = 0f;
        InputStateManager.Instance.SwitchState(InputState.UI);
        uiManager.ShowPanel(UIPanelType.Settings);
        
        OnGamePaused?.Invoke();
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        if (!_isPaused || _isCountingDown) return;

        uiManager.ClosePanel(UIPanelType.Settings);
        
        _countdownCoroutine = StartCoroutine(ResumeCountdown());
    }

    private IEnumerator ResumeCountdown()
    {
        _isCountingDown = true;

        if (countdownPanel == null || countdownText == null)
        {
            Debug.LogWarning("Referências do painel de contagem regressiva não estão atribuídas. Pulando contagem.");
            Time.timeScale = 1f;
            _isPaused = false;
            _isCountingDown = false;
            _countdownCoroutine = null;
            InputStateManager.Instance.SwitchState(InputState.Gameplay);
            OnGameResumed?.Invoke();
            yield break;
        }

        if (countdownPanel != null) countdownPanel.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "2";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "1";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "VAI!";
        yield return new WaitForSecondsRealtime(0.5f);

        if (countdownPanel != null) countdownPanel.SetActive(false);

        Time.timeScale = 1f;
        _isPaused = false;
        _isCountingDown = false;
        _countdownCoroutine = null;
        InputStateManager.Instance.SwitchState(InputState.Gameplay);

        OnGameResumed?.Invoke();
        Debug.Log("Game Resumed");
        
        
    }
}