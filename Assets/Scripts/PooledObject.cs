using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private bool isReturning;

    public GameObject Prefab { get; private set; }

    public void SetPrefab(GameObject prefab)
    {
        Prefab = prefab;
    }

    public void OnSpawned()
    {
        isReturning = false;
    }

    public void ReturnToPool()
    {
        if (isReturning)
            return;

        isReturning = true;

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}