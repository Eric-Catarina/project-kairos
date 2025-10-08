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
    }

    private void Start()
    {
        scoreUIController = FindObjectOfType<ScoreUIController>();
        if (scoreUIController == null)
        {
            Debug.LogWarning("ScoreUIController não encontrado na cena.");
        }
    }
    
    private void OnEnable()
    {
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
        if (_levelStarted)
        {
            _isTimerRunning = true;
        }
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
        Debug.Log($"Nível concluído! Tempo: {_levelTimer:F2}s - Ranque: {finalRank}");
        
        OnLevelCompleted?.Invoke(_levelTimer, finalRank);
        
        await SubmitScoreAsync();

        // Atualiza e exibe o leaderboard ao finalizar a fase
        LeaderboardUIController leaderboardUIController = FindObjectOfType<LeaderboardUIController>(true);
        leaderboardUIController?.ShowLeaderboard();
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