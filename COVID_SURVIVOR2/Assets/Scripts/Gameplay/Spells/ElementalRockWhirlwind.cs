using System.Collections.Generic;
using UnityEngine;

// Move o centro em direção ao centro do grupo mais próximo e organiza as pedras em órbita.
public class ElementalRockWhirlwind : MonoBehaviour
{
    private readonly List<ElementalOrbitingRock> m_rocks = new();
    private Player m_player;
    private ElementalSpellBehavior m_behavior;
    private Vector2 m_direction;
    private Vector2 m_groupPosition;
    private float m_remaining;
    private float m_nextSearchTime;
    private float m_angle;
    private float m_speed;
    private int m_damage;
    private float m_sizeMultiplier = 1f;
    private float m_rangeBonus;
    private bool m_hasGroup;
    private bool m_isConfigured;

    public Vector2 Center => transform.position;
    public float OrbitRadius => m_behavior == null ? 1.2f :
        m_behavior.RockWhirlwindOrbitRadius * m_sizeMultiplier + m_rangeBonus;
    public float RotationSpeed => m_behavior == null ? 540f : m_behavior.RockWhirlwindRotationSpeed;

    public void Configure(Player player, Vector2 direction, int damage, float speed,
        int rockCount, ElementalSpellBehavior behavior, Sprite sprite, Material material,
        Color color, int sortingLayerId, int sortingOrder, float sizeMultiplier, float rangeBonus)
    {
        if (player == null || behavior == null || sprite == null || material == null ||
            direction.sqrMagnitude <= 0.0001f || damage <= 0 || speed <= 0f || rockCount < 1 ||
            sizeMultiplier <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        m_player = player;
        m_behavior = behavior;
        m_direction = direction.normalized;
        m_damage = damage;
        m_speed = speed;
        m_sizeMultiplier = sizeMultiplier;
        m_rangeBonus = rangeBonus;
        m_remaining = behavior.ActiveDuration;
        if (m_remaining <= 0f || behavior.RockWhirlwindOrbitRadius <= 0f ||
            behavior.RockWhirlwindDamageInterval <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        SpriteRenderer centerRenderer = gameObject.AddComponent<SpriteRenderer>();
        centerRenderer.sprite = sprite;
        centerRenderer.sharedMaterial = material;
        centerRenderer.color = new Color(color.r, color.g, color.b, 0.25f);
        centerRenderer.sortingLayerID = sortingLayerId;
        centerRenderer.sortingOrder = sortingOrder;
        transform.localScale = Vector3.one * 0.7f;

        for (int i = 0; i < rockCount; i++)
        {
            GameObject rockObject = new GameObject("OrbitingRock");
            rockObject.transform.SetParent(transform, false);
            ElementalOrbitingRock rock = rockObject.AddComponent<ElementalOrbitingRock>();
            rock.Configure(this, m_damage, behavior.RockWhirlwindDamageMultiplier,
                behavior.RockWhirlwindDamageInterval, sprite, material, color,
                sortingLayerId, sortingOrder + 1, 0.42f * m_sizeMultiplier);
            m_rocks.Add(rock);
        }

        m_isConfigured = true;
    }

    private void Update()
    {
        if (!m_isConfigured || m_player == null || !m_player.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        m_remaining -= Time.deltaTime;
        if (m_remaining <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time >= m_nextSearchTime)
        {
            m_nextSearchTime = Time.time + m_behavior.VortexTargetSearchInterval;
            FindNearestGroup();
        }

        if (m_hasGroup && (m_groupPosition - (Vector2)transform.position).sqrMagnitude > 0.04f)
            m_direction = ((Vector2)m_groupPosition - (Vector2)transform.position).normalized;

        transform.position = (Vector2)transform.position +
            m_direction * m_speed * Time.deltaTime;
        m_angle += m_behavior.RockWhirlwindRotationSpeed * Time.deltaTime;

        for (int i = m_rocks.Count - 1; i >= 0; i--)
        {
            if (m_rocks[i] == null) m_rocks.RemoveAt(i);
            else m_rocks[i].UpdateOrbit(m_angle + 360f * i / Mathf.Max(1, m_rocks.Count),
                m_behavior.RockWhirlwindOrbitRadius);
        }
    }

    private void FindNearestGroup()
    {
        BaseEnemy closest = null;
        float closestDistance = float.PositiveInfinity;
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>();
        foreach (BaseEnemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;
            float distance = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (distance >= closestDistance) continue;
            closestDistance = distance;
            closest = enemy;
        }

        if (closest == null)
        {
            m_hasGroup = false;
            return;
        }
        Vector2 groupCenter = Vector2.zero;
        int groupCount = 0;
        float groupRadiusSquared = m_behavior.RockWhirlwindGroupRadius *
                                   m_behavior.RockWhirlwindGroupRadius;
        foreach (BaseEnemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive ||
                ((Vector2)enemy.transform.position - (Vector2)closest.transform.position).sqrMagnitude > groupRadiusSquared)
                continue;
            groupCenter += (Vector2)enemy.transform.position;
            groupCount++;
        }

        if (groupCount > 0)
        {
            m_groupPosition = groupCenter / groupCount;
            m_hasGroup = true;
        }
    }
}
