using UnityEngine;

public class DoubleJumpResetRingCaio : BasePowerUpRing
{
    protected override void ApplyEffect(GameObject playerObject)
    {
        var playerMovement = playerObject.GetComponent<PlayerMovimentCaio>();
        if (playerMovement != null)
        {
            playerMovement.EnableDoubleJump();
        }
    }
}