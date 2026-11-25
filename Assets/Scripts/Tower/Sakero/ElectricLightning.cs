
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ElectricLightning : MonoBehaviour
{
    [Header("Pixel Beam")]
    [SerializeField] private float pixelSize = 0.08f;
    [SerializeField] private float thickness = 0.32f;
    [SerializeField] private float jaggedness = 0.2f;
    [SerializeField] private float jaggedStep = 0.25f;

    [Header("Appearance")]
    [SerializeField] private Color beamColor = Color.cyan;
    [SerializeField] private int sortingOrder = 10;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh beamMesh;
    private MaterialPropertyBlock propertyBlock;

    private readonly HashSet<Vector2Int> pixels = new HashSet<Vector2Int>();

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        beamMesh = new Mesh();
        beamMesh.name = "Pixel Lightning Mesh";
        beamMesh.indexFormat = IndexFormat.UInt32;

        meshFilter.sharedMesh = beamMesh;

        propertyBlock = new MaterialPropertyBlock();

        meshRenderer.sortingOrder = sortingOrder;
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", beamColor);
        propertyBlock.SetColor("_BaseColor", beamColor);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    public void Initialize(
        Vector3 startPosition,
        Vector3 endPosition,
        float duration)
    {
        StopAllCoroutines();

        // Keep the mesh positioned at the world origin.
        // Its vertices use world-space coordinates.
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        DrawLightning(startPosition, endPosition);

        StartCoroutine(LifetimeRoutine(duration));
    }

    private void DrawLightning(
        Vector3 startPosition,
        Vector3 endPosition)
    {
        beamMesh.Clear();
        pixels.Clear();

        Vector2 start = startPosition;
        Vector2 end = endPosition;

        Vector2 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
            return;

        Vector2 perpendicular = new Vector2(
            -direction.normalized.y,
            direction.normalized.x
        );

        float spacing = Mathf.Max(pixelSize, jaggedStep);
        int steps = Mathf.Max(2, Mathf.CeilToInt(distance / spacing));

        Vector2 previousPoint = start;

        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 currentPoint = Vector2.Lerp(start, end, t);

            if (i < steps)
            {
                currentPoint += perpendicular *
                    Random.Range(-jaggedness, jaggedness);
            }

            AddPixelsAlongSegment(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        BuildMesh();
    }

    private void AddPixelsAlongSegment(Vector2 start, Vector2 end)
    {
        float radius = Mathf.Max(pixelSize, thickness) * 0.5f;

        Vector2 min = Vector2.Min(start, end) - Vector2.one * radius;
        Vector2 max = Vector2.Max(start, end) + Vector2.one * radius;

        int minX = Mathf.FloorToInt(min.x / pixelSize);
        int maxX = Mathf.CeilToInt(max.x / pixelSize);
        int minY = Mathf.FloorToInt(min.y / pixelSize);
        int maxY = Mathf.CeilToInt(max.y / pixelSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int pixel = new Vector2Int(x, y);

                Vector2 center = new Vector2(
                    (x + 0.5f) * pixelSize,
                    (y + 0.5f) * pixelSize
                );

                float distance = DistanceToSegment(center, start, end);

                if (distance <= radius)
                    pixels.Add(pixel);
            }
        }
    }

    private float DistanceToSegment(
        Vector2 point,
        Vector2 start,
        Vector2 end)
    {
        Vector2 segment = end - start;

        float lengthSquared = segment.sqrMagnitude;

        if (lengthSquared <= 0.0001f)
            return Vector2.Distance(point, start);

        float t = Mathf.Clamp01(
            Vector2.Dot(point - start, segment) / lengthSquared
        );

        Vector2 closestPoint = start + segment * t;

        return Vector2.Distance(point, closestPoint);
    }

    private void BuildMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (Vector2Int pixel in pixels)
        {
            float x = pixel.x * pixelSize;
            float y = pixel.y * pixelSize;

            int index = vertices.Count;

            vertices.Add(new Vector3(x, y, 0f));
            vertices.Add(new Vector3(x, y + pixelSize, 0f));
            vertices.Add(new Vector3(x + pixelSize, y + pixelSize, 0f));
            vertices.Add(new Vector3(x + pixelSize, y, 0f));

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        beamMesh.SetVertices(vertices);
        beamMesh.SetTriangles(triangles, 0);
        beamMesh.RecalculateBounds();
    }

    private IEnumerator LifetimeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        else
            Destroy(gameObject);
    }

    public void InitializeImpact(Vector3 position, float duration)
    {
        StopAllCoroutines();

        float radius = 0.3f;

        Vector3 endPosition = position + new Vector3(
            Random.Range(-radius, radius),
            Random.Range(-radius, radius),
            0f
        );

        DrawLightning(position, endPosition);

        StartCoroutine(LifetimeRoutine(duration));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        if (beamMesh != null)
            Destroy(beamMesh);
    }
}
