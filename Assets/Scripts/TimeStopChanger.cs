using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class HoldGlobalVolumeStaminaSafe : MonoBehaviour
{
	[Header("Assign the Global Volumes")]
	public Volume volumeA;
	public Volume volumeB;

	[Header("Input Action Reference (from Input System)")]
	public InputActionReference holdAction;

	[Header("Stamina Settings")]
	public float maxHoldTime = 3f;
	public float rechargeRate = 1.5f;

	private float currentStamina;
	private bool isHolding = false;

	private void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		FindVolumes(force: true);
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Always re-find volumes after scene reload
		FindVolumes(force: true);
		ResetStaminaAndVolumes();
		RebindInputActions();
	}

	private void OnEnable()
	{
		RebindInputActions();
	}

	private void OnDisable()
	{
		UnbindInputActions();
	}

	private void Start()
	{
		ResetStaminaAndVolumes();
	}

	private void Update()
	{
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
			currentStamina += Time.deltaTime * rechargeRate;
			currentStamina = Mathf.Min(currentStamina, maxHoldTime);
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
		if (volumeA == null || volumeB == null) return;

		isHolding = true;
		SafeSetVolumeState(volumeA, false);
		SafeSetVolumeState(volumeB, true);
	}

	private void StopHolding()
	{
		if (volumeA == null || volumeB == null) return;

		isHolding = false;
		SafeSetVolumeState(volumeA, true);
		SafeSetVolumeState(volumeB, false);
	}

	private void ResetStaminaAndVolumes()
	{
		currentStamina = maxHoldTime;
		isHolding = false;
		SafeSetVolumeState(volumeA, true);
		SafeSetVolumeState(volumeB, false);
	}

	private void FindVolumes(bool force = false)
	{
		// Always search and assign volumes after scene reload
		if (force || volumeA == null || volumeB == null)
		{
			Volume[] allVolumes = GameObject.FindObjectsOfType<Volume>();
			volumeA = allVolumes.Length > 0 ? allVolumes[0] : null;
			volumeB = allVolumes.Length > 1 ? allVolumes[1] : null;

			if (volumeA == null || volumeB == null)
				Debug.LogWarning($"{name}: Could not find both volumes in scene.");
		}
	}

	private void SafeSetVolumeState(Volume vol, bool state)
	{
		if (vol == null) return;
		vol.enabled = state;
	}

	private void RebindInputActions()
	{
		if (holdAction?.action != null)
		{
			holdAction.action.performed -= OnPress;
			holdAction.action.canceled -= OnRelease;
			holdAction.action.performed += OnPress;
			holdAction.action.canceled += OnRelease;
			holdAction.action.Enable();
		}
	}

	private void UnbindInputActions()
	{
		if (holdAction?.action != null)
		{
			holdAction.action.performed -= OnPress;
			holdAction.action.canceled -= OnRelease;
			holdAction.action.Disable();
		}
	}

	public float GetStaminaNormalized()
	{
		return Mathf.Clamp01(currentStamina / maxHoldTime);
	}
}
