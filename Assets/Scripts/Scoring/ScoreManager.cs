// Local: Assets/Scripts/Scoring/ScoreManager.cs

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public static event Action<float, Rank> OnLevelCompleted;

    [Header("Configuração do Nível")]
    [SerializeField] private LevelData currentLevelData;
    
    private ScoreUIController _scoreUIController;
    private LeaderboardUIController _leaderboardUIController;

    private float _levelTimer;
    private bool _isTimerRunning = false;
    private bool _levelStarted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLevelFinished += HandleLevelFinished;
        }
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGamePaused += PauseTimer;
            GameFlowManager.Instance.OnGameResumed += ResumeTimer;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLevelFinished -= HandleLevelFinished;
        }
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGamePaused -= PauseTimer;
            GameFlowManager.Instance.OnGameResumed -= ResumeTimer;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSceneReferences();
        ResetLevelTimer();
    }
    
    private void FindSceneReferences()
    {
        _scoreUIController = FindObjectOfType<ScoreUIController>(true);
        _leaderboardUIController = FindObjectOfType<LeaderboardUIController>(true);
    }

    private void Update()
    {
        if (_isTimerRunning)
        {
            _levelTimer += Time.deltaTime;
            _scoreUIController?.UpdateTime(_levelTimer);
        }
    }
    
    public void StartLevelTimer()
    {
        _levelTimer = 0f;
        _levelStarted = true;
        _isTimerRunning = true;
    }

    private void PauseTimer()
    {
        _isTimerRunning = false;
    }

    private void ResumeTimer()
    {
        if (_levelStarted)
        {
            _isTimerRunning = true;
        }
    }

    public void ResetLevelTimer()
    {
        _levelTimer = 0f;
        _isTimerRunning = false;
        _levelStarted = false;
        _scoreUIController?.UpdateTime(_levelTimer);
    }

    private void HandleLevelFinished()
    {
        EndLevelTimer();
    }

    public async void EndLevelTimer()
    {
        if (!_isTimerRunning && !_levelStarted) return;
        _isTimerRunning = false;
        _levelStarted = false;

        if (currentLevelData == null) { Debug.LogError("LevelData não está configurado!"); return; }

        Rank finalRank = currentLevelData.GetRankForTime(_levelTimer);
        
        OnLevelCompleted?.Invoke(_levelTimer, finalRank);
        
        await SubmitScoreAsync();

        _leaderboardUIController.ShowLeaderboard();
    }

    private async System.Threading.Tasks.Task SubmitScoreAsync()
    {
        if (LeaderboardManager.Instance == null || PlayerProfile.Instance == null)
        {
            Debug.LogError("LeaderboardManager ou PlayerProfile não estão disponíveis para submeter a pontuação.");
            return;
        }

        var scoreEntry = new ScoreEntry(
            PlayerProfile.Instance.PlayerId,
            PlayerProfile.Instance.PlayerName,
            _levelTimer,
            currentLevelData.GetFullLevelId()
        );

        bool success = await LeaderboardManager.Instance.SubmitScoreAsync(scoreEntry);

        if (success)
        {
            Debug.Log("Pontuação submetida com sucesso!");
        }
        else
        {
            Debug.LogWarning("Falha ao submeter pontuação.");
        }
    }
}