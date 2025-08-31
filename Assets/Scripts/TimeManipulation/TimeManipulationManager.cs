// Local: Assets/Scripts/TimeManipulation/TimeManipulationManager.cs

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia o estado global da manipulação do tempo para objetos específicos.
/// Implementa o padrão Singleton para ser o ponto central de controle.
/// Ouve o InputManager para ativar/desativar o efeito em todos os objetos registrados.
/// </summary>
public class TimeManipulationManager : MonoBehaviour
{
    public static TimeManipulationManager Instance { get; private set; }

    [Header("Configurações da Habilidade")]
    [Tooltip("A porcentagem de lentidão a ser aplicada. 0 = sem efeito, 50 = 50% da velocidade, 100 = completamente parado.")]
    [Range(0f, 100f)]
    [SerializeField] private float slowPercentage = 50f;
    [SerializeField] private Color slowDownColor = Color.cyan;

    // Lista de todos os objetos que podem ser desacelerados na cena.
    private readonly List<ITimeSlowable> _slowableObjects = new List<ITimeSlowable>();
    private bool _isTimeSlowed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        InputManager.Instance.OnSlowTimeStarted += HandleSlowTimeStarted;
        InputManager.Instance.OnSlowTimeCanceled += HandleSlowTimeCanceled;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnSlowTimeStarted -= HandleSlowTimeStarted;
        InputManager.Instance.OnSlowTimeCanceled -= HandleSlowTimeCanceled;

        // Garante que o tempo seja restaurado se o manager for desativado.
        if (_isTimeSlowed)
        {
            HandleSlowTimeCanceled();
        }
    }
    
    /// <summary>
    /// Registra um objeto na lista para que ele possa ser afetado pela manipulação do tempo.
    /// </summary>
    public void Register(ITimeSlowable slowable)
    {
        if (!_slowableObjects.Contains(slowable))
        {
            _slowableObjects.Add(slowable);
            slowable.SetSlowDownColor(slowDownColor);
        }
    }

    /// <summary>
    /// Remove um objeto da lista, geralmente quando ele é destruído.
    /// </summary>
    public void Unregister(ITimeSlowable slowable)
    {
        if (_slowableObjects.Contains(slowable))
        {
            _slowableObjects.Remove(slowable);
        }
    }

    private void HandleSlowTimeStarted()
    {
        if (_isTimeSlowed) return;
        _isTimeSlowed = true;
        
        // Percorre a lista de trás para frente para evitar problemas se um objeto for removido durante a iteração.
        for (int i = _slowableObjects.Count - 1; i >= 0; i--)
        {
            _slowableObjects[i].SlowDown(slowPercentage);
        }
    }

    private void HandleSlowTimeCanceled()
    {
        if (!_isTimeSlowed) return;
        _isTimeSlowed = false;
        
        for (int i = _slowableObjects.Count - 1; i >= 0; i--)
        {
            _slowableObjects[i].RestoreNormalTime();
        }
    }
}