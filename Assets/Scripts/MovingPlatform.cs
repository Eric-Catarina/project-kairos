using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour, ITimeSlowable, IResettable, IGrappleable
{
    // O evento OnGrappabilityChanged não é mais necessário, pois o GrapplingHookController
    // fará uma verificação ativa via IGrappleable.

    [Header("Configurações de Patrulha")]
    [SerializeField] private Transform targetA;
    [SerializeField] private Transform targetB;
    [Tooltip("A velocidade da plataforma em unidades por segundo.")]
    public float speed = 3f;
    [Tooltip("Fator de influência no movimento do jogador quando está sobre a plataforma.")]
    public float playerInfluence = 0.69f;

    [Header("Visual")]
    [SerializeField] private Color slowDownColor = Color.cyan;

    [Header("Configurações de Grapple")]
    [Tooltip("Se marcado, a plataforma pode ser agarrada (grapple) mesmo quando está se movendo normalmente. Se desmarcado, só pode ser agarrada com o tempo parado.")]
    [SerializeField] private bool canBeGrappledWhileMoving = true;
    [Tooltip("Nome da camada para a qual a plataforma será movida quando puder ser agarrada. Certifique-se de que o GrapplingHookController possa interagir com esta camada.")]
    [SerializeField] private string grappleableLayerName = "Grappleable";
    [Tooltip("Nome da camada original da plataforma, para onde ela retorna quando não pode ser agarrada. Será detectada automaticamente se deixado vazio.")]
    [SerializeField] private string defaultLayerName = ""; // Preenche automaticamente

    private Rigidbody _rb;
    private Renderer _renderer;
    private Color _originalColor;

    // --- NOVA ARQUITETURA DE MOVIMENTO ---
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private float _journeyLength;
    private float _journeyTravelled;
    private float _originalSpeed; // Para salvar a velocidade durante o slow-motion

    // --- ESTADO ---
    private bool _isSlowed = false;
    private Vector3 _initialPosition; // Para IResettable
    private int _originalLayer; // Armazena a camada inicial do objeto
    private int _grappleableLayerIndex;
    private int _nonGrappleableLayerIndex; // Usaremos a camada original como a "não-grappable" se a checkbox estiver desmarcada

    // private bool _currentGrappabilityState; // Não é mais necessário rastrear aqui, a interface lida com isso.

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        if (_renderer != null) _originalColor = _renderer.material.color;

        _rb.useGravity = false;
        _rb.freezeRotation = true;
        
        _rb.isKinematic = true; 
        
        _initialPosition = transform.position;

        // Armazena a camada original e resolve os índices de camada
        _originalLayer = gameObject.layer;
        _grappleableLayerIndex = LayerMask.NameToLayer(grappleableLayerName);

        // Se o nome da camada padrão não for especificado, usamos a camada original
        if (string.IsNullOrEmpty(defaultLayerName))
        {
            _nonGrappleableLayerIndex = _originalLayer;
        }
        else
        {
            _nonGrappleableLayerIndex = LayerMask.NameToLayer(defaultLayerName);
        }

        if (_grappleableLayerIndex == -1)
        {
            Debug.LogError($"Camada 'Grappleable' ({grappleableLayerName}) não encontrada para MovingPlatform em {gameObject.name}. Verifique as configurações de Layer do projeto.", this);
            _grappleableLayerIndex = _originalLayer; 
        }
        if (_nonGrappleableLayerIndex == -1)
        {
            Debug.LogError($"Camada 'Não-Grappleable' ({defaultLayerName}) não encontrada para MovingPlatform em {gameObject.name}. Usando a camada original.", this);
            _nonGrappleableLayerIndex = _originalLayer;
        }
    }

    private void Start()
    {
        if (targetA == null || targetB == null)
        {
            Debug.LogError("Os alvos de patrulha (Target A e Target B) não foram configurados!", this);
            enabled = false;
            return;
        }

        InitializeJourney();
        _originalSpeed = speed;

        TimeManipulationManager.Instance.Register(this);
        UpdateGrappleableLayer(); // Define o estado inicial da camada
    }

    private void OnDisable()
    {
        if (_isSlowed) RestoreNormalTime();
        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.Unregister(this);
        }
    }

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    /// <summary>
    /// Lógica principal de movimento. Calcula a posição exata da plataforma
    /// ao longo do trajeto com base no tempo e na velocidade, evitando o problema de "overshooting".
    /// </summary>
    private void UpdateMovement()
    {
        if (_journeyLength <= 0) return;

        _journeyTravelled += speed * Time.fixedDeltaTime;

        if (_journeyTravelled >= _journeyLength)
        {
            float overshoot = _journeyTravelled - _journeyLength;
            SwapEndpoints();
            _journeyTravelled = overshoot;
        }

        float fractionOfJourney = _journeyTravelled / _journeyLength;

        Vector3 newPosition = Vector3.Lerp(_startPoint, _endPoint, fractionOfJourney);
        _rb.MovePosition(newPosition);
    }
    
    /// <summary>
    /// Configura ou reconfigura o estado inicial do trajeto da plataforma.
    /// </summary>
    private void InitializeJourney()
    {
        transform.position = _initialPosition; // Garante que a plataforma comece na posição certa
        _startPoint = targetA.position;
        _endPoint = targetB.position;
        _journeyLength = Vector3.Distance(_startPoint, _endPoint);
        _journeyTravelled = 0f;
    }
    
    /// <summary>
    /// Inverte os pontos de início e fim para fazer a plataforma voltar.
    /// </summary>
    private void SwapEndpoints()
    {
        Vector3 temp = _startPoint;
        _startPoint = _endPoint;
        _endPoint = temp;
    }

    #region ITimeSlowable Implementation

    public void SlowDown(float slowPercentage)
    {
        if (_isSlowed) return;
        _isSlowed = true;

        float slowFactor = 1.0f - (slowPercentage / 100.0f);
        speed = _originalSpeed * slowFactor;

        if (_renderer != null) _renderer.material.color = slowDownColor;
        UpdateGrappleableLayer(); // Atualiza a camada quando o tempo é lento
    }

    public void RestoreNormalTime()
    {
        if (!_isSlowed) return;
        _isSlowed = false;

        speed = _originalSpeed;

        if (_renderer != null) _renderer.material.color = _originalColor;
        UpdateGrappleableLayer(); // Atualiza a camada quando o tempo volta ao normal
    }

    public void SetSlowDownColor(Color newColor)
    {
        slowDownColor = newColor;
    }
    
    #endregion

    #region IResettable Implementation
    
    public void ResetState()
    {
        InitializeJourney();
        RestoreNormalTime();
        UpdateGrappleableLayer(); // Garante que a camada seja redefinida corretamente
    }
    
    #endregion

    /// <summary>
    /// Controla dinamicamente a camada do GameObject para permitir ou impedir o grapple.
    /// </summary>
    private void UpdateGrappleableLayer()
    {
        // A camada é definida com base na checkbox e no estado de lentidão.
        // O método CanBeGrappledNow() será o ponto de verificação para o GrapplingHookController.
        if (CanBeGrappledNow())
        {
            gameObject.layer = _grappleableLayerIndex;
        }
        else
        {
            gameObject.layer = _nonGrappleableLayerIndex;
        }
    }

    #region IGrappleable Implementation
    public bool CanBeGrappledNow()
    {
        // A plataforma é agarrável se:
        // 1. A checkbox permite agarrar enquanto ela se move, OU
        // 2. O tempo está lento.
        return canBeGrappledWhileMoving || _isSlowed;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (targetA != null && targetB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(targetA.position, targetB.position);
            Gizmos.DrawWireSphere(targetA.position, 0.5f);
            Gizmos.DrawWireSphere(targetB.position, 0.5f);
        }
    }
}