using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[ExecuteAlways]
public class LinePrefabRendererFinal : MonoBehaviour
{
	[Header("References")]
	public LineRenderer lineRenderer;
	public GameObject lineSegmentPrefab;

	[Header("Settings")]
	public bool updateInEditor = true;
	public bool updateInPlay = true;
	public float minSegmentLength = 0.001f;

	private List<GameObject> segments = new List<GameObject>();
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
		if (lineSegmentPrefab == null) return;

		prefabOriginalLocalScale = lineSegmentPrefab.transform.localScale;
		Vector3 localSize = Vector3.zero;
		bool found = false;

		MeshFilter mf = lineSegmentPrefab.GetComponentInChildren<MeshFilter>();
		if (mf && mf.sharedMesh)
		{
			localSize = mf.sharedMesh.bounds.size;
			found = true;
		}
		else
		{
			SkinnedMeshRenderer smr = lineSegmentPrefab.GetComponentInChildren<SkinnedMeshRenderer>();
			if (smr && smr.sharedMesh)
			{
				localSize = smr.sharedMesh.bounds.size;
				found = true;
			}
			else
			{
				SpriteRenderer sr = lineSegmentPrefab.GetComponentInChildren<SpriteRenderer>();
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
		if (lineRenderer == null || lineSegmentPrefab == null) return;
		EnsureContainer();

		int needed = Mathf.Max(0, lineRenderer.positionCount - 1);

		while (segments.Count < needed)
		{
			GameObject seg = Instantiate(lineSegmentPrefab, segmentParent);
			seg.name = $"LineSeg_{segments.Count}";
			segments.Add(seg);
		}

		for (int i = 0; i < segments.Count; i++)
		{
			bool active = (i < needed);
			if (segments[i] && segments[i].activeSelf != active)
				segments[i].SetActive(active);
		}

		for (int i = 0; i < needed; i++)
		{
			Vector3 start = lineRenderer.GetPosition(i);
			Vector3 end = lineRenderer.GetPosition(i + 1);
			float segLength = Vector3.Distance(start, end);
			if (segLength < minSegmentLength)
			{
				segments[i].SetActive(false);
				continue;
			}

			GameObject seg = segments[i];
			seg.SetActive(true);

			Vector3 mid = (start + end) * 0.5f;
			Vector3 dir = (end - start).normalized;
			seg.transform.position = mid;

			Vector3 prefabAxisWorld = seg.transform.TransformDirection(prefabPrimaryAxisLocal).normalized;
			Quaternion align = Quaternion.FromToRotation(prefabAxisWorld, dir);
			seg.transform.rotation = align * seg.transform.rotation;

			float scaleFactor = segLength / Mathf.Max(0.0001f, prefabLength);
			Vector3 newScale = prefabOriginalLocalScale;

			if (prefabPrimaryAxisLocal == Vector3.right)
				newScale.x *= scaleFactor;
			else if (prefabPrimaryAxisLocal == Vector3.up)
				newScale.y *= scaleFactor;
			else
				newScale.z *= scaleFactor;

			seg.transform.localScale = newScale;

			// Start animations next frame (so Unity has initialized animators)
			if (Application.isPlaying)
				StartCoroutine(DelayedAnimationStart(seg, 0.05f));
		}
	}

	IEnumerator DelayedAnimationStart(GameObject seg, float delay)
	{
		yield return new WaitForSeconds(delay); // wait 1 frame or 0.05s for reliability
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
