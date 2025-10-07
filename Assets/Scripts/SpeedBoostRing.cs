// Local: Assets/Scripts/PowerUps/SpeedBoostRing.cs

using UnityEngine;

public class SpeedBoostRing : BasePowerUpRing
{
    public enum BoostMode
    {
        [Tooltip("Multiplica a velocidade atual do jogador, mantendo sua direção.")]
        MultiplyVelocity,
        [Tooltip("Aplica um impulso na direção que o jogador está olhando.")]
        PlayerForwardDirection,
        [Tooltip("Aplica um impulso em uma direção fixa, definida neste componente.")]
        FixedDirection
    }

    [Header("Configurações do Impulso")]
    [SerializeField] private BoostMode boostMode = BoostMode.PlayerForwardDirection;

    [Tooltip("A força do impulso para os modos 'PlayerForwardDirection' e 'FixedDirection'.")]
    [SerializeField] private float boostForce = 50f;

    [Tooltip("O multiplicador de velocidade para o modo 'MultiplyVelocity'.")]
    [SerializeField] private float velocityMultiplier = 1.5f;

    [Tooltip("Define a direção fixa para o modo 'FixedDirection'.")]
    [SerializeField] private Vector3 fixedDirection = Vector3.forward;

    [Tooltip("Se verdadeiro, a velocidade do jogador será zerada antes de aplicar o impulso (não se aplica ao modo MultiplyVelocity).")]
    [SerializeField] private bool resetMomentum = false;

    protected override void ApplyEffect(GameObject playerObject)
    {
        var playerMovement = playerObject.GetComponent<PlayerMovementController>();
        if (playerMovement == null) return;

        switch (boostMode)
        {
            case BoostMode.MultiplyVelocity:
                playerMovement.MultiplyVelocity(velocityMultiplier);
                break;

            case BoostMode.PlayerForwardDirection:
                Vector3 playerForward = playerMovement.GetOrientation().forward;
                playerMovement.ApplyExternalForce(playerForward, boostForce, resetMomentum);
                break;

            case BoostMode.FixedDirection:
                playerMovement.ApplyExternalForce(fixedDirection.normalized, boostForce, resetMomentum);
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (boostMode == BoostMode.FixedDirection)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, fixedDirection.normalized * 5f);
            Gizmos.DrawWireSphere(transform.position + fixedDirection.normalized * 5f, 0.5f);
        }
    }
}