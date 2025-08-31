// Local: Assets/Scripts/TimeManipulation/TimeSlowerController.cs

using UnityEngine;

/// <summary>
/// Gerencia a habilidade do jogador de desacelerar objetos específicos.
/// Ouve os eventos do InputManager e usa um Raycast para encontrar alvos.
/// </summary>
public class TimeSlowerController : MonoBehaviour
{
    [Header("Configurações da Habilidade")]
    [Tooltip("A distância máxima que o jogador pode usar a habilidade.")]
    [SerializeField] private float maxDistance = 100f;
    [Tooltip("A intensidade da desaceleração. Para Rigidbodies, este valor é somado ao 'drag'.")]
    [SerializeField] private float slowFactor = 10f;
    [Tooltip("A camada de objetos que podem ser desacelerados.")]
    [SerializeField] private LayerMask slowableLayer;

    [Header("Referências")]
    [Tooltip("A câmera do jogador para determinar a direção do Raycast.")]
    [SerializeField] private Transform cameraTransform;
    
    private ITimeSlowable _currentTarget;

    private void OnEnable()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager não encontrado na cena!");
            return;
        }
        InputManager.Instance.OnSlowTimeStarted += HandleSlowTimeStarted;
        InputManager.Instance.OnSlowTimeCanceled += HandleSlowTimeCanceled;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnSlowTimeStarted -= HandleSlowTimeStarted;
        InputManager.Instance.OnSlowTimeCanceled -= HandleSlowTimeCanceled;

        // Garante que, se o script for desativado, o alvo atual volte ao normal.
        HandleSlowTimeCanceled();
    }

    /// <summary>
    /// Chamado quando o input de desacelerar é pressionado.
    /// Procura por um alvo e, se encontrado, o desacelera.
    /// </summary>
    private void HandleSlowTimeStarted()
    {
        // Se já houver um alvo, restaura-o primeiro antes de procurar um novo.
        if (_currentTarget != null)
        {
            _currentTarget.RestoreNormalTime();
            _currentTarget = null;
        }

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxDistance, slowableLayer))
        {
            // Tenta obter o componente que implementa a interface ITimeSlowable.
            ITimeSlowable slowableObject = hit.collider.GetComponent<ITimeSlowable>();

            if (slowableObject != null)
            {
                _currentTarget = slowableObject;
                _currentTarget.SlowDown(slowFactor);
            }
        }
    }

    /// <summary>
    /// Chamado quando o input de desacelerar é solto.
    /// Restaura o tempo do alvo atual, se houver um.
    /// </summary>
    private void HandleSlowTimeCanceled()
    {
        if (_currentTarget != null)
        {
            _currentTarget.RestoreNormalTime();
            _currentTarget = null;
        }
    }
}