using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections;

public class HoldGlobalVolumeWithMaxTime : MonoBehaviour
{
	[Header("Assign the Global Volumes")]
	public Volume volumeA;
	public Volume volumeB;

	[Header("Input Action Reference (from Input System)")]
	public InputActionReference toggleAction;

	[Header("Max Hold Time (seconds)")]
	[Tooltip("Maximum time the button can be held before automatically returning to Volume A.")]
	public float maxHoldTime = 3f;

	private Coroutine holdTimerCoroutine;

	private void OnEnable()
	{
		if (toggleAction != null)
		{
			toggleAction.action.performed += OnPress;
			toggleAction.action.canceled += OnRelease;
			toggleAction.action.Enable();
		}
	}

	private void OnDisable()
	{
		if (toggleAction != null)
		{
			toggleAction.action.performed -= OnPress;
			toggleAction.action.canceled -= OnRelease;
			toggleAction.action.Disable();
		}
	}

	private void Start()
	{
		if (volumeA == null || volumeB == null)
		{
			Debug.LogWarning("Please assign both Volume A and Volume B in the inspector.");
			return;
		}

		volumeA.enabled = true;
		volumeB.enabled = false;
	}

	private void OnPress(InputAction.CallbackContext ctx)
	{
		// Activate B, disable A
		volumeA.enabled = false;
		volumeB.enabled = true;

		// Start max-hold timer
		if (holdTimerCoroutine != null)
			StopCoroutine(holdTimerCoroutine);
		holdTimerCoroutine = StartCoroutine(HoldTimer());
	}

	private void OnRelease(InputAction.CallbackContext ctx)
	{
		ReturnToA();
	}

	private IEnumerator HoldTimer()
	{
		yield return new WaitForSeconds(maxHoldTime);
		ReturnToA();
	}

	private void ReturnToA()
	{
		if (holdTimerCoroutine != null)
		{
			StopCoroutine(holdTimerCoroutine);
			holdTimerCoroutine = null;
		}

		volumeA.enabled = true;
		volumeB.enabled = false;
	}
}
