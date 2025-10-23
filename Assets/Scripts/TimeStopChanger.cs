using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections;

public class HoldGlobalVolumeStamina : MonoBehaviour
{
	[Header("Assign the Global Volumes")]
	public Volume volumeA;
	public Volume volumeB;

	[Header("Input Action Reference (from Input System)")]
	public InputActionReference holdAction;

	[Header("Stamina Settings")]
	[Tooltip("Maximum active time (in seconds) Volume B can stay on with full stamina.")]
	public float maxHoldTime = 3f;

	[Tooltip("Recharge rate (seconds of stamina recovered per second).")]
	public float rechargeRate = 1.5f;

	private float currentStamina;
	private bool isHolding = false;

	private void OnEnable()
	{
		if (holdAction != null)
		{
			holdAction.action.performed += OnPress;
			holdAction.action.canceled += OnRelease;
			holdAction.action.Enable();
		}
	}

	private void OnDisable()
	{
		if (holdAction != null)
		{
			holdAction.action.performed -= OnPress;
			holdAction.action.canceled -= OnRelease;
			holdAction.action.Disable();
		}
	}

	private void Start()
	{
		currentStamina = maxHoldTime;

		if (volumeA == null || volumeB == null)
		{
			Debug.LogWarning("Please assign both Volume A and Volume B in the inspector.");
			return;
		}

		volumeA.enabled = true;
		volumeB.enabled = false;
	}

	private void Update()
	{
		// Handle stamina drain and recharge
		if (isHolding)
		{
			currentStamina -= Time.deltaTime;
			if (currentStamina <= 0f)
			{
				currentStamina = 0f;
				StopHolding();
			}
		}
		else
		{
			if (currentStamina < maxHoldTime)
			{
				currentStamina += Time.deltaTime * rechargeRate;
				if (currentStamina > maxHoldTime)
					currentStamina = maxHoldTime;
			}
		}
	}

	private void OnPress(InputAction.CallbackContext ctx)
	{
		if (currentStamina > 0f)
			StartHolding();
	}

	private void OnRelease(InputAction.CallbackContext ctx)
	{
		StopHolding();
	}

	private void StartHolding()
	{
		isHolding = true;
		volumeA.enabled = false;
		volumeB.enabled = true;
	}

	private void StopHolding()
	{
		isHolding = false;
		volumeA.enabled = true;
		volumeB.enabled = false;
	}

	/// <summary>
	/// Returns current stamina percentage (0–1) for UI bars etc.
	/// </summary>
	public float GetStaminaNormalized()
	{
		return currentStamina / maxHoldTime;
	}
}
