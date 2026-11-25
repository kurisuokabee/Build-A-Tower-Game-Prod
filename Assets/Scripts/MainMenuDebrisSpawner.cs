
using UnityEngine;

public class MainMenuDebrisSpawner : MonoBehaviour
{
    [Header("Debris")]
    [SerializeField] private GameObject[] debrisPrefabs;
    [SerializeField] private float spawnInterval = 0.3f;
    [SerializeField] private float spawnWidth = 8f;
    [SerializeField] private float spawnY = -6f;

    [Header("Movement")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float minRotationSpeed = -100f;
    [SerializeField] private float maxRotationSpeed = 100f;
    [Header("Debris Size")]
    [SerializeField] private float minSize = 0.5f;
    [SerializeField] private float maxSize = 1.5f;
    [Header("Lifetime")]
    [SerializeField] private float destroyY = 7f;
    [Header("Sorting Order")]
[SerializeField] private int slowSortingOrder = -1;
[SerializeField] private int fastSortingOrder = 2;

    private float spawnTimer;

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnDebris();
        }
    }

    private void SpawnDebris()
    {
        if (debrisPrefabs == null || debrisPrefabs.Length == 0)
            return;

        GameObject prefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Length)];

        float randomX = Random.Range(-spawnWidth, spawnWidth);

        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        float speed = Random.Range(minSpeed, maxSpeed);
        float rotation = Random.Range(minRotationSpeed, maxRotationSpeed);

        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
        float size = Mathf.Lerp(minSize, maxSize, speedPercent);

        GameObject debris = Instantiate(prefab, spawnPosition, Quaternion.identity);

        int sortingOrder = Mathf.RoundToInt(
            Mathf.Lerp(slowSortingOrder, fastSortingOrder, speedPercent)
        );

        SpriteRenderer[] renderers = debris.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingOrder = sortingOrder;
        }
        debris.transform.localScale *= size;

        MainMenuDebris movement = debris.AddComponent<MainMenuDebris>();
        movement.Initialize(speed, rotation, destroyY);
    }
}
