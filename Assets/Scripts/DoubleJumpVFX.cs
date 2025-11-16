using UnityEngine;

public class DoubleJumpEffectSpawner : MonoBehaviour
{
	[Header("Prefab to Spawn on Double Jump")]
	public GameObject jumpEffectPrefab;

	[Header("Lifetime of the Effect")]
	public float effectLifetime = 1.5f;

	/// <summary>
	/// Call this function exactly when you perform the double jump.
	/// </summary>
	public void SpawnDoubleJumpEffect()
	{
		if (jumpEffectPrefab == null)
		{
			Debug.LogWarning("DoubleJumpEffectSpawner: No prefab assigned!");
			return;
		}

		// Feet position = player position minus half height
		Vector3 feetPos = GetFeetPosition();

		GameObject effect = Instantiate(jumpEffectPrefab, feetPos, Quaternion.identity);

		Destroy(effect, effectLifetime);
	}

	private Vector3 GetFeetPosition()
	{
		// Try to use a collider to get the real bottom of the player
		Collider col = GetComponent<Collider>();
		if (col != null)
		{
			return new Vector3(transform.position.x, transform.position.y - col.bounds.extents.y, transform.position.z);
		}

		// Fallback: simple offset
		return transform.position + Vector3.down * 1f;
	}
}
