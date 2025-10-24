using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[ExecuteAlways]
public class LinePrefabRendererFinal : MonoBehaviour
{
	[Header("References")]
	public LineRenderer lineRenderer;
	public GameObject lineSegmentPrefabA;
	public GameObject lineSegmentPrefabB;

	[Header("Settings")]
	public bool updateInEditor = true;
	public bool updateInPlay = true;
	public float minSegmentLength = 0.001f;

	private class SegmentPair
	{
		public GameObject A;
		public GameObject B;
	}

	private List<SegmentPair> segments = new List<SegmentPair>();
	private Transform segmentParent;

	private float prefabLength = 1f;
	private Vector3 prefabPrimaryAxisLocal = Vector3.forward;
	private Vector3 prefabOriginalLocalScale = Vector3.one;

	void OnEnable()
	{
		CleanupOldSegments();
		CachePrefabInfo();
		EnsureContainer();
		RebuildAll();
	}

	void OnDestroy() => CleanupOldSegments();

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

	void CleanupOldSegments()
	{
		if (segmentParent)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				DestroyImmediate(segmentParent.gameObject);
			else
				Destroy(segmentParent.gameObject);
#else
            Destroy(segmentParent.gameObject);
#endif
		}

		segments.Clear();
		segmentParent = null;
	}

	void CachePrefabInfo()
	{
		GameObject prefabRef = lineSegmentPrefabA != null ? lineSegmentPrefabA : lineSegmentPrefabB;
		if (prefabRef == null) return;

		prefabOriginalLocalScale = prefabRef.transform.localScale;
		Vector3 localSize = Vector3.zero;
		bool found = false;

		MeshFilter mf = prefabRef.GetComponentInChildren<MeshFilter>();
		if (mf && mf.sharedMesh)
		{
			localSize = mf.sharedMesh.bounds.size;
			found = true;
		}
		else
		{
			SkinnedMeshRenderer smr = prefabRef.GetComponentInChildren<SkinnedMeshRenderer>();
			if (smr && smr.sharedMesh)
			{
				localSize = smr.sharedMesh.bounds.size;
				found = true;
			}
			else
			{
				SpriteRenderer sr = prefabRef.GetComponentInChildren<SpriteRenderer>();
				if (sr && sr.sprite)
				{
					localSize = sr.sprite.bounds.size;
					found = true;
				}
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
		if (lineRenderer == null || (lineSegmentPrefabA == null && lineSegmentPrefabB == null))
			return;

		EnsureContainer();

		int needed = Mathf.Max(0, lineRenderer.positionCount - 1);

		// Create more segments if needed
		while (segments.Count < needed)
		{
			SegmentPair pair = new SegmentPair();

			if (lineSegmentPrefabA)
				pair.A = Instantiate(lineSegmentPrefabA, segmentParent);

			if (lineSegmentPrefabB)
				pair.B = Instantiate(lineSegmentPrefabB, segmentParent);

			segments.Add(pair);
		}

		// Activate only what’s needed
		for (int i = 0; i < segments.Count; i++)
		{
			bool active = (i < needed);
			if (segments[i].A) segments[i].A.SetActive(active);
			if (segments[i].B) segments[i].B.SetActive(active);
		}

		// Update positions, scales, etc.
		for (int i = 0; i < needed; i++)
		{
			Vector3 start = lineRenderer.GetPosition(i);
			Vector3 end = lineRenderer.GetPosition(i + 1);
			float segLength = Vector3.Distance(start, end);

			if (segLength < minSegmentLength)
			{
				if (segments[i].A) segments[i].A.SetActive(false);
				if (segments[i].B) segments[i].B.SetActive(false);
				continue;
			}

			Vector3 mid = (start + end) * 0.5f;
			Vector3 dir = (end - start).normalized;

			ApplyTransform(segments[i].A, mid, dir, segLength);
			ApplyTransform(segments[i].B, mid, dir, segLength);

			if (Application.isPlaying)
			{
				if (segments[i].A) StartCoroutine(DelayedAnimationStart(segments[i].A, 0.05f));
				if (segments[i].B) StartCoroutine(DelayedAnimationStart(segments[i].B, 0.05f));
			}
		}
	}

	void ApplyTransform(GameObject seg, Vector3 mid, Vector3 dir, float segLength)
	{
		if (seg == null) return;

		seg.transform.position = mid;

		// Align prefab along line direction
		Vector3 prefabAxisWorld = seg.transform.TransformDirection(prefabPrimaryAxisLocal).normalized;
		Quaternion align = Quaternion.FromToRotation(prefabAxisWorld, dir);
		seg.transform.rotation = align * seg.transform.rotation;

		// Scale to match segment length
		float scaleFactor = segLength / Mathf.Max(0.0001f, prefabLength);
		Vector3 newScale = prefabOriginalLocalScale;

		if (prefabPrimaryAxisLocal == Vector3.right)
			newScale.x *= scaleFactor;
		else if (prefabPrimaryAxisLocal == Vector3.up)
			newScale.y *= scaleFactor;
		else
			newScale.z *= scaleFactor;

		seg.transform.localScale = newScale;
	}

	IEnumerator DelayedAnimationStart(GameObject seg, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (seg == null) yield break;

		Animator animator = seg.GetComponentInChildren<Animator>();
		if (animator && animator.runtimeAnimatorController)
		{
			animator.Update(0f);
			animator.Play(0, 0, 0f);
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
}
