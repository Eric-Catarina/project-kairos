// Local: Assets/Scripts/Scoring/ScoreManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Configuração do Nível")]
    [SerializeField] private LevelData currentLevelData;
    [Tooltip("Se marcado, o timer da fase começará com o primeiro input de movimento, pulo ou grapple.")]
    [SerializeField] private bool startLevelOnFirstMoveInput = false;
    
    [Header("Configurações de UI")]
    [Tooltip("Tempo em milissegundos para esperar a atualização do PlayFab antes de mostrar o leaderboard.")]
    private int leaderboardDisplayDelayMs = 750;

    private ScoreUIController _scoreUIController;
    private LeaderboardUIController _leaderboardUIController;
    private PlayerMovementController _playerMovementController;
    private GrapplingHookController _grapplingHookController;

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
        GameFlowManager.OnLevelCompleted += ProcessLevelCompletion;

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGamePaused += PauseTimer;
            GameFlowManager.Instance.OnGameResumed += ResumeTimer;
        }
        
        SubscribeToFirstInputEvents();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameFlowManager.OnLevelCompleted -= ProcessLevelCompletion;

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGamePaused -= PauseTimer;
            GameFlowManager.Instance.OnGameResumed -= ResumeTimer;
        }
        
        UnsubscribeFromFirstInputEvents();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSceneReferences();
        ResetLevelTimer();
        SubscribeToFirstInputEvents();
    }
    
    private void FindSceneReferences()
    {
        _scoreUIController = FindObjectOfType<ScoreUIController>(true);
        _leaderboardUIController = FindObjectOfType<LeaderboardUIController>(true);
        _playerMovementController = FindObjectOfType<PlayerMovementController>(true);
        _grapplingHookController = FindObjectOfType<GrapplingHookController>(true);
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
    
    public void StopTimerAndGetResults(out float finalTime, out Rank finalRank)
    {
        if (!_levelStarted)
        {
            finalTime = -1f;
            finalRank = Rank.None;
            return;
        }
        _isTimerRunning = false;
        finalTime = _levelTimer;
        finalRank = currentLevelData.GetRankForTime(_levelTimer);
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

    public Rank GetRankForTime(float time)
    {
        if (currentLevelData == null) return Rank.None;
        return currentLevelData.GetRankForTime(time);
    }

    private async void ProcessLevelCompletion(float finalTime)
    {
        SaveBestTime(finalTime);

        PostGamePanel postGamePanel = FindObjectOfType<PostGamePanel>(true);
        postGamePanel.gameObject.SetActive(true);
        postGamePanel.GetComponent<UIJuice>()?.PlayAnimation();

        bool submissionSuccess = await SubmitScoreAsync(finalTime);
        
        if (submissionSuccess)
        {
            await Task.Delay(leaderboardDisplayDelayMs);
        }
        
        _leaderboardUIController?.ShowLeaderboard();
    }

    private void SaveBestTime(float finalTime)
    {
        UserProfile activeProfile = SaveManager.Instance.GetActiveUserProfile();
        if (activeProfile == null || currentLevelData == null)
        {
            Debug.LogError("Perfil ativo ou LevelData não encontrado para salvar o recorde.");
            return;
        }

        string levelId = currentLevelData.GetFullLevelId();

        if (activeProfile.levelRecords.TryGetValue(levelId, out LevelRecord record))
        {
            if (finalTime < record.bestTime)
            {
                record.bestTime = finalTime;
                record.bestRank = GetRankForTime(finalTime);
            }
        }
        else
        {
            record = new LevelRecord
            {
                bestTime = finalTime,
                bestRank = GetRankForTime(finalTime)
            };
            activeProfile.levelRecords.Add(levelId, record);
        }

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
    
    private void SubscribeToFirstInputEvents()
    {
        if (InputManager.Instance != null && startLevelOnFirstMoveInput && _playerMovementController != null)
        {
            InputManager.Instance.OnMove += HandleFirstMoveInput;
            _playerMovementController.OnJumped += HandleFirstInput;
            _grapplingHookController.OnGrappleStarted += HandleFirstInput;
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
}