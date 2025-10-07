// Local: Assets/Scripts/Scoring/ScoreManager.cs

using System;
using System.Collections;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public static event Action<float, Rank> OnLevelCompleted;

    [Header("Configuração do Nível")]
    [SerializeField] private LevelData currentLevelData;
    private ScoreUIController scoreUIController;

    private float _levelTimer;
    private bool _isTimerRunning = false;
    private bool _levelStarted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        scoreUIController = FindObjectOfType<ScoreUIController>();
    }

    private void Start()
    {
        StartLevelTimer();
    }
    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLevelFinished += EndLevelTimer;
            InputManager.Instance.OnLevelRestarted += StartLevelTimer;
        }
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGamePaused += PauseTimer;
            GameFlowManager.Instance.OnGameResumed += ResumeTimer;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLevelFinished -= EndLevelTimer;
            InputManager.Instance.OnLevelRestarted -= StartLevelTimer;
        }
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGamePaused -= PauseTimer;
            GameFlowManager.Instance.OnGameResumed -= ResumeTimer;
        }
    }
    
    private void Update()
    {
        if (_isTimerRunning)
        {
            _levelTimer += Time.deltaTime;
            scoreUIController?.UpdateTime(_levelTimer);
        }
    }
    
    public void StartLevelTimer()
    {
        scoreUIController = FindAnyObjectByType<ScoreUIController>();
        _levelTimer = 0f;
        _levelStarted = true;
        _isTimerRunning = true;
        Debug.Log("Cronômetro do nível iniciado!");
    }

    private void PauseTimer()
    {
        _isTimerRunning = false;
    }

    private void ResumeTimer()
    {
        // Só retoma o timer se o nível já tiver começado
        if (_levelStarted)
        {
            _isTimerRunning = true;
        }
    }

    public void EndLevelTimer()
    {
        if (!_isTimerRunning && !_levelStarted) return;
        _isTimerRunning = false;
        _levelStarted = false;

        if (currentLevelData == null) { Debug.LogError("LevelData não está configurado!"); return; }

        Rank finalRank = currentLevelData.GetRankForTime(_levelTimer);
        Debug.Log($"Nível concluído! Tempo: {_levelTimer:F2}s - Ranque: {finalRank}");
        OnLevelCompleted?.Invoke(_levelTimer, finalRank);
    }
    public IEnumerator WaitAndStartLevelTimer()
    {
        return null;
    }
}