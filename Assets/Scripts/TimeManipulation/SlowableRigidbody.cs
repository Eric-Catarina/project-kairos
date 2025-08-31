// Local: Assets/Scripts/TimeManipulation/SlowableRigidbody.cs

using UnityEngine;

/// <summary>
/// Implementação concreta da interface ITimeSlowable para objetos com Rigidbody.
/// Salva o estado de movimento (velocidades), torna o Rigidbody cinemático durante a lentidão
/// e simula o movimento em 'FixedUpdate' com base na porcentagem de lentidão.
/// Ao restaurar, reaplica o estado de movimento salvo.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SlowableRigidbody : MonoBehaviour, ITimeSlowable
{
    [Header("Visual Feedback")]
    private Renderer objectRenderer;
    private Color slowDownColor = Color.cyan;

    private Rigidbody _rb;
    private Color _originalColor;

    // Variáveis para salvar o estado do Rigidbody
    private Vector3 _savedVelocity;
    private Vector3 _savedAngularVelocity;

    private bool _isSlowed = false;
    private float _slowFactor; // Armazena o multiplicador (ex: 0.5 para 50% de lentidão)

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null && objectRenderer.material != null)
        {
            _originalColor = objectRenderer.material.color;
        }
    }

    private void OnEnable()
    {
        // Se registra no manager para receber os eventos de tempo.
        TimeManipulationManager.Instance?.Register(this);
    }

    private void OnDisable()
    {
        // Garante que o tempo seja restaurado se o objeto for desativado.
        if (_isSlowed)
        {
            RestoreNormalTime();
        }
        // Remove o registro do manager para evitar referências nulas.
        TimeManipulationManager.Instance?.Unregister(this);
    }

    private void FixedUpdate()
    {
        // Se o tempo não está lento, a física normal do Unity cuida de tudo.
        if (!_isSlowed) return;

        // Se o slow for 100%, não fazemos nada, o objeto fica parado.
        if (_slowFactor <= 0f) return;

        // Como o Rigidbody está cinemático, precisamos simular seu movimento.
        // Usamos MovePosition e MoveRotation para respeitar a física e colisões.
        Vector3 newPosition = _rb.position + (_savedVelocity * _slowFactor * Time.fixedDeltaTime);
        _rb.MovePosition(newPosition);

        Quaternion deltaRotation = Quaternion.Euler(_savedAngularVelocity * _slowFactor * Time.fixedDeltaTime);
        _rb.MoveRotation(_rb.rotation * deltaRotation);
    }

    /// <summary>
    /// Salva o estado de movimento, torna o Rigidbody cinemático e aplica o feedback visual.
    /// </summary>
    public void SlowDown(float slowPercentage)
    {
        if (_isSlowed) return;
        _isSlowed = true;

        // Salva o estado atual
        _savedVelocity = _rb.linearVelocity;
        _savedAngularVelocity = _rb.angularVelocity;

        // Converte a porcentagem (0-100) para um multiplicador (1.0-0.0)
        _slowFactor = 1.0f - (slowPercentage / 100.0f);

        // Torna o Rigidbody cinemático para que possamos controlar seu movimento manualmente.
        _rb.isKinematic = true;

        // Feedback visual
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = slowDownColor;
        }
    }

    /// <summary>
    /// Retorna o Rigidbody ao estado dinâmico, restaura suas velocidades e o feedback visual.
    /// </summary>
    public void RestoreNormalTime()
    {
        if (!_isSlowed) return;
        _isSlowed = false;

        // Retorna o Rigidbody ao controle total da física.
        _rb.isKinematic = false;

        // Restaura o estado de movimento. A física do Unity continuará a partir daqui.
        _rb.linearVelocity = _savedVelocity;
        _rb.angularVelocity = _savedAngularVelocity;

        // Restaura feedback visual
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = _originalColor;
        }
    }
    public void SetSlowDownColor(Color newColor)
    {
        slowDownColor = newColor;
    }


}