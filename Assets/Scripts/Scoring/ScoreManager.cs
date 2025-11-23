using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Configuração do Nível")]
    [SerializeField] private LevelData currentLevelData;
    [SerializeField] private bool startLevelOnFirstMoveInput = false;

    [Header("Configurações de Penalidade")]
    [Tooltip("Segundos a serem adicionados ao tempo ao completar a fase via atalho de debug.")]
    [SerializeField] private float debugCompletionPenalty = 70f;

    private ScoreUIController _scoreUIController;
    private LeaderboardUIController _leaderboardUIController;
    private PlayerMovementController _playerMovementController;
    private GrapplingHookController _grapplingHookController;

    private float _levelTimer;
    private bool _isTimerRunning = false;
    private bool _levelStarted = false;
    private int _deathCount = 0;
    private bool _levelCompleted = false;

    public float CurrentTime => _levelTimer;
    public int DeathCount => _deathCount;
    
    public bool IsLevelStarted => _levelStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.SetCurrentLevelData(currentLevelData);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnLevelCompleted += ProcessLevelCompletion;
            GameFlowManager.Instance.OnGamePaused += PauseTimer;
            GameFlowManager.Instance.OnGameResumed += ResumeTimer;
        }
        
        // Tenta se inscrever se o player já estiver cacheado (ex: reload de script)
        if (_playerMovementController != null)
        {
            _playerMovementController.OnResetToCheckpointFinished += HandleResetFinished;
        }
        
        SubscribeToFirstInputEvents();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnLevelCompleted -= ProcessLevelCompletion;
            GameFlowManager.Instance.OnGamePaused -= PauseTimer;
            GameFlowManager.Instance.OnGameResumed -= ResumeTimer;
        }
        
        if (_playerMovementController != null)
        {
            _playerMovementController.OnResetToCheckpointFinished -= HandleResetFinished;
        }
        
        UnsubscribeFromFirstInputEvents();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSceneReferences();
        ResetLevelTimer();
        LoadDeathCountForLevel();
        _levelCompleted = false;
        SubscribeToFirstInputEvents();
    }

    private void FindSceneReferences()
    {
        _scoreUIController = FindObjectOfType<ScoreUIController>(true);
        _leaderboardUIController = FindObjectOfType<LeaderboardUIController>(true);
        
        // Remove listeners antigos antes de buscar novas referências para evitar duplicação
        if (_playerMovementController != null)
        {
            _playerMovementController.OnResetToCheckpointFinished -= HandleResetFinished;
        }

        _playerMovementController = FindObjectOfType<PlayerMovementController>(true);
        _grapplingHookController = FindObjectOfType<GrapplingHookController>(true);

        // Se inscreve no novo player encontrado
        if (_playerMovementController != null)
        {
            _playerMovementController.OnResetToCheckpointFinished += HandleResetFinished;
        }
    }

    // *** LÓGICA NOVA: Chamado EXATAMENTE quando o DOTween termina ***
    private void HandleResetFinished()
    {
        if (startLevelOnFirstMoveInput && !_levelStarted)
        {
            // Verifica o input atual no InputManager (que agora lê direto do hardware)
            if (InputManager.Instance != null && InputManager.Instance.CurrentMoveInput.sqrMagnitude > 0.1f)
            {
                StartLevelTimer();
            }
        }
    }

    private void Update()
    {
        if (_isTimerRunning)
        {
            _levelTimer += Time.deltaTime;
            _scoreUIController?.UpdateTimeAndRank(_levelTimer);
        }
    }

    public void StartLevelTimer()
    {
        if (_levelStarted) return;
        _levelTimer = 0f;
        _levelStarted = true;
        _isTimerRunning = true;
        UnsubscribeFromFirstInputEvents();
    }

    public void StopTimerAndGetResults(out float finalTime, out Rank finalRank, out int deaths)
    {
        if (!_levelStarted)
        {
            finalTime = -1f;
            finalRank = Rank.NA;
            deaths = _deathCount;
            return;
        }
        _isTimerRunning = false;
        finalTime = _levelTimer;
        finalRank = currentLevelData.GetRankForTime(finalTime);
        deaths = _deathCount;
    }

    private void PauseTimer() { _isTimerRunning = false; }
    private void ResumeTimer() { if (_levelStarted) { _isTimerRunning = true; } }

    public void ResetLevelTimer()
    {
        _levelTimer = 0f;
        _isTimerRunning = false;
        _levelStarted = false;
        _scoreUIController?.UpdateTimeAndRank(_levelTimer);
    }

    public void SetCurrentTime(float newTime)
    {
        if (_levelStarted)
        {
            _levelTimer = Mathf.Max(0f, newTime);
        }
    }

    public void AddPenalty(float penaltySeconds)
    {
        if (_levelStarted)
        {
            _levelTimer += penaltySeconds;
        }
    }

    public Rank GetRankForTime(float time)
    {
        if (currentLevelData == null) return Rank.NA;
        return currentLevelData.GetRankForTime(time);
    }

    public void IncrementDeathCount()
    {
        _deathCount++;
    }

    private void LoadDeathCountForLevel()
    {
        UserProfile activeProfile = SaveManager.Instance.GetActiveUserProfile();
        if (activeProfile == null || currentLevelData == null)
        {
            _deathCount = 0;
            return;
        }

        string levelId = currentLevelData.GetFullLevelId();
        if (activeProfile.levelRecords.TryGetValue(levelId, out LevelRecord record))
        {
            _deathCount = record.totalDeaths;
        }
        else
        {
            _deathCount = 0;
        }
    }

    private async void ProcessLevelCompletion(LevelCompletionData data)
    {
        if (_levelCompleted) return;
        _levelCompleted = true;

        if (data.IsDebugWin)
        {
            data.FinalTime += debugCompletionPenalty;
            data.FinalRank = GetRankForTime(data.FinalTime);
        }

        SaveLevelStats(data.FinalTime, data.Deaths);

        VictoryPanelUI victoryPanel = FindObjectOfType<VictoryPanelUI>(true);
        if (victoryPanel != null)
        {
            victoryPanel.gameObject.SetActive(true);
            victoryPanel.GetComponent<UIJuice>()?.PlayAnimation();
            victoryPanel.ShowResults(data.FinalTime, data.Deaths, data.FinalRank);
        }

        await SubmitScoreAsync(data.FinalTime);

        await Task.Delay(System.TimeSpan.FromSeconds(0.75));

        _leaderboardUIController?.ShowLeaderboard();
    }

    private void SaveLevelStats(float finalTime, int totalDeathsForLevel)
    {
        UserProfile activeProfile = SaveManager.Instance.GetActiveUserProfile();
        if (activeProfile == null || currentLevelData == null)
        {
            Debug.LogError("Perfil ativo ou LevelData não encontrado para salvar o recorde.");
            return;
        }

        string levelId = currentLevelData.GetFullLevelId();

        if (!activeProfile.levelRecords.TryGetValue(levelId, out LevelRecord record))
        {
            record = new LevelRecord();
            activeProfile.levelRecords.Add(levelId, record);
        }
        
        if (finalTime < record.bestTime)
        {
            record.bestTime = finalTime;
            record.bestRank = GetRankForTime(finalTime);
        }
        
        record.totalDeaths = totalDeathsForLevel;
        SaveManager.Instance.SaveGame();
    }

    private async Task<bool> SubmitScoreAsync(float finalTime)
    {
        if (LeaderboardManager.Instance == null || PlayerProfile.Instance?.CurrentProfile == null)
        {
            Debug.LogError("LeaderboardManager ou PlayerProfile não estão disponíveis.");
            return false;
        }

        var scoreEntry = new ScoreEntry(
            PlayerProfile.Instance.CurrentProfile.profileId,
            PlayerProfile.Instance.CurrentProfile.profileName,
            finalTime,
            currentLevelData.GetFullLevelId()
        );

        bool success = await LeaderboardManager.Instance.SubmitScoreAsync(scoreEntry);

        if (success) { Debug.Log("Pontuação submetida com sucesso!"); }
        else { Debug.LogWarning("Falha ao submeter pontuação."); }

        return success;
    }

    private void HandleFirstInput()
    {
        if (startLevelOnFirstMoveInput && !_levelStarted)
        {
            StartLevelTimer();
        }
    }

    private void HandleFirstMoveInput(Vector2 moveValue)
    {
        if (moveValue.sqrMagnitude > 0.1f)
        {
            HandleFirstInput();
        }
    }

    public void SubscribeToFirstInputEvents()
    {
        if (InputManager.Instance != null && startLevelOnFirstMoveInput)
        {
            InputManager.Instance.OnMove += HandleFirstMoveInput;
            if (_playerMovementController != null) _playerMovementController.OnJumped += HandleFirstInput;
            if (_grapplingHookController != null) _grapplingHookController.OnGrappleStarted += HandleFirstInput;
            
            // Nota: Removemos a verificação imediata aqui para evitar o bug do reset.
            // Agora confiamos no evento HandleMove (para início normal) 
            // e no HandleResetFinished (para início pós-reset).
        }
    }

    private void UnsubscribeFromFirstInputEvents()
    {
        if (InputManager.Instance != null && _playerMovementController != null && _grapplingHookController != null)
        {
            InputManager.Instance.OnMove -= HandleFirstMoveInput;
            _playerMovementController.OnJumped -= HandleFirstInput;
            _grapplingHookController.OnGrappleStarted -= HandleFirstInput;
        }
    }

    public LevelData GetCurrentLevelData()
    {
        return currentLevelData;
    }
    
    public void SetCurrentLevelData(LevelData data)
    {
        currentLevelData = data;
    }
}