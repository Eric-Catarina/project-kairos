// Local: Assets/Scripts/Grappling/PullObjectStrategy.cs

using UnityEngine;

/// <summary>
/// Estrat�gia que implementa o comportamento de puxar um objeto em dire��o ao jogador.
/// Aplica uma for�a cont�nua ao Rigidbody do objeto alvo.
/// </summary>
public class PullObjectStrategy : IGrappleStrategy
{
    private Rigidbody _targetRigidbody;
    private float _pullForce;
    private float _stopDistance;

    public PullObjectStrategy(Rigidbody target, float force, float distance)
    {
        _targetRigidbody = target;
        _pullForce = force;
        _stopDistance = distance;
    }

    public void Execute(GrapplingHookController context)
    {
        // A l�gica principal acontece no FixedUpdate, aqui apenas garantimos que temos a refer�ncia.
        if (_targetRigidbody == null)
        {
            Debug.LogError("PullObjectStrategy iniciada sem um Rigidbody alvo.");
            context.StopGrapple();
        }
    }

    public void Stop(GrapplingHookController context)
    {
        // Zera a velocidade do objeto ao soltar para um feedback mais imediato.
        if (_targetRigidbody != null)
        {
            _targetRigidbody.linearVelocity = Vector3.zero;
        }
    }

    public void FixedUpdate(GrapplingHookController context)
    {
        if (_targetRigidbody == null) return;

        // Calcula a dire��o do objeto para o jogador
        Vector3 directionToPlayer = (context.transform.position - _targetRigidbody.position).normalized;
        float distance = Vector3.Distance(context.transform.position, _targetRigidbody.position);

        // S� aplica a for�a se o objeto estiver mais longe que a dist�ncia de parada
        if (distance > _stopDistance)
        {
            _targetRigidbody.AddForce(directionToPlayer * _pullForce, ForceMode.Force);
        }
        else
        {
            // Opcional: freia o objeto quando chega perto
            _targetRigidbody.linearVelocity *= 0.9f;
        }

        // Atualiza o ponto do gancho para a posi��o atual do objeto
        context.SetGrapplePoint(_targetRigidbody.position);
    }

    public void LateUpdate(GrapplingHookController context)
    {
        // Apenas desenha a corda
        context.DrawRope();
    }
}