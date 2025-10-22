using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class ToggleGlobalVolumeInputSystem : MonoBehaviour
{
	[Header("Assign the Global Volumes")]
	public Volume volumeA;
	public Volume volumeB;

	[Header("Input Action Reference (from Input System)")]
	public InputActionReference toggleAction;

	private bool isVolumeAActive = true;

	private void OnEnable()
	{
		if (toggleAction != null)
		{
			toggleAction.action.performed += OnToggle;
			toggleAction.action.Enable();
		}
	}

	private void OnDisable()
	{
		if (toggleAction != null)
		{
			toggleAction.action.performed -= OnToggle;
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

	private void OnToggle(InputAction.CallbackContext ctx)
	{
		isVolumeAActive = !isVolumeAActive;

		volumeA.enabled = isVolumeAActive;
		volumeB.enabled = !isVolumeAActive;
	}
}
