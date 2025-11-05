using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Configuração de Penalidade")]
    [Tooltip("Segundos a serem adicionados ao tempo ao respawnar em um checkpoint.")]
    [SerializeField] private float penaltyPerCheckpoint = 5f;

    private List<Checkpoint> _checkpointsInLevel;
    private List<IResettable> _resettableObjects;
    private Checkpoint _lastActivatedCheckpoint;
    private PlayerMovementController _player;
    private bool _areCheckpointsEnabled;

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
        SceneManager.sceneLoaded += OnSceneLoaded;
        Checkpoint.OnCheckpointActivated += HandleCheckpointActivated;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnResetToCheckpoint += ResetToLastCheckpoint;
            InputManager.Instance.OnFullLevelReset += ResetLevelFromStart;
        }
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.OnCheckpointsEnabledChanged += UpdateCheckpointSetting;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Checkpoint.OnCheckpointActivated -= HandleCheckpointActivated;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnResetToCheckpoint -= ResetToLastCheckpoint;
            InputManager.Instance.OnFullLevelReset -= ResetLevelFromStart;
        }
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.OnCheckpointsEnabledChanged -= UpdateCheckpointSetting;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(InitializeAfterSceneLoad());
    }

    private IEnumerator InitializeAfterSceneLoad()
    {
        yield return null;

        _player = FindObjectOfType<PlayerMovementController>();
        _checkpointsInLevel = FindObjectsOfType<Checkpoint>().OrderBy(c => c.orderIndex).ToList();
        
        _resettableObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IResettable>().ToList();

        _lastActivatedCheckpoint = null;
        
        if (GameSettingsManager.Instance != null)
        {
            _areCheckpointsEnabled = GameSettingsManager.Instance.CheckpointsEnabled;
        }
        else
        {
            Debug.LogError("GameSettingsManager.Instance é nulo. Desativando checkpoints.");
            _areCheckpointsEnabled = false;
        }
    }

    private void UpdateCheckpointSetting(bool isEnabled)
    {
        _areCheckpointsEnabled = isEnabled;
    }

    private void HandleCheckpointActivated(Checkpoint activatedCheckpoint)
    {
        if (!_areCheckpointsEnabled) return;

        int newIndex = _checkpointsInLevel.IndexOf(activatedCheckpoint);
        int currentIndex = _lastActivatedCheckpoint != null ? _checkpointsInLevel.IndexOf(_lastActivatedCheckpoint) : -1;

        if (newIndex > currentIndex)
        {
            _lastActivatedCheckpoint = activatedCheckpoint;
        }
    }

    public void ResetToLastCheckpoint()
    {
        if (!_areCheckpointsEnabled || _lastActivatedCheckpoint == null)
        {
            ResetLevelFromStart();
            return;
        }

        ResetWorldState();
        ApplyTimePenaltyAndReset();

        if (_player != null)
        {
            _player.ResetToPosition(_lastActivatedCheckpoint.SpawnPoint.position, _lastActivatedCheckpoint.SpawnPoint.rotation);
        }
    }

    private void ApplyTimePenaltyAndReset()
    {
        if (ScoreManager.Instance == null || _lastActivatedCheckpoint.ActivationTime < 0) return;

        float stampedTime = _lastActivatedCheckpoint.ActivationTime;
        ScoreManager.Instance.SetCurrentTime(stampedTime);
        ScoreManager.Instance.AddPenalty(penaltyPerCheckpoint);
        
        Debug.Log($"Tempo restaurado para {stampedTime:F3}s com penalidade. Novo tempo: {ScoreManager.Instance.CurrentTime:F3}s");
    }

    private void ResetWorldState()
    {
        foreach (var resettable in _resettableObjects)
        {
            resettable.ResetState();
        }
    }

    private void ResetLevelFromStart()
    {
        SceneManagerLogic.Instance.RestartScene();
    }
}