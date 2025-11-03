using UnityEngine;

public class GrappleResetRing : BasePowerUpRing
{
    protected override void ApplyEffect(GameObject playerObject)
    {
        var grappleController = playerObject.GetComponent<GrapplingHookController>();
        if (grappleController != null)
        {
            grappleController.ResetGrapple();
        }
    }
}