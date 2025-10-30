using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class GrappleCooldownSlider : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Reference to the player's GrapplingHookController.")]
	[SerializeField] private GrapplingHookController grappleController;

	[Tooltip("UI Slider representing the Grapple cooldown.")]
	[SerializeField] private Slider cooldownSlider;

	[Header("Appearance")]
	[Tooltip("If true, the slider fills up during cooldown (instead of emptying).")]
	[SerializeField] private bool fillWhileCooling = true;

	private void Awake()
	{
		if (cooldownSlider == null)
			cooldownSlider = GetComponent<Slider>();

		if (grappleController == null)
			grappleController = FindFirstObjectByType<GrapplingHookController>();

		cooldownSlider.minValue = 0f;
		cooldownSlider.maxValue = 1f;
		cooldownSlider.value = 1f; // starts full (ready to use)
	}

	private void Update()
	{
		if (grappleController == null) return;

		// If currently grappling, keep the slider at 0 (hidden/empty)
		if (grappleController.IsGrappling)
		{
			cooldownSlider.value = 0f;
			return;
		}

		float totalCooldown = GetTotalCooldown();
		float remainingCooldown = GetRemainingCooldown();

		if (totalCooldown <= 0f)
		{
			cooldownSlider.value = 1f;
			return;
		}

		float normalized = Mathf.Clamp01(remainingCooldown / totalCooldown);
		cooldownSlider.value = fillWhileCooling ? (1f - normalized) : normalized;
	}

	private float GetTotalCooldown()
	{
		var field = typeof(GrapplingHookController)
			.GetField("grappleCooldown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		return field != null ? (float)field.GetValue(grappleController) : 1f;
	}

	private float GetRemainingCooldown()
	{
		var field = typeof(GrapplingHookController)
			.GetField("_cooldownTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		return field != null ? (float)field.GetValue(grappleController) : 0f;
	}
}
