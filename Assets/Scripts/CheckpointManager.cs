using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;  // Adicionado para TMP_Text
using DG.Tweening;  // Adicionado para animações

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    
    [Header("Configuração de Spawn")]
    [Tooltip("Ponto de spawn inicial do jogador na fase. Obrigatório para o reset sem recarregar a cena.")]
    [SerializeField] private Transform initialSpawnPoint;

    [Header("Configuração de Penalidade")]
    [Tooltip("Segundos a serem adicionados ao tempo ao respawnar em um checkpoint.")]
    [SerializeField] private float penaltyPerCheckpoint = 5f;

    [Header("Feedback de UI")]  // Novo: Seção para feedback
    [Tooltip("Nome do GameObject que contém o TMP_Text para feedback (ex.: 'CheckpointFeedbackText'). Deve existir na cena.")]
    [SerializeField] private string feedbackTextObjectName = "CheckpointFeedbackText";
    [Tooltip("Texto a ser exibido no feedback.")]
    [SerializeField] private string feedbackMessage = "Checkpoint Salvo!";
    [Tooltip("Duração total da exibição (em segundos).")]
    [SerializeField] private float displayDuration = 1f;
    [Tooltip("Duração do fade in/out (em segundos).")]
    [SerializeField] private float fadeDuration = 0.2f;

    private List<Checkpoint> _checkpointsInLevel;
    private List<IResettable> _resettableObjects;
    private Checkpoint _lastActivatedCheckpoint;
    private PlayerMovementController _player;
    [SerializeField] private ParticleSystem _respawnEffect;
    private bool _areCheckpointsEnabled;
    private TMP_Text feedbackText;  // Novo: Referência privada ao TMP_Text

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
            InputManager.Instance.OnResetToCheckpoint += SoftResetToCheckpoint;
            InputManager.Instance.OnFullLevelReset += HardResetLevel;
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
            InputManager.Instance.OnResetToCheckpoint -= SoftResetToCheckpoint;
            InputManager.Instance.OnFullLevelReset -= HardResetLevel;
        }
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.OnCheckpointsEnabledChanged -= UpdateCheckpointSetting;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(InitializeAfterSceneLoad());
        if (initialSpawnPoint == null)
        {
            initialSpawnPoint = GameObject.Find("Core")?.transform;
        }
    }

    private IEnumerator InitializeAfterSceneLoad()
    {
        yield return null;

        _player = FindObjectOfType<PlayerMovementController>();
        _checkpointsInLevel = FindObjectsOfType<Checkpoint>().OrderBy(c => c.orderIndex).ToList();
        
        _resettableObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IResettable>().ToList();

        // Novo: Localiza o TMP_Text na cena atual
        feedbackText = GameObject.Find(feedbackTextObjectName)?.GetComponent<TMP_Text>();
        if (feedbackText == null)
        {
            Debug.LogWarning($"TMP_Text para feedback não encontrado. Certifique-se de que um GameObject chamado '{feedbackTextObjectName}' com TMP_Text existe na cena.");
        }

        _lastActivatedCheckpoint = null;
        _player.OnResetToCheckpointFinished += () => 
        {
            PlayResetToCheckpointEffect(_player.transform.position);
        };
        
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
            ShowCheckpointFeedback();  // Novo: Mostra o feedback
        }
    }
    
    public void SoftResetToCheckpoint()
    {
        ScoreManager.Instance?.IncrementDeathCount();
        ScoreManager.Instance?.SubscribeToFirstInputEvents();
        TimeManipulationManager.Instance?.ResetCharge();
        

        if (!_areCheckpointsEnabled || _lastActivatedCheckpoint == null)
        {
            SoftResetToStart();
            return;
        }

        ResetWorldState();
        ApplyTimePenaltyAndReset();

        if (_player != null)
        {
            _player.ResetToPosition(_lastActivatedCheckpoint.SpawnPoint.position);
        }
    }

    private void ApplyTimePenaltyAndReset()
    {
        if (ScoreManager.Instance == null || _lastActivatedCheckpoint.ActivationTime < 0) return;

        float stampedTime = _lastActivatedCheckpoint.ActivationTime;
        ScoreManager.Instance.SetCurrentTime(stampedTime);
        ScoreManager.Instance.AddPenalty(penaltyPerCheckpoint);
    }

    private void ResetWorldState()
    {
        foreach (var resettable in _resettableObjects)
        {
            resettable.ResetState();
        }
    }
    
    private void SoftResetToStart()
    {
        if (initialSpawnPoint == null)
        {
            HardResetLevel();
            return;
        }

        ResetWorldState();
        ScoreManager.Instance?.ResetLevelTimer();
        _lastActivatedCheckpoint = null;

        if (_player != null)
        {
            _player.ResetToPosition(initialSpawnPoint.position);
        }
    }

    public void HardResetLevel()
    {
        ScoreManager.Instance?.IncrementDeathCount();
        SceneManagerLogic.Instance.RestartScene();
    }

    private void PlayResetToCheckpointEffect(Vector3 position)
    {
        if (_respawnEffect != null)
        {
            ParticleSystem spawnedEffect = Instantiate(_respawnEffect, position, Quaternion.identity);
            spawnedEffect.Play();
        }
    }

    // Novo: Método para mostrar o feedback de UI
    private void ShowCheckpointFeedback()
    {
        if (feedbackText == null)
        {
            Debug.LogWarning("Tentativa de mostrar feedback, mas TMP_Text é nulo. Verifique se foi localizado na cena.");
            return;
        }

        feedbackText.text = feedbackMessage;
        feedbackText.alpha = 0f;
        feedbackText.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(feedbackText.DOFade(1f, fadeDuration))
                .AppendInterval(displayDuration)
                .Append(feedbackText.DOFade(0f, fadeDuration))
                .OnComplete(() => feedbackText.gameObject.SetActive(false));
    }
}
