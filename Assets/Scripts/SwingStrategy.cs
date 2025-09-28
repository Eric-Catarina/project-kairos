using UnityEngine;

/// <summary>
/// Estratégia que implementa o comportamento de balanço (swing) do jogador.
/// Cria um SpringJoint para conectar o jogador ao ponto de ancoragem.
/// </summary>
public class SwingStrategy : IGrappleStrategy
{
    private SpringJoint _joint;

    public void Execute(GrapplingHookController context)
    {
        // Cria e configura o SpringJoint para o balanço
        _joint = context.gameObject.AddComponent<SpringJoint>();
        _joint.autoConfigureConnectedAnchor = false;
        _joint.anchor = Vector3.zero;
        _joint.connectedAnchor = context.GetGrapplePoint();

        float distanceFromPoint = Vector3.Distance(context.transform.position, context.GetGrapplePoint());

        _joint.maxDistance = distanceFromPoint * context.maxSpringSize;
        _joint.minDistance = distanceFromPoint * context.minSpringSize;
        _joint.spring = context.springForce;
        _joint.damper = context.damper;
        _joint.massScale = context.massScale;
    }

    public void Stop(GrapplingHookController context)
    {
        // Destrói o joint quando o gancho é solto
        if (_joint != null)
        {
            Object.Destroy(_joint);
        }
    }

    public void FixedUpdate(GrapplingHookController context)
    {
        // Aplica a força de balanço baseada no input do jogador
        if (!_joint) return;

        Vector3 viewDirection = context.cameraTransform.forward;
        Vector3 rightDirection = context.cameraTransform.right;
        Vector2 moveInput = context.GetMoveInput();

        context.PlayerMovement.Rb.AddForce(viewDirection * moveInput.y * context.swingForce, ForceMode.Force);
        context.PlayerMovement.Rb.AddForce(rightDirection * moveInput.x * context.swingForce, ForceMode.Force);
    }

    public void LateUpdate(GrapplingHookController context)
    {
        // Atualiza a âncora do joint caso o ponto de gancho seja um objeto móvel
        if (context.GetGrappleAnchorRigidbody() != null)
        {
            Vector3 newAnchorPoint = context.GetGrappleAnchorRigidbody().transform.TransformPoint(context.GetGrapplePointRelativeOffset());
            context.SetGrapplePoint(newAnchorPoint);
            if (_joint) _joint.connectedAnchor = newAnchorPoint;
        }

        context.DrawRope();
    }
}