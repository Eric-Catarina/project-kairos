using UnityEngine;

public class DoubleJumpResetRing : BasePowerUpRing
{
    protected override void ApplyEffect(GameObject playerObject)
    {
        var playerMovement = playerObject.GetComponent<PlayerMovementController>();
        if (playerMovement != null)
        {
            playerMovement.EnableDoubleJump();
        }
    }
}