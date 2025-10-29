// Assets/Scripts/TimeManipulation/TimeManipulationManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeManipulationManager : MonoBehaviour
{
    public static TimeManipulationManager Instance { get; private set; }
    
    public event Action<float> OnChargeChanged;

    [Header("Configurações da Bateria")]
    [Tooltip("Tempo máximo em segundos que a habilidade pode ser usada com carga total.")]
    [SerializeField] private float maxChargeDuration = 3f;
    [Tooltip("Tempo em segundos para recarregar a bateria de 0 a 100%.")]
    [SerializeField] private float rechargeDuration = 5f;
    
    private float _currentCharge;
    private float _rechargeRate;
    private bool _isTimeSlowed = false;

    public bool IsTimeSlowed
    {
        get { return _isTimeSlowed; }
    }

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
            // MUDANÇA: Se inscreve no novo evento de toggle.
            InputManager.Instance.OnSlowTimeToggled += HandleSlowTimeToggle;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        
        // MUDANÇA: Se desinscreve do evento de toggle.
        InputManager.Instance.OnSlowTimeToggled -= HandleSlowTimeToggle;

        if (_isTimeSlowed) DeactivateSlowTime();
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
                    DeactivateSlowTime(); // Desativa automaticamente quando a carga acaba
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

    // NOVO MÉTODO: Lida com a lógica de toggle.
    private void HandleSlowTimeToggle()
    {
        // Se já está ativo, desativa.
        if (_isTimeSlowed)
        {
            DeactivateSlowTime();
        }
        // Se não está ativo, mas tem carga suficiente, ativa.
        else if (_currentCharge > 0.1f) 
        {
            ActivateSlowTime();
        }
    }

    private void ActivateSlowTime()
    {
        _isTimeSlowed = true;
        foreach (var slowable in _slowableObjects)
        {
            slowable?.SlowDown(slowPercentage);
        }
        OnTimeStopStarted?.Invoke();
    }

    private void DeactivateSlowTime()
    {
        _isTimeSlowed = false;
        foreach (var slowable in _slowableObjects)
        {
            slowable?.RestoreNormalTime();
        }
        OnTimeStopStopped?.Invoke();
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