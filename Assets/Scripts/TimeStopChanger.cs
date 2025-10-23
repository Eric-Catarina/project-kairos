using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

<<<<<<< HEAD
public class HoldGlobalVolumeStaminaSafe : MonoBehaviour
=======
public class HoldGlobalVolumeWithMaxTime : MonoBehaviour
>>>>>>> parent of defd300 (everything working)
{
	[Header("Assign the Global Volumes")]
	public Volume volumeA;
	public Volume volumeB;

	[Header("Input Action Reference (from Input System)")]
	public InputActionReference toggleAction;

<<<<<<< HEAD
	[Header("Stamina Settings")]
	public float maxHoldTime = 3f;
	public float rechargeRate = 1.5f;

	private float currentStamina;
	private bool isHolding = false;
=======
	[Header("Max Hold Time (seconds)")]
	[Tooltip("Maximum time the button can be held before automatically returning to Volume A.")]
	public float maxHoldTime = 3f;

	private Coroutine holdTimerCoroutine;
>>>>>>> parent of defd300 (everything working)

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
<<<<<<< HEAD
		if (holdAction?.action != null)
=======
		if (toggleAction != null)
>>>>>>> parent of defd300 (everything working)
		{
			toggleAction.action.performed += OnPress;
			toggleAction.action.canceled += OnRelease;
			toggleAction.action.Enable();
		}
	}

	private void OnDisable()
	{
<<<<<<< HEAD
		if (holdAction?.action != null)
=======
		if (toggleAction != null)
>>>>>>> parent of defd300 (everything working)
		{
			toggleAction.action.performed -= OnPress;
			toggleAction.action.canceled -= OnRelease;
			toggleAction.action.Disable();
		}
	}

	private void Start()
	{
<<<<<<< HEAD
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

=======
		if (volumeA == null || volumeB == null)
		{
			Debug.LogWarning("Please assign both Volume A and Volume B in the inspector.");
			return;
		}

		volumeA.enabled = true;
		volumeB.enabled = false;
	}

>>>>>>> parent of defd300 (everything working)
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
<<<<<<< HEAD
		if (volumeA == null || volumeB == null) return;

		isHolding = true;
		SafeSetVolumeState(volumeA, false);
		SafeSetVolumeState(volumeB, true);
=======
		yield return new WaitForSeconds(maxHoldTime);
		ReturnToA();
>>>>>>> parent of defd300 (everything working)
	}

	private void ReturnToA()
	{
<<<<<<< HEAD
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
=======
		if (holdTimerCoroutine != null)
		{
			StopCoroutine(holdTimerCoroutine);
			holdTimerCoroutine = null;
		}

		volumeA.enabled = true;
		volumeB.enabled = false;
	}
>>>>>>> parent of defd300 (everything working)
}
