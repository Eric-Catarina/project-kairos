using UnityEngine;

public class SpawnOnTouch : MonoBehaviour
{
	public GameObject prefab;          // Prefab to spawn
	public float lifetime = 2f;        // How long before it disappears
	public Vector3 offset;             // Optional position offset

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			// Spawn as a child of the player so it follows
			GameObject spawned = Instantiate(
				prefab,
				other.transform.position + offset,
				other.transform.rotation,
				other.transform // parent = player
			);

			// Destroy after time
			Destroy(spawned, lifetime);
		}
	}
}
