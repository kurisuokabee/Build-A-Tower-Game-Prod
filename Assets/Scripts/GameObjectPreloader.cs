using UnityEngine;
using System.Collections;

public class GameObjectPreloader : MonoBehaviour
{
    [System.Serializable]
    public class PreloadEntry
    {
        public GameObject prefab;
        [Min(0)] public int count = 20;
    }

    [SerializeField] private PreloadEntry[] objectsToPreload;

    private IEnumerator Start()
    {
        // Wait until scene Awake methods have initialized.
        yield return null;

        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager is missing!");
            yield break;
        }

        foreach (PreloadEntry entry in objectsToPreload)
        {
            if (entry.prefab == null || entry.count <= 0)
                continue;

            ObjectPoolManager.Instance.Preload(
                entry.prefab,
                entry.count
            );
        }
    }
}