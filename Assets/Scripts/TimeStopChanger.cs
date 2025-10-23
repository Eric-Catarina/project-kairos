using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.Collections;

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

	[Header("Transition Settings")]
	public float transitionDuration = 0.5f;

	private float currentStamina;
	private bool isHolding = false;
	private Coroutine transitionCoroutine;

	private const string VolumeAKey = "HoldGlobalVolumeStaminaSafe_VolumeA";
	private const string VolumeBKey = "HoldGlobalVolumeStaminaSafe_VolumeB";

	private void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void Start()
	{
		RestoreVolumes();
		RebindInputActions();
		ResetStaminaAndVolumes();
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		RestoreVolumes();
		RebindInputActions();
		ResetStaminaAndVolumes();
	}

	private void OnEnable()
	{
		RebindInputActions();
	}

	private void OnDisable()
	{
		UnbindInputActions();
		SaveVolumes();
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
		StartTransition(volumeA, volumeB);
	}

	private void StopHolding()
	{
		if (volumeA == null || volumeB == null) return;

		isHolding = false;
		StartTransition(volumeB, volumeA);
	}

	private void ResetStaminaAndVolumes()
	{
		currentStamina = maxHoldTime;
		isHolding = false;
		SetVolumesInstant(volumeA, 1f, true);
		SetVolumesInstant(volumeB, 0f, false);
	}

	private void RestoreVolumes()
	{
		// Try to restore by name
		string volumeAName = PlayerPrefs.GetString(VolumeAKey, "");
		string volumeBName = PlayerPrefs.GetString(VolumeBKey, "");

		Volume[] allVolumes = GameObject.FindObjectsOfType<Volume>(true);

		if (!string.IsNullOrEmpty(volumeAName))
			volumeA = System.Array.Find(allVolumes, v => v.name == volumeAName);

		if (!string.IsNullOrEmpty(volumeBName))
			volumeB = System.Array.Find(allVolumes, v => v.name == volumeBName);

		// Fallback if not found
		if (volumeA == null || volumeB == null)
		{
			volumeA = allVolumes.Length > 0 ? allVolumes[0] : null;
			volumeB = allVolumes.Length > 1 ? allVolumes[1] : null;
		}

		if (volumeA == null || volumeB == null)
			Debug.LogWarning($"{name}: Could not find both volumes in scene.");
	}

	private void SaveVolumes()
	{
		if (volumeA != null)
			PlayerPrefs.SetString(VolumeAKey, volumeA.name);
		if (volumeB != null)
			PlayerPrefs.SetString(VolumeBKey, volumeB.name);
		PlayerPrefs.Save();
	}

	private void SetVolumesInstant(Volume active, float activeWeight, bool activeEnabled)
	{
		if (volumeA != null)
		{
			volumeA.weight = (active == volumeA) ? activeWeight : 0f;
			volumeA.enabled = (active == volumeA) ? activeEnabled : false;
		}
		if (volumeB != null)
		{
			volumeB.weight = (active == volumeB) ? activeWeight : 0f;
			volumeB.enabled = (active == volumeB) ? activeEnabled : false;
		}
	}

	private void StartTransition(Volume from, Volume to)
	{
		if (transitionCoroutine != null)
			StopCoroutine(transitionCoroutine);
		transitionCoroutine = StartCoroutine(TransitionVolumes(from, to, transitionDuration));
	}

	private IEnumerator TransitionVolumes(Volume from, Volume to, float duration)
	{
		if (from != null) from.enabled = true;
		if (to != null) to.enabled = true;

		float time = 0f;
		float fromStart = from != null ? from.weight : 0f;
		float toStart = to != null ? to.weight : 0f;

		while (time < duration)
		{
			time += Time.deltaTime;
			float t = Mathf.Clamp01(time / duration);

			if (from != null) from.weight = Mathf.Lerp(fromStart, 0f, t);
			if (to != null) to.weight = Mathf.Lerp(toStart, 1f, t);

			yield return null;
		}

		if (from != null)
		{
			from.weight = 0f;
			from.enabled = false;
		}
		if (to != null)
		{
			to.weight = 1f;
			to.enabled = true;
		}
		transitionCoroutine = null;
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
