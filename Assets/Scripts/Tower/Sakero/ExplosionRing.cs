
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExplosionRing : MonoBehaviour
{
    [Header("Ring Settings")]
    [SerializeField] private float ringThickness = 0.2f;
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float startRadius = 0.1f;

    [Header("Pixel Settings")]
    [SerializeField] private float pixelSize = 0.15f;

    [Header("Visual")]
    [SerializeField] private Color ringColor =
        new Color(1f, 0.65f, 0.15f, 1f);

    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();

    private float elapsed;
    private float targetRadius;
    private bool isPlaying;
    public float Duration => duration;
    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Pixel Explosion Ring";

        GetComponent<MeshFilter>().mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();

        UpdateColor(1f);
        GenerateRing(startRadius);

        meshRenderer.enabled = false;
    }

    public void Play(float explosionRadius)
    {
        targetRadius = explosionRadius;
        elapsed = 0f;
        isPlaying = true;

        meshRenderer.enabled = true;

        GenerateRing(startRadius);
        UpdateColor(1f);
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        elapsed += Time.deltaTime;

        float t = Mathf.Clamp01(elapsed / duration);

        float radius = Mathf.Lerp(
            startRadius,
            targetRadius,
            t
        );

        GenerateRing(radius);
        UpdateColor(1f - t);

        if (t >= 1f)
        {
            isPlaying = false;
            meshRenderer.enabled = false;
        }
    }

    private void GenerateRing(float radius)
    {
        vertices.Clear();
        triangles.Clear();

        float pixel = Mathf.Max(0.01f, pixelSize);
        float innerRadius = Mathf.Max(0f, radius - ringThickness);

        int gridRadius = Mathf.CeilToInt(radius / pixel);

        for (int x = -gridRadius; x < gridRadius; x++)
        {
            for (int y = -gridRadius; y < gridRadius; y++)
            {
                float centerX = (x + 0.5f) * pixel;
                float centerY = (y + 0.5f) * pixel;

                float distance = Mathf.Sqrt(
                    centerX * centerX +
                    centerY * centerY
                );

                if (distance > radius || distance < innerRadius)
                    continue;

                AddPixel(x * pixel, y * pixel, pixel);
            }
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
    }

    private void AddPixel(float x, float y, float size)
    {
        int index = vertices.Count;

        vertices.Add(new Vector3(x, y, 0f));
        vertices.Add(new Vector3(x + size, y, 0f));
        vertices.Add(new Vector3(x + size, y + size, 0f));
        vertices.Add(new Vector3(x, y + size, 0f));

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }

    private void UpdateColor(float alpha)
    {
        meshRenderer.GetPropertyBlock(propertyBlock);

        Color color = ringColor;
        color.a *= alpha;

        propertyBlock.SetColor("_Color", color);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}
