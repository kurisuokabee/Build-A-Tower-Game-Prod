using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 100;

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new();
    private readonly Dictionary<GameObject, GameObject> prefabLookup = new();
    private readonly Dictionary<GameObject, Transform> parentLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject GetObject(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("Cannot pool a null prefab!");
            return null;
        }

        if (!pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
        {
            pool = CreatePool(prefab);
            pools.Add(prefab, pool);
        }

        GameObject obj = pool.Get();

        obj.transform.SetParent(GetPoolParent(prefab));
        obj.transform.SetPositionAndRotation(position, rotation);

        PooledObject pooledObject = obj.GetComponent<PooledObject>();
        pooledObject?.OnSpawned();

        obj.SetActive(true);

        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        if (obj == null)
            return;

        if (!prefabLookup.TryGetValue(obj, out GameObject prefab))
        {
            Debug.LogWarning(
                $"{obj.name} was not created by ObjectPoolManager.",
                obj
            );
            return;
        }

        if (pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            pool.Release(obj);
    }

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        return new ObjectPool<GameObject>(
            () => CreateObject(prefab),
            obj => { },
            obj =>
            {
                obj.SetActive(false);

                PooledObject pooledObject = obj.GetComponent<PooledObject>();

                if (pooledObject != null)
                    obj.transform.SetParent(GetPoolParent(pooledObject.Prefab));
            },
            Destroy,
            true,
            defaultCapacity,
            maxSize
        );
    }

    private GameObject CreateObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);

        PooledObject pooledObject = obj.GetComponent<PooledObject>();

        if (pooledObject == null)
            pooledObject = obj.AddComponent<PooledObject>();

        pooledObject.SetPrefab(prefab);
        prefabLookup[obj] = prefab;

        return obj;
    }
    private Transform GetPoolParent(GameObject prefab)
    {
        if (parentLookup.TryGetValue(prefab, out Transform parent))
            return parent;

        GameObject container = new GameObject(prefab.name);
        container.transform.SetParent(transform);

        parentLookup.Add(prefab, container.transform);

        return container.transform;
    }

    public void Preload(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
            return;

        List<GameObject> preloadedObjects = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = GetObject(
                prefab,
                Vector3.zero,
                Quaternion.identity
            );

            if (obj != null)
                preloadedObjects.Add(obj);
        }

        foreach (GameObject obj in preloadedObjects)
            ReturnObject(obj);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        foreach (ObjectPool<GameObject> pool in pools.Values)
            pool.Clear();

        pools.Clear();
        prefabLookup.Clear();
    }
}
