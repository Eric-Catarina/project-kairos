using UnityEngine;

public class GrappleResetRingCaio : BasePowerUpRing
{
    protected override void ApplyEffect(GameObject playerObject)
    {
        var grappleController = playerObject.GetComponent<GrapplingHookCaio>();
        if (grappleController != null)
        {
            grappleController.ResetGrapple();
        }
    }
}