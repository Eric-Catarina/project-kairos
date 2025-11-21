using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeManipulationManager : MonoBehaviour
{
    public static TimeManipulationManager Instance { get; private set; }
    
    public event Action<float> OnChargeChanged;

    [Header("Configurações da Bateria")]
    [Tooltip("Tempo máximo em segundos que a habilidade pode ser usada com carga total.")]
    [SerializeField] private float maxChargeDuration = 3f;
    [Tooltip("Tempo em segundos para recarregar a bateria de 0 a 100%.")]
    [SerializeField] private float rechargeDuration = 5f;

    [Header("Configurações de Input")]
    [Tooltip("Tempo máximo em segundos para considerar o clique como um 'Toggle'. Segurar mais que isso conta como 'Hold'.")]
    [SerializeField] private float holdThreshold = 0.25f;
    
    private float _currentCharge;
    private float _rechargeRate;
    private bool _isTimeSlowed = false;

    // Variáveis para lógica Híbrida (Toggle/Hold)
    private float _inputPressTime;
    private bool _wasSlowedBeforePress;

    public bool IsTimeSlowed => _isTimeSlowed;

    public event Action OnTimeStopStarted;
    public event Action OnTimeStopStopped;

    private readonly List<ITimeSlowable> _slowableObjects = new List<ITimeSlowable>();
    
    [Header("Configurações do Efeito")]
    [Tooltip("A porcentagem de lentidão a ser aplicada.")]
    [Range(0f, 100f)]
    [SerializeField] private float slowPercentage = 50f;
    [SerializeField] private Color slowDownColor = Color.cyan;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _currentCharge = maxChargeDuration;
        _rechargeRate = maxChargeDuration / rechargeDuration;
    }
    
    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSlowTimeInputStarted += HandleInputStarted;
            InputManager.Instance.OnSlowTimeInputCanceled += HandleInputCanceled;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSlowTimeInputStarted -= HandleInputStarted;
            InputManager.Instance.OnSlowTimeInputCanceled -= HandleInputCanceled;
        }

        if (_isTimeSlowed) DeactivateSlowTime();
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetCharge();
    }
    
    private void Start()
    {
        OnChargeChanged?.Invoke(_currentCharge / maxChargeDuration);
    }

    private void Update()
    {
        bool chargeChanged = false;
        
        if (_isTimeSlowed)
        {
            bool infiniteTimeStop = CheatManager.Instance != null && CheatManager.Instance.IsInfiniteTimeStopActive;
            if (!infiniteTimeStop)
            {
                _currentCharge -= Time.deltaTime;
                if (_currentCharge <= 0)
                {
                    _currentCharge = 0;
                    DeactivateSlowTime();
                }
            }
            chargeChanged = true;
        }
        else
        {
            if (_currentCharge < maxChargeDuration)
            {
                _currentCharge += _rechargeRate * Time.deltaTime;
                _currentCharge = Mathf.Min(_currentCharge, maxChargeDuration);
                chargeChanged = true;
            }
        }
        
        if (chargeChanged)
        {
            OnChargeChanged?.Invoke(_currentCharge / maxChargeDuration);
        }
    }
    
    private void HandleInputStarted()
    {
        _inputPressTime = Time.unscaledTime;
        _wasSlowedBeforePress = _isTimeSlowed;

        // Feedback imediato: se não estava parado, para agora (se tiver carga)
        if (!_isTimeSlowed && _currentCharge > 0.1f)
        {
            ActivateSlowTime();
        }
    }

    private void HandleInputCanceled()
    {
        float pressDuration = Time.unscaledTime - _inputPressTime;

        if (pressDuration < holdThreshold)
        {
            // Foi um TAP (Toggle)
            // Se já estava ativado antes de apertar, o tap serve para desligar
            if (_wasSlowedBeforePress)
            {
                DeactivateSlowTime();
            }
            // Se estava desligado, já ligamos no Started, então mantemos ligado
        }
        else
        {
            // Foi um HOLD (Segurou)
            // Ao soltar o botão depois de segurar, sempre desativa
            DeactivateSlowTime();
        }
    }

    private void ActivateSlowTime()
    {
        if (_isTimeSlowed) return;
        
        _isTimeSlowed = true;
        foreach (var slowable in _slowableObjects)
        {
            slowable?.SlowDown(slowPercentage);
        }
        OnTimeStopStarted?.Invoke();
    }

    private void DeactivateSlowTime()
    {
        if (!_isTimeSlowed) return;

        _isTimeSlowed = false;
        foreach (var slowable in _slowableObjects)
        {
            slowable?.RestoreNormalTime();
        }
        OnTimeStopStopped?.Invoke();
    }

    public void ResetCharge()
    {
        if (_isTimeSlowed)
        {
            DeactivateSlowTime();
        }

        _currentCharge = maxChargeDuration;
        OnChargeChanged?.Invoke(1f);
    }
    
    public void Register(ITimeSlowable slowable)
    {
        if (!_slowableObjects.Contains(slowable))
        {
            _slowableObjects.Add(slowable);
            slowable.SetSlowDownColor(slowDownColor);
        }
    }

    public void Unregister(ITimeSlowable slowable)
    {
        _slowableObjects.Remove(slowable);
    }
}