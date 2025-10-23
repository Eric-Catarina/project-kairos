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
		FindVolumesIfMissing();
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Re-find volumes and reset everything
		FindVolumesIfMissing();
		ResetStaminaAndVolumes();
	}

	private void OnEnable()
	{
		if (holdAction?.action != null)
		{
			holdAction.action.performed += OnPress;
			holdAction.action.canceled += OnRelease;
			holdAction.action.Enable();
		}
	}

	private void OnDisable()
	{
		if (holdAction?.action != null)
		{
			holdAction.action.performed -= OnPress;
			holdAction.action.canceled -= OnRelease;
			holdAction.action.Disable();
		}
	}

	private void Start()
	{
		// Ensure volumes are active and stamina is full at start
		ResetStaminaAndVolumes();
	}

	private void Update()
	{
		if (volumeA == null || volumeB == null)
			FindVolumesIfMissing();

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

	private void FindVolumesIfMissing()
	{
		if (volumeA == null)
			volumeA = GameObject.FindObjectOfType<Volume>(); // finds the first in scene

		if (volumeB == null)
		{
			Volume[] allVolumes = GameObject.FindObjectsOfType<Volume>();
			foreach (var vol in allVolumes)
			{
				if (vol != volumeA)
				{
					volumeB = vol;
					break;
				}
			}
		}

		if (volumeA == null || volumeB == null)
			Debug.LogWarning($"{name}: Could not find both volumes in scene.");
	}

	private void SafeSetVolumeState(Volume vol, bool state)
	{
		if (vol == null) return;
		if (vol.Equals(null)) return;
		vol.enabled = state;
	}

	public float GetStaminaNormalized()
	{
		return Mathf.Clamp01(currentStamina / maxHoldTime);
	}
}
