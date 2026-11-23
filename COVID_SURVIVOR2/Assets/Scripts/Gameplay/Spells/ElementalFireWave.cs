using System.Collections.Generic;
using UnityEngine;

// Uma onda estacionária que expande a partir do local onde o Inferno a criou.
public class ElementalFireWave : MonoBehaviour
{
    private const int SegmentCount = 40;
    private readonly HashSet<BaseEnemy> m_hitEnemies = new();
    private readonly List<BaseEnemy> m_staleEnemies = new();
    private LineRenderer m_lineRenderer;
    private Vector2 m_center;
    private float m_maxRadius;
    private float m_expansionSpeed;
    private float m_previousRadius;
    private int m_damage;
    private Color m_color;

    public void Initialize(Vector2 center, float maxRadius, float expansionSpeed,
        float lineWidth, int damage, Color color, Material material,
        int sortingLayerId, int sortingOrder)
    {
        if (maxRadius <= 0f || expansionSpeed <= 0f || lineWidth <= 0f || material == null)
        {
            Destroy(gameObject);
            return;
        }

        m_center = center;
        m_maxRadius = maxRadius;
        m_expansionSpeed = expansionSpeed;
        m_damage = damage;
        m_color = color;
        m_lineRenderer = gameObject.AddComponent<LineRenderer>();
        m_lineRenderer.useWorldSpace = true;
        m_lineRenderer.loop = true;
        m_lineRenderer.positionCount = SegmentCount;
        m_lineRenderer.startWidth = lineWidth;
        m_lineRenderer.endWidth = lineWidth;
        m_lineRenderer.startColor = color;
        m_lineRenderer.endColor = color;
        m_lineRenderer.sharedMaterial = material;
        m_lineRenderer.sortingLayerID = sortingLayerId;
        m_lineRenderer.sortingOrder = sortingOrder;
        UpdateRing(0.05f);
    }

    private void Update()
    {
        float radius = Mathf.Min(m_maxRadius, m_previousRadius + m_expansionSpeed * Time.deltaTime);
        DamageCrossedEnemies(m_previousRadius, radius);
        m_previousRadius = radius;
        UpdateRing(radius);

        if (radius >= m_maxRadius)
            Destroy(gameObject);
    }

    private void DamageCrossedEnemies(float previousRadius, float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_center, radius);
        foreach (Collider2D collider in colliders)
        {
            BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
            if (enemy == null || !enemy.IsAlive || m_hitEnemies.Contains(enemy)) continue;

            float distance = ((Vector2)enemy.transform.position - m_center).magnitude;
            if (distance <= previousRadius || distance > radius) continue;

            m_hitEnemies.Add(enemy);
            enemy.TakeDamage(m_damage);
        }

        m_staleEnemies.Clear();
        foreach (BaseEnemy enemy in m_hitEnemies)
        {
            if (enemy == null || !enemy.IsAlive)
                m_staleEnemies.Add(enemy);
        }
        foreach (BaseEnemy enemy in m_staleEnemies)
            m_hitEnemies.Remove(enemy);
    }

    private void UpdateRing(float radius)
    {
        for (int i = 0; i < SegmentCount; i++)
        {
            float angle = i * Mathf.PI * 2f / SegmentCount;
            Vector2 point = m_center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            m_lineRenderer.SetPosition(i, point);
        }

        Color faded = m_color;
        faded.a = Mathf.Lerp(m_color.a, 0f, radius / m_maxRadius);
        m_lineRenderer.startColor = faded;
        m_lineRenderer.endColor = faded;
    }
}
