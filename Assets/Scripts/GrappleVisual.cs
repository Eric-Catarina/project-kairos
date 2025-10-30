using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class LinePrefabRendererEffects : MonoBehaviour
{
	[Header("References")]
	public LineRenderer lineRenderer;
	public GameObject lineSegmentPrefabA;
	public GameObject lineSegmentPrefabB;
	public GameObject startTipPrefab;
	public GameObject endTipPrefab;

	[Header("Settings")]
	public bool updateInEditor = true;
	public bool updateInPlay = true;
	public float minSegmentLength = 0.001f;
	[Range(0f, 1f)] public float flickerChance = 0.3f;
	public float flickerRate = 0.05f;
	public bool alignTipsToLine = true;

	private GameObject segmentAInstance;
	private GameObject segmentBInstance;
	private GameObject startTipInstance;
	private GameObject endTipInstance;

	private Transform segmentParent;
	private float prefabLength = 1f;
	private Vector3 prefabPrimaryAxisLocal = Vector3.forward;
	private Vector3 prefabOriginalLocalScale = Vector3.one;

	void OnEnable()
	{
		CleanupAll();
		CachePrefabInfo();
		EnsureContainer();
		RebuildAll();
	}

	void OnDestroy() => CleanupAll();

	void Start()
	{
		if (lineRenderer == null)
			lineRenderer = GetComponent<LineRenderer>();

		CachePrefabInfo();
		EnsureContainer();
		RebuildAll();
	}

	void Update()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying && updateInEditor)
			RebuildAll();
#endif
		if (Application.isPlaying && updateInPlay)
			RebuildAll();
	}

	void EnsureContainer()
	{
		if (segmentParent == null)
		{
			Transform existing = transform.Find("LineSegmentsContainer");
			if (existing)
				segmentParent = existing;
			else
			{
				GameObject container = new GameObject("LineSegmentsContainer");
				container.transform.SetParent(transform, false);
				segmentParent = container.transform;
			}
		}
	}

	void CleanupAll()
	{
		SafeDestroy(segmentAInstance);
		SafeDestroy(segmentBInstance);
		SafeDestroy(startTipInstance);
		SafeDestroy(endTipInstance);

		segmentAInstance = null;
		segmentBInstance = null;
		startTipInstance = null;
		endTipInstance = null;
	}

	void CachePrefabInfo()
	{
		GameObject prefabRef = lineSegmentPrefabA != null ? lineSegmentPrefabA : lineSegmentPrefabB;
		if (prefabRef == null) return;

		prefabOriginalLocalScale = prefabRef.transform.localScale;
		Vector3 localSize = Vector3.zero;
		bool found = false;

		MeshFilter mf = prefabRef.GetComponentInChildren<MeshFilter>();
		if (mf && mf.sharedMesh) { localSize = mf.sharedMesh.bounds.size; found = true; }
		else
		{
			SkinnedMeshRenderer smr = prefabRef.GetComponentInChildren<SkinnedMeshRenderer>();
			if (smr && smr.sharedMesh) { localSize = smr.sharedMesh.bounds.size; found = true; }
			else
			{
				SpriteRenderer sr = prefabRef.GetComponentInChildren<SpriteRenderer>();
				if (sr && sr.sprite) { localSize = sr.sprite.bounds.size; found = true; }
			}
		}

		if (!found) localSize = new Vector3(0.1f, 0.1f, 1f);

		if (localSize.x >= localSize.y && localSize.x >= localSize.z)
		{
			prefabPrimaryAxisLocal = Vector3.right;
			prefabLength = localSize.x * Mathf.Abs(prefabOriginalLocalScale.x);
		}
		else if (localSize.y >= localSize.x && localSize.y >= localSize.z)
		{
			prefabPrimaryAxisLocal = Vector3.up;
			prefabLength = localSize.y * Mathf.Abs(prefabOriginalLocalScale.y);
		}
		else
		{
			prefabPrimaryAxisLocal = Vector3.forward;
			prefabLength = localSize.z * Mathf.Abs(prefabOriginalLocalScale.z);
		}

		if (prefabLength <= 0f) prefabLength = 1f;
	}

	void RebuildAll()
	{
		if (lineRenderer == null || lineRenderer.positionCount < 2)
		{
			CleanupAll();
			return;
		}

		EnsureContainer();

		Vector3 startPos = lineRenderer.GetPosition(0);
		Vector3 endPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
		Vector3 dir = (endPos - startPos).normalized;

		// Ensure single instance for each prefab
		EnsureSingleInstance(ref segmentAInstance, lineSegmentPrefabA);
		EnsureSingleInstance(ref segmentBInstance, lineSegmentPrefabB);
		EnsureSingleInstance(ref startTipInstance, startTipPrefab);
		EnsureSingleInstance(ref endTipInstance, endTipPrefab);

		// Place segments at midpoint and scale
		Vector3 mid = (startPos + endPos) * 0.5f;
		ApplyTransform(segmentAInstance, mid, dir, Vector3.Distance(startPos, endPos));
		ApplyTransform(segmentBInstance, mid, dir, Vector3.Distance(startPos, endPos));

		// Place tips
		if (startTipInstance != null)
		{
			startTipInstance.SetActive(true);
			startTipInstance.transform.position = startPos;
			if (alignTipsToLine) startTipInstance.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
		}

		if (endTipInstance != null)
		{
			endTipInstance.SetActive(true);
			endTipInstance.transform.position = endPos;
			if (alignTipsToLine) endTipInstance.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
		}
	}

	void EnsureSingleInstance(ref GameObject instance, GameObject prefab)
	{
		if (prefab == null) return;

		// Check if instance already exists in the container
		if (instance == null)
		{
			foreach (Transform child in segmentParent)
			{
				if (child.name.StartsWith(prefab.name))
				{
					instance = child.gameObject;
					break;
				}
			}

			// Instantiate if still null
			if (instance == null)
			{
				instance = Instantiate(prefab, segmentParent);
				PlayAnimator(instance);
				if (Application.isPlaying) StartCoroutine(FlickerRoutine(instance));
			}
		}
	}

	void ApplyTransform(GameObject seg, Vector3 mid, Vector3 dir, float segLength)
	{
		if (seg == null) return;

		seg.transform.position = mid;
		Vector3 prefabAxisWorld = seg.transform.TransformDirection(prefabPrimaryAxisLocal).normalized;
		Quaternion align = Quaternion.FromToRotation(prefabAxisWorld, dir);
		seg.transform.rotation = align * seg.transform.rotation;

		float scaleFactor = segLength / Mathf.Max(0.0001f, prefabLength);
		Vector3 newScale = prefabOriginalLocalScale;
		if (prefabPrimaryAxisLocal == Vector3.right) newScale.x *= scaleFactor;
		else if (prefabPrimaryAxisLocal == Vector3.up) newScale.y *= scaleFactor;
		else newScale.z *= scaleFactor;

		seg.transform.localScale = newScale;
	}

	void PlayAnimator(GameObject seg)
	{
		if (seg == null) return;

		Animator animator = seg.GetComponentInChildren<Animator>();
		if (animator != null && animator.runtimeAnimatorController != null)
		{
			animator.enabled = true;
			animator.Rebind();
			animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
		}

		Animation legacy = seg.GetComponentInChildren<Animation>();
		if (legacy && legacy.clip)
		{
			legacy.Stop();
			legacy.Play();
		}

		ParticleSystem[] ps = seg.GetComponentsInChildren<ParticleSystem>();
		foreach (var p in ps)
		{
			p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			p.Play(true);
		}
	}

	void SafeDestroy(GameObject go)
	{
		if (go == null) return;
#if UNITY_EDITOR
		if (!Application.isPlaying) DestroyImmediate(go);
		else Destroy(go);
#else
        Destroy(go);
#endif
	}

	IEnumerator FlickerRoutine(GameObject seg)
	{
		if (seg == null) yield break;
		SpriteRenderer sr = seg.GetComponentInChildren<SpriteRenderer>();

		while (seg != null)
		{
			yield return new WaitForSeconds(flickerRate);
			if (sr) sr.enabled = Random.value > flickerChance;
			else seg.SetActive(Random.value > flickerChance);
		}
	}
}
