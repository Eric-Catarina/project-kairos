using UnityEngine;

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

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= shootInterval)
        {
            ShootRandomPrefab();
            timer = 0f;
        }
    }

    private void ShootRandomPrefab()
    {
        // Filter out null prefabs to avoid errors
        GameObject[] validPrefabs = System.Array.FindAll(prefabs, p => p != null);
        if (validPrefabs.Length == 0) return;

        // Pick a random prefab
        GameObject chosenPrefab = validPrefabs[Random.Range(0, validPrefabs.Length)];

        // Spawn it at this object's position and rotation
        GameObject spawned = Instantiate(chosenPrefab, transform.position, transform.rotation);

        // Apply a forward force if it has a Rigidbody
        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // reset any existing velocity
            rb.AddForce(transform.forward * shootForce, ForceMode.VelocityChange);
        }

        // Schedule destruction
        Destroy(spawned, prefabLifetime);
    }

    private void OnDrawGizmosSelected()
    {
        // Optional: visualize the shooting direction in the scene view
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}
