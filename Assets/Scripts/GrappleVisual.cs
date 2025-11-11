using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LinePrefabRendererEffects : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lineRenderer;
    public GameObject lineSegmentPrefabA;
    public GameObject lineSegmentPrefabB;
    public GameObject startTipPrefab;
    public GameObject endTipPrefab;

    [Header("Settings")]
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

    private List<Coroutine> _flickerCoroutines = new List<Coroutine>();

    private GrapplingHookController _grapplingHookController;

    void Awake()
    {
        _grapplingHookController = FindObjectOfType<GrapplingHookController>();
        if (_grapplingHookController == null)
        {
            Debug.LogError("LinePrefabRendererEffects: GrapplingHookController não encontrado na cena. Este componente será desativado.", this);
            enabled = false;
            return;
        }

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
    }
    
    void Start()
    {
        EnsureContainer();
        CachePrefabInfo();
        
        EnsureSingleInstance(ref segmentAInstance, lineSegmentPrefabA);
        EnsureSingleInstance(ref segmentBInstance, lineSegmentPrefabB);
        EnsureSingleInstance(ref startTipInstance, startTipPrefab);
        EnsureSingleInstance(ref endTipInstance, endTipPrefab);

        // Garante que tudo comece desativado
        CleanupAllChildObjects();
        if (segmentParent != null) segmentParent.gameObject.SetActive(false);
    }
    
    // A lógica principal agora está em LateUpdate
    void LateUpdate()
    {
        if (_grapplingHookController.IsGrappling)
        {
            RebuildAll();
        }
        else
        {
            CleanupAllChildObjects();
            if (segmentParent != null && segmentParent.gameObject.activeSelf)
            {
                segmentParent.gameObject.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        CleanupAll();
        StopAllFlickerCoroutines();
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
        StopAllFlickerCoroutines();

        SafeDestroy(segmentAInstance);
        SafeDestroy(segmentBInstance);
        SafeDestroy(startTipInstance);
        SafeDestroy(endTipInstance);

        segmentAInstance = null;
        segmentBInstance = null;
        startTipInstance = null;
        endTipInstance = null;
        
        if (segmentParent != null)
        {
            segmentParent.gameObject.SetActive(false);
        }
    }
    
    private void CleanupAllChildObjects()
    {
        if (segmentAInstance != null) segmentAInstance.SetActive(false);
        if (segmentBInstance != null) segmentBInstance.SetActive(false);
        if (startTipInstance != null) startTipInstance.SetActive(false);
        if (endTipInstance != null) endTipInstance.SetActive(false);
    }

    void CachePrefabInfo()
    {
        GameObject prefabRef = lineSegmentPrefabA != null ? lineSegmentPrefabA : lineSegmentPrefabB;
        if (prefabRef == null) return;

        prefabOriginalLocalScale = prefabRef.transform.localScale;
        Vector3 localSize = Vector3.zero;
        bool found = false;

        MeshFilter mf = prefabRef.GetComponent<MeshFilter>();
        if (mf == null) mf = prefabRef.GetComponentInChildren<MeshFilter>();
        if (mf && mf.sharedMesh) { localSize = mf.sharedMesh.bounds.size; found = true; }
        else
        {
            SkinnedMeshRenderer smr = prefabRef.GetComponent<SkinnedMeshRenderer>();
            if (smr == null) smr = prefabRef.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr && smr.sharedMesh) { localSize = smr.sharedMesh.bounds.size; found = true; }
            else
            {
                SpriteRenderer sr = prefabRef.GetComponent<SpriteRenderer>();
                if (sr == null) sr = prefabRef.GetComponentInChildren<SpriteRenderer>();
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
            if (segmentParent != null) segmentParent.gameObject.SetActive(false);
            CleanupAllChildObjects();
            return;
        }

        EnsureContainer();
        if (segmentParent != null && !segmentParent.gameObject.activeSelf)
        {
            segmentParent.gameObject.SetActive(true);
        }

        Vector3 startPos = lineRenderer.GetPosition(0);
        Vector3 endPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        Vector3 dir = (endPos - startPos);
        float segmentDistance = dir.magnitude;
        if (segmentDistance < minSegmentLength)
        {
            if (segmentParent != null) segmentParent.gameObject.SetActive(false);
            CleanupAllChildObjects();
            return;
        }
        dir.Normalize();

        EnsureSingleInstance(ref segmentAInstance, lineSegmentPrefabA);
        EnsureSingleInstance(ref segmentBInstance, lineSegmentPrefabB);
        EnsureSingleInstance(ref startTipInstance, startTipPrefab);
        EnsureSingleInstance(ref endTipInstance, endTipPrefab);

        ApplySegmentTransform(segmentAInstance, startPos, dir, segmentDistance);
        ApplySegmentTransform(segmentBInstance, startPos, dir, segmentDistance);

        ApplyTipTransform(startTipInstance, startPos, dir, alignTipsToLine);
        ApplyTipTransform(endTipInstance, endPos, dir, alignTipsToLine);
    }

    void EnsureSingleInstance(ref GameObject instance, GameObject prefab)
    {
        if (prefab == null) return;

        if (instance == null)
        {
            if(segmentParent == null) EnsureContainer();
            
            foreach (Transform child in segmentParent)
            {
                if (child != null && child.gameObject != null && child.name.StartsWith(prefab.name))
                {
                    instance = child.gameObject;
                    break;
                }
            }

            if (instance == null)
            {
                instance = Instantiate(prefab, segmentParent);
                instance.name = prefab.name + "_Instance";
                instance.SetActive(false); 

                if (Application.isPlaying)
                {
                    Coroutine flicker = StartCoroutine(FlickerRoutine(instance));
                    _flickerCoroutines.Add(flicker);
                }
            }
        }
    }

    void ApplySegmentTransform(GameObject seg, Vector3 startPos, Vector3 dir, float segLength)
    {
        if (seg == null) return;

        seg.transform.position = startPos + dir * (segLength / 2f);
        seg.transform.rotation = Quaternion.FromToRotation(prefabPrimaryAxisLocal, dir);

        float scaleFactor = segLength / Mathf.Max(0.0001f, prefabLength);
        Vector3 newScale = prefabOriginalLocalScale;

        if (prefabPrimaryAxisLocal == Vector3.right) newScale.x *= scaleFactor;
        else if (prefabPrimaryAxisLocal == Vector3.up) newScale.y *= scaleFactor;
        else newScale.z *= scaleFactor;

        seg.transform.localScale = newScale;
        seg.SetActive(true);
    }

    void ApplyTipTransform(GameObject tip, Vector3 position, Vector3 dir, bool align)
    {
        if (tip == null) return;

        tip.transform.position = position;
        if (align)
        {
            tip.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
        tip.SetActive(true);
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
    
    private void StopAllFlickerCoroutines()
    {
        foreach (Coroutine coroutine in _flickerCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        _flickerCoroutines.Clear();
    }

    IEnumerator FlickerRoutine(GameObject seg)
    {
        if (seg == null) yield break;

        SpriteRenderer sr = seg.GetComponentInChildren<SpriteRenderer>();

        while (seg != null && this != null)
        {
            yield return new WaitForSeconds(flickerRate);
            if (seg == null) yield break;
            
            if (sr != null)
            {
                sr.enabled = Random.value > flickerChance;
            }
            else
            {
                if (seg.activeSelf)
                {
                    seg.SetActive(Random.value > flickerChance);
                }
            }
        }
    }
}