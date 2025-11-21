using System.Collections.Generic;
using UnityEngine;

public class RandomPrefabShooter : MonoBehaviour, ITimeSlowable, IResettable
{
    [Header("Prefab Settings")]
    public GameObject[] prefabs = new GameObject[5];

    [Header("Shooting Settings")]
    public float shootInterval = 2f;
    public float shootForce = 10f;

    [Header("Lifetime Settings")]
    public float prefabLifetime = 5f;

    [Header("Prewarm Settings")]
    public bool enablePrewarm = true;

    [Header("Pool Settings")]
    public int initialPoolSize = 10;

    // Estado interno
    private float _shootTimer;
    private float _timeScale = 1.0f;
    private bool _isSlowed = false;

    // Pool System
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();
    private Dictionary<GameObject, int> _activeObjectMap = new Dictionary<GameObject, int>();

    private struct SpawnedObject
    {
        public GameObject obj;
        public float remainingLifetime;
        public ITimeSlowable slowableComponent; // Cache da interface para performance
    }

    private List<SpawnedObject> _activeObjects = new List<SpawnedObject>();

    private void Awake()
    {
        InitializePools();
    }

    private void Start()
    {
        if (TimeManipulationManager.Instance != null)
            TimeManipulationManager.Instance.Register(this);

        if (enablePrewarm)
            PerformPrewarm();
    }

    private void OnDisable()
    {
        if (TimeManipulationManager.Instance != null)
            TimeManipulationManager.Instance.Unregister(this);
    }

    private void Update()
    {
        float adjustedDeltaTime = Time.deltaTime * _timeScale;
        if (adjustedDeltaTime <= 0f) return;

        // 1. Lógica de Disparo
        _shootTimer += adjustedDeltaTime;
        if (_shootTimer >= shootInterval)
        {
            ShootRandomPrefab();
            _shootTimer = 0f;
        }

        // 2. Gerenciar objetos ativos
        for (int i = _activeObjects.Count - 1; i >= 0; i--)
        {
            SpawnedObject so = _activeObjects[i];
            
            if (so.obj == null || !so.obj.activeInHierarchy)
            {
                _activeObjects.RemoveAt(i);
                continue;
            }

            so.remainingLifetime -= adjustedDeltaTime;

            if (so.remainingLifetime <= 0f)
            {
                ReturnToPool(so.obj);
                _activeObjects.RemoveAt(i);
            }
            else
            {
                _activeObjects[i] = so;
            }
        }
    }

    // --- Pool Logic ---

    private void InitializePools()
    {
        foreach (var prefab in prefabs)
        {
            if (prefab == null) continue;
            int prefabId = prefab.GetInstanceID();
            
            if (!_pools.ContainsKey(prefabId))
            {
                _pools[prefabId] = new Queue<GameObject>();
                for (int i = 0; i < initialPoolSize; i++)
                {
                    CreateNewPoolObject(prefab, prefabId);
                }
            }
        }
    }

    private GameObject CreateNewPoolObject(GameObject prefab, int prefabId)
    {
        GameObject obj = Instantiate(prefab, transform.position, transform.rotation);
        obj.SetActive(false); // Nasce desativado
        _pools[prefabId].Enqueue(obj);
        return obj;
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        int prefabId = prefab.GetInstanceID();
        if (!_pools.ContainsKey(prefabId)) _pools[prefabId] = new Queue<GameObject>();

        GameObject obj = (_pools[prefabId].Count > 0) 
            ? _pools[prefabId].Dequeue() 
            : Instantiate(prefab, transform.position, transform.rotation);

        obj.SetActive(true);
        _activeObjectMap[obj] = prefabId;

        // --- CORREÇÃO CRÍTICA DO SLOW MOTION ---
        // O Start() do objeto não vai rodar, então precisamos configurar o tempo manualmente
        ITimeSlowable slowable = obj.GetComponent<ITimeSlowable>();
        if (slowable != null)
        {
            // 1. Limpa estado anterior (garante que não venha travado)
            slowable.RestoreNormalTime(); 

            // 2. Registra manualmente no Manager (já que o Start não roda)
            if (TimeManipulationManager.Instance != null)
                TimeManipulationManager.Instance.Register(slowable);

            // 3. Se o mundo JÁ estiver em slow motion, aplica no objeto agora
            if (_isSlowed)
            {
                // Calcula a porcentagem inversa baseada no timescale atual
                float currentSlowPercentage = (1f - _timeScale) * 100f;
                slowable.SlowDown(currentSlowPercentage);
            }
        }

        return obj;
    }

    private void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        // --- LIMPEZA ANTES DE DEVOLVER ---
        ITimeSlowable slowable = obj.GetComponent<ITimeSlowable>();
        if (slowable != null)
        {
            slowable.RestoreNormalTime(); // Reseta cor, velocidade, kinematic, etc.
            
            // Desregistra para não ficar pesando no Manager enquanto dorme no pool
            if (TimeManipulationManager.Instance != null)
                TimeManipulationManager.Instance.Unregister(slowable);
        }
        
        // Reseta Física Básica (caso o script de slow falhe)
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false; 
        }

        obj.SetActive(false);
        obj.transform.SetParent(null);

        if (_activeObjectMap.TryGetValue(obj, out int prefabId))
        {
            _pools[prefabId].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    // --- Shooting & Prewarm ---

    private void PerformPrewarm()
    {
        if (prefabs == null || prefabs.Length == 0) return;

        int steps = Mathf.FloorToInt(prefabLifetime / shootInterval);
        
        for (int i = 0; i < steps; i++)
        {
            float age = i * shootInterval;
            float remainingLife = prefabLifetime - age;

            if (remainingLife > 0f)
            {
                GameObject spawned = SpawnPrefabAtTimeOffset(age);
                if (spawned != null)
                {
                    _activeObjects.Add(new SpawnedObject { 
                        obj = spawned, 
                        remainingLifetime = remainingLife,
                        slowableComponent = spawned.GetComponent<ITimeSlowable>()
                    });
                }
            }
        }
    }

    private GameObject SpawnPrefabAtTimeOffset(float timeOffset)
    {
        GameObject[] validPrefabs = System.Array.FindAll(prefabs, p => p != null);
        if (validPrefabs.Length == 0) return null;

        GameObject chosenPrefab = validPrefabs[Random.Range(0, validPrefabs.Length)];
        GameObject spawned = GetFromPool(chosenPrefab); // Usa o método corrigido

        spawned.transform.position = transform.position;
        spawned.transform.rotation = transform.rotation;

        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 velocity = transform.forward * shootForce;
            
            // Aplica o offset de tempo na posição física
            // Se estiver em slow, o Physics.Simulate não roda, então movemos manualmente
            spawned.transform.position += velocity * timeOffset; 
            
            // Define a velocidade inicial correta
            if (_isSlowed)
                 rb.linearVelocity = velocity * _timeScale;
            else
                 rb.linearVelocity = velocity;
        }

        return spawned;
    }

    private void ShootRandomPrefab()
    {
        GameObject[] validPrefabs = System.Array.FindAll(prefabs, p => p != null);
        if (validPrefabs.Length == 0) return;

        GameObject chosenPrefab = validPrefabs[Random.Range(0, validPrefabs.Length)];
        GameObject spawned = GetFromPool(chosenPrefab); // Usa o método corrigido

        spawned.transform.position = transform.position;
        spawned.transform.rotation = transform.rotation;

        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Aplica força. Se tiver SlowableRigidbody, ele vai capturar essa velocidade
            // e multiplicar pelo fator de lentidão automaticamente no próximo frame dele
            rb.AddForce(transform.forward * shootForce, ForceMode.VelocityChange);
            
            // Ajuste fino imediato caso o objeto nasça já dentro do slow motion
            if (_isSlowed)
            {
                 rb.linearVelocity *= _timeScale;
            }
        }

        _activeObjects.Add(new SpawnedObject { 
            obj = spawned, 
            remainingLifetime = prefabLifetime,
            slowableComponent = spawned.GetComponent<ITimeSlowable>()
        });
    }

    // --- Interfaces ---

    public void SlowDown(float slowPercentage)
    {
        _isSlowed = true;
        _timeScale = 1f - (slowPercentage / 100f);
        
        // ATENÇÃO: Não precisamos iterar sobre _activeObjects para dar slow neles,
        // pois eles já estão registrados no TimeManager individualmente.
    }

    public void RestoreNormalTime()
    {
        _isSlowed = false;
        _timeScale = 1.0f;
    }

    public void SetSlowDownColor(Color newColor) { }

    public void ResetState()
    {
        // Devolve todos pro pool
        for (int i = _activeObjects.Count - 1; i >= 0; i--)
        {
            ReturnToPool(_activeObjects[i].obj);
        }
        _activeObjects.Clear();

        _shootTimer = 0f;
        RestoreNormalTime();

        if (enablePrewarm)
            PerformPrewarm();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}