// Local: Assets/Scripts/Scoring/ScoreManager.cs

using System;
using UnityEngine;

/// <summary>
/// Gerencia o cronômetro, estado do nível e cálculo de pontuação.
/// Utiliza o padrão Singleton para acesso global fácil.
/// Dispara um evento OnLevelCompleted para desacoplar a lógica da UI. (Observer Pattern)
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // Evento que a UI e outros sistemas podem ouvir para saber quando o nível terminou.
    public static event Action<float, Rank> OnLevelCompleted;

    [Header("Configuração do Nível")]
    [Tooltip("Os dados de pontuação para o nível atual (tempos para ranques S, A, B, etc.).")]
    [SerializeField] private LevelData currentLevelData;

    private float _levelTimer;
    private bool _isTimerRunning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (_isTimerRunning)
        {
            _levelTimer += Time.deltaTime;
        }
    }

    private void OnEnable()
    {
        InputManager.Instance.OnLevelFinished += EndLevelTimer;
        InputManager.Instance.OnLevelRestarted += StartLevelTimer;
    }
    
    private void OnDisable()
    {
        InputManager.Instance.OnLevelFinished -= EndLevelTimer;
        InputManager.Instance.OnLevelRestarted -= StartLevelTimer;
    }

    /// <summary>
    /// Inicia ou reinicia o cronômetro do nível.
    /// Chame isso quando o jogador começar a fase.
    /// </summary>
    public void StartLevelTimer()
    {
        _levelTimer = 0f;
        _isTimerRunning = true;
        Debug.Log("Cronômetro do nível iniciado!");
    }

    /// <summary>
    /// Para o cronômetro, calcula o ranque e notifica os ouvintes.
    /// Chame isso quando o jogador cruzar a linha de chegada.
    /// </summary>
    public void EndLevelTimer()
    {
        if (!_isTimerRunning) return;

        _isTimerRunning = false;
        
        if (currentLevelData == null)
        {
            Debug.LogError("LevelData não está configurado no ScoreManager!");
            return;
        }

        Rank finalRank = currentLevelData.GetRankForTime(_levelTimer);
        
        Debug.Log($"Nível concluído! Tempo: {_levelTimer:F2}s - Ranque: {finalRank}");

        // Dispara o evento com os resultados finais.
        OnLevelCompleted?.Invoke(_levelTimer, finalRank);
    }
}