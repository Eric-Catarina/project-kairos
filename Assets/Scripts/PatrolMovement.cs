// Local: Assets/Scripts/PatrolMovement.cs

using UnityEngine;

/// <summary>
/// Move um objeto com Rigidbody entre dois pontos de destino de forma contínua.
/// Projetado para testar a mecânica de manipulação do tempo.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PatrolMovement : MonoBehaviour
{
    [Header("Configurações de Patrulha")]
    [Tooltip("O ponto de partida da patrulha.")]
    [SerializeField] private Transform targetA;
    [Tooltip("O ponto final da patrulha.")]
    [SerializeField] private Transform targetB;
    [Tooltip("A velocidade de movimento do objeto.")]
    [SerializeField] private float speed = 5f;

    [Header("Precisão")]
    [Tooltip("A distância mínima do alvo para considerá-lo alcançado.")]
    [SerializeField] private float reachThreshold = 0.5f;

    private Rigidbody _rb;
    private Transform _currentTarget;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Garante que a gravidade não interfira na patrulha horizontal/vertical.
        _rb.useGravity = false; 
    }

    private void Start()
    {
        // Valida se os alvos foram configurados para evitar erros.
        if (targetA == null || targetB == null)
        {
            Debug.LogError("Os alvos de patrulha (Target A e Target B) não foram configurados!", this);
            enabled = false; // Desativa o script se não houver alvos.
            return;
        }

        // Define o primeiro alvo e inicia o movimento.
        _currentTarget = targetB;
    }

    private void FixedUpdate()
    {
        // Ponto de integração CRÍTICO:
        // Se o Rigidbody for cinemático, significa que o SlowableRigidbody está no controle.
        // Portanto, este script não deve interferir no movimento para evitar conflitos.
        if (_rb.isKinematic)
        {
            return;
        }

        // Calcula a direção para o alvo atual.
        Vector3 direction = (_currentTarget.position - _rb.position).normalized;

        // Aplica a velocidade na direção do alvo.
        _rb.linearVelocity = direction * speed;

        // Verifica se o objeto chegou perto o suficiente do alvo.
        if (Vector3.Distance(_rb.position, _currentTarget.position) < reachThreshold)
        {
            // Troca o alvo para o outro ponto.
            _currentTarget = (_currentTarget == targetA) ? targetB : targetA;
        }
    }

    /// <summary>
    /// Desenha linhas na Scene View para facilitar a visualização dos pontos de patrulha.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (targetA != null && targetB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(targetA.position, targetB.position);
            Gizmos.DrawWireSphere(targetA.position, reachThreshold);
            Gizmos.DrawWireSphere(targetB.position, reachThreshold);
        }
    }
}