// Local: Assets/Scripts/Grappling/PullableObject.cs

using UnityEngine;

/// <summary>
/// Componente para ser adicionado em objetos que podem ser puxados pelo Grappling Hook.
/// Requer um Rigidbody no objeto.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PullableObject : MonoBehaviour
{
    [Header("Configurações de Puxão")]
    [Tooltip("A força com que o objeto será puxado em direção ao jogador.")]
    [SerializeField] private float pullForce = 50f;

    [Tooltip("A que distância do jogador o objeto deve parar de ser puxado.")]
    [SerializeField] private float stopDistance = 3f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Método de fábrica que cria e retorna a estratégia de puxar para este objeto.
    /// </summary>
    /// <returns>Uma nova instância da PullObjectStrategy configurada para este objeto.</returns>
    public IGrappleStrategy CreateStrategy()
    {
        return new PullObjectStrategy(_rb, pullForce, stopDistance);
    }
}