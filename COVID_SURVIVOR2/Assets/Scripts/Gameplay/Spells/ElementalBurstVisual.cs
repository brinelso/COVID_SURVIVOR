using UnityEngine;

// Apresenta uma explosão circular simples e temporária, independente do dano aplicado.
public class ElementalBurstVisual : MonoBehaviour
{
    private SpriteRenderer m_spriteRenderer;
    private Vector3 m_finalScale;
    private Color m_color;
    private float m_duration;
    private float m_elapsed;

    public static void Show(Sprite sprite, Material material, Vector2 position, float radius,
        float duration, Color color, int sortingLayerId, int sortingOrder)
    {
        if (sprite == null || material == null || radius <= 0f || duration <= 0f)
            return;

        GameObject visual = new GameObject("ElementalBurstVisual");
        visual.transform.position = position;
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.sortingLayerID = sortingLayerId;
        renderer.sortingOrder = sortingOrder;

        ElementalBurstVisual effect = visual.AddComponent<ElementalBurstVisual>();
        effect.Initialize(radius, duration, color);
    }

    public static void ShowLine(Sprite sprite, Material material, Vector2 start, Vector2 end,
        float width, float duration, Color color, int sortingLayerId, int sortingOrder)
    {
        if (sprite == null || material == null || width <= 0f || duration <= 0f || start == end)
            return;

        GameObject visual = new GameObject("ElementalFissureVisual");
        LineRenderer line = visual.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = width;
        line.endWidth = width * 0.45f;
        line.startColor = color;
        Color fadedColor = color;
        fadedColor.a = 0f;
        line.endColor = fadedColor;
        line.sharedMaterial = material;
        line.sortingLayerID = sortingLayerId;
        line.sortingOrder = sortingOrder;
        Object.Destroy(visual, duration);
    }

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Initialize(float radius, float duration, Color color)
    {
        Vector2 spriteSize = m_spriteRenderer.sprite.bounds.size;
        m_finalScale = new Vector3(radius * 2f / spriteSize.x, radius * 2f / spriteSize.y, 1f);
        m_duration = duration;
        m_color = color;
        transform.localScale = m_finalScale * 0.25f;
        m_spriteRenderer.color = color;
    }

    private void Update()
    {
        m_elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(m_elapsed / m_duration);
        transform.localScale = m_finalScale * Mathf.Lerp(0.25f, 1f, progress);

        Color currentColor = m_color;
        currentColor.a = m_color.a * (1f - progress);
        m_spriteRenderer.color = currentColor;

        if (m_elapsed >= m_duration)
            Destroy(gameObject);
    }
}
