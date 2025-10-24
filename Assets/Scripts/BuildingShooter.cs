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
	[Tooltip("Maximum button hold duration (seconds).")]
	public float maxHoldTime = 3f;

	[Tooltip("Regeneration rate per second when button is not held.")]
	public float regenRate = 1f;

	private float holdTimer = 0f;
	private float shootTimer = 0f;
	private static bool isButtonPressed;
	private static bool inputSubscribed = false;

	private void Awake()
	{
		// Subscribe input only once
		if (holdAction != null && !inputSubscribed)
		{
			holdAction.action.performed += ctx =>
			{
				if (holdTimer > 0f)
					isButtonPressed = true;
			};
			holdAction.action.canceled += ctx => isButtonPressed = false;
			inputSubscribed = true;
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
		holdTimer = maxHoldTime;
		shootTimer = 0f;
		isButtonPressed = false;
	}

	private void Update()
	{
		if (isButtonPressed)
		{
			// Decrease hold timer
			holdTimer -= Time.deltaTime;

			// Auto-release when max hold time used up
			if (holdTimer <= 0f)
			{
				holdTimer = 0f;
				isButtonPressed = false;
			}
		}
		else
		{
			// Regenerate hold timer when button not pressed
			holdTimer += regenRate * Time.deltaTime;
			if (holdTimer > maxHoldTime)
				holdTimer = maxHoldTime;

			// Shooting logic
			shootTimer += Time.deltaTime;
			if (shootTimer >= shootInterval)
			{
				ShootRandomPrefab();
				shootTimer = 0f;
			}
		}
	}

	private void ShootRandomPrefab()
	{
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
}
