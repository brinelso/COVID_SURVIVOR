using UnityEngine;
using UnityEngine.Rendering;

// Cria a forma provisória do cone sem misturar sua apresentação com o cálculo do dano.
public static class ElementalConeVisual
{
    public static void Show(Vector2 origin, Vector2 direction, float length, float angle,
        float duration, Material material, int sortingLayerId, int sortingOrder)
    {
        if (length <= 0f || angle <= 0f || duration <= 0f || material == null)
            return;

        GameObject visual = new GameObject("SteamConeVisual");
        visual.transform.position = origin;
        visual.transform.rotation = Quaternion.Euler(
            0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        Mesh mesh = CreateConeMesh(length, angle);
        MeshFilter meshFilter = visual.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshRenderer meshRenderer = visual.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.sortingLayerID = sortingLayerId;
        meshRenderer.sortingOrder = sortingOrder;

        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(properties);
        properties.SetTexture("_MainTex", Texture2D.whiteTexture);
        meshRenderer.SetPropertyBlock(properties);

        Object.Destroy(mesh, duration);
        Object.Destroy(visual, duration);
    }

    private static Mesh CreateConeMesh(float length, float angle)
    {
        const int segments = 16;
        float halfAngle = Mathf.Clamp(angle, 1f, 120f) * 0.5f;
        Vector3[] vertices = new Vector3[segments + 2];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        colors[0] = new Color(1f, 0.97f, 0.87f, 0.62f);

        for (int i = 0; i <= segments; i++)
        {
            float normalized = i / (float)segments;
            float radians = Mathf.Lerp(-halfAngle, halfAngle, normalized) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * length;
            colors[i + 1] = Color.Lerp(
                new Color(1f, 0.97f, 0.87f, 0.62f),
                new Color(1f, 0.48f, 0.2f, 0.48f),
                Mathf.Abs(normalized - 0.5f) * 2f);
        }

        for (int i = 0; i < segments; i++)
        {
            int triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i + 2;
        }

        Mesh mesh = new Mesh { name = "TemporarySteamCone" };
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }
}
