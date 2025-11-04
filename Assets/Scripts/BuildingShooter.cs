using System.Collections;
using System.Collections.Generic;
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

    [Header("Prewarm Settings")]
    [Tooltip("If true, the shooter starts as if it has already been firing for 'prefabLifetime' seconds.")]
    public bool enablePrewarm = true;

    private float shootTimer;
    private bool _isTimeStopped;

    // Nova estrutura para gerenciar objetos spawnados e seus tempos de vida restantes
    private struct SpawnedObject
    {
        public GameObject obj;
        public float remainingLifetime;
    }

    private List<SpawnedObject> spawnedObjects = new List<SpawnedObject>();

    private void Start()
    {
        if (enablePrewarm)
            PerformPrewarm();
    }

    private void Update()
    {
        _isTimeStopped = TimeManipulationManager.Instance != null && TimeManipulationManager.Instance.IsTimeSlowed;

        if (!_isTimeStopped)
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                ShootRandomPrefab();
                shootTimer = 0f;
            }

            // Gerenciar tempos de vida dos objetos spawnados
            for (int i = spawnedObjects.Count - 1; i >= 0; i--)
            {
                SpawnedObject so = spawnedObjects[i];
                so.remainingLifetime -= Time.deltaTime;
                if (so.remainingLifetime <= 0f)
                {
                    Destroy(so.obj);
                    spawnedObjects.RemoveAt(i);
                }
                else
                {
                    spawnedObjects[i] = so; // Atualizar na lista
                }
            }
        }
    }

    private void PerformPrewarm()
    {
        if (prefabs == null || prefabs.Length == 0)
            return;

        int steps = Mathf.FloorToInt(prefabLifetime / shootInterval);
        for (int i = 0; i < steps; i++)
        {
            float age = i * shootInterval; // seconds since it would have been fired
            float remainingLife = prefabLifetime - age;

            if (remainingLife > 0f)
            {
                GameObject spawned = SpawnPrefabAtTimeOffset(age);
                // Adicionar à lista de objetos gerenciados
                spawnedObjects.Add(new SpawnedObject { obj = spawned, remainingLifetime = remainingLife });
            }
        }
    }

    private GameObject SpawnPrefabAtTimeOffset(float timeOffset)
    {
        GameObject[] validPrefabs = System.Array.FindAll(prefabs, p => p != null);
        if (validPrefabs.Length == 0) return null;

        GameObject chosenPrefab = validPrefabs[Random.Range(0, validPrefabs.Length)];
        GameObject spawned = Instantiate(chosenPrefab, transform.position, transform.rotation);

        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Simulate where it would be if fired 'timeOffset' seconds ago
            Vector3 velocity = transform.forward * shootForce;
            spawned.transform.position += velocity * timeOffset;

            rb.linearVelocity = velocity;
        }

        return spawned;
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

        // Adicionar à lista de objetos gerenciados com tempo de vida total
        spawnedObjects.Add(new SpawnedObject { obj = spawned, remainingLifetime = prefabLifetime });
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}
