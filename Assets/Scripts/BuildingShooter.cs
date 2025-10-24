using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RandomPrefabShooter : MonoBehaviour
{
	[Header("Prefab Settings")]
	[Tooltip("Assign up to 5 prefabs to shoot randomly.")]
	public GameObject[] prefabs = new GameObject[5];

	[Header("Shooting Settings")]
	[Tooltip("Time interval between each shot (seconds).")]
	public float shootInterval = 2f;

	[Tooltip("Force applied to shoot the prefab forward.")]
	public float shootForce = 10f;

	[Header("Lifetime Settings")]
	[Tooltip("Time before each spawned prefab is destroyed (seconds).")]
	public float prefabLifetime = 5f;

	[Header("Input Settings")]
	[Tooltip("Input Action Reference for the button (e.g. 'Fire' or 'Jump').")]
	public InputActionReference holdAction;

	[Header("Stamina Settings")]
	[Tooltip("Maximum stamina duration (seconds).")]
	public float maxStamina = 3f;

	[Tooltip("Regeneration rate in seconds per second.")]
	public float regenRate = 1f;

	private float timer;
	private static float sharedStamina;
	private static bool isButtonPressed;

	private void Awake()
	{
		// Subscribe input only once to avoid duplicates
		if (holdAction != null && !inputSubscribed)
		{
			holdAction.action.performed += ctx => isButtonPressed = true;
			holdAction.action.canceled += ctx => isButtonPressed = false;
			inputSubscribed = true;
		}

		// Initialize stamina once on startup or scene reload
		if (!staminaInitialized)
		{
			sharedStamina = maxStamina;
			staminaInitialized = true;
		}
	}

	private void OnEnable()
	{
		holdAction?.action.Enable();
		SceneManager.sceneLoaded += OnSceneReload;
	}

	private void OnDisable()
	{
		holdAction?.action.Disable();
		SceneManager.sceneLoaded -= OnSceneReload;
	}

	private void OnSceneReload(Scene scene, LoadSceneMode mode)
	{
		sharedStamina = maxStamina;
	}

	private void Update()
	{
		// --- Handle shared stamina ---
		if (isButtonPressed)
		{
			sharedStamina -= Time.deltaTime;
			if (sharedStamina < 0f)
				sharedStamina = 0f;
		}
		else
		{
			sharedStamina += regenRate * Time.deltaTime;
			if (sharedStamina > maxStamina)
				sharedStamina = maxStamina;
		}

		// --- Only shoot when button is NOT pressed and stamina > 0 ---
		if (!isButtonPressed && sharedStamina > 0f)
		{
			timer += Time.deltaTime;
			if (timer >= shootInterval)
			{
				ShootRandomPrefab();
				timer = 0f;
			}
		}
	}

	private void ShootRandomPrefab()
	{
		// Keep your existing logic intact
		GameObject[] validPrefabs = System.Array.FindAll(prefabs, p => p != null);
		if (validPrefabs.Length == 0) return;

		GameObject chosenPrefab = validPrefabs[Random.Range(0, validPrefabs.Length)];
		GameObject spawned = Instantiate(chosenPrefab, transform.position, transform.rotation);

		Rigidbody rb = spawned.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.linearVelocity = Vector3.zero;
			rb.AddForce(transform.forward * shootForce, ForceMode.VelocityChange);
		}

		Destroy(spawned, prefabLifetime);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawRay(transform.position, transform.forward * 2f);
	}

	// --- Static helpers ---
	private static bool staminaInitialized = false;
	private static bool inputSubscribed = false;
}
