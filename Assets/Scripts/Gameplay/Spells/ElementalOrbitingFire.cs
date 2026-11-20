using System.Collections.Generic;
using UnityEngine;

// Cria e mantém tornadozinhos provisórios distribuídos ao redor do médico.
public class ElementalOrbitingFire : MonoBehaviour
{
    private class Orbiter
    {
        public Transform Transform;
        public SpriteRenderer Renderer;
        public float NextPulseTime;
        public readonly Dictionary<BaseEnemy, float> NextDamageTimes = new();
    }

    private readonly Collider2D[] m_overlapResults = new Collider2D[128];
    private readonly HashSet<BaseEnemy> m_damagedThisPulse = new();
    private ContactFilter2D m_contactFilter;
    private readonly List<Orbiter> m_orbiters = new();
    private readonly List<BaseEnemy> m_staleEnemies = new();
    private Player m_player;
    private int m_damage;
    private float m_radius;
    private float m_rotationSpeed;
    private float m_tickInterval;
    private float m_damageMultiplier;
    private float m_size;
    private Color m_color;
    private Sprite m_sprite;
    private Material m_material;
    private int m_sortingLayerId;
    private int m_sortingOrder;
    private bool m_isConfigured;

    public void Configure(Player player, int damage, int count, float radius,
        float rotationSpeed, float tickInterval, float damageMultiplier, float size,
        Color color, Sprite sprite, Material material, int sortingLayerId, int sortingOrder)
    {
        if (player == null || sprite == null || material == null || count < 1 ||
            radius <= 0f || tickInterval <= 0f || size <= 0f)
            return;

        m_player = player;
        m_damage = damage;
        m_radius = radius;
        m_rotationSpeed = rotationSpeed;
        m_tickInterval = tickInterval;
        m_damageMultiplier = damageMultiplier;
        m_size = size;
        m_color = color;
        m_sprite = sprite;
        m_material = material;
        m_sortingLayerId = sortingLayerId;
        m_sortingOrder = sortingOrder;

        while (m_orbiters.Count < count)
            AddOrbiter();
        while (m_orbiters.Count > count)
        {
            int lastIndex = m_orbiters.Count - 1;
            if (m_orbiters[lastIndex].Transform != null)
                Destroy(m_orbiters[lastIndex].Transform.gameObject);
            m_orbiters.RemoveAt(lastIndex);
        }

        foreach (Orbiter orbiter in m_orbiters)
        {
            if (orbiter.Transform == null) continue;
            orbiter.Transform.localScale = new Vector3(m_size, m_size * 0.6f, 1f);
            orbiter.Renderer.color = m_color;
            orbiter.Renderer.sharedMaterial = m_material;
            orbiter.Renderer.sortingLayerID = m_sortingLayerId;
            orbiter.Renderer.sortingOrder = m_sortingOrder;
        }

        m_isConfigured = true;
        m_contactFilter.useTriggers = true;
        m_contactFilter.useLayerMask = false;
    }

    private void Update()
    {
        if (!m_isConfigured || m_player == null || !m_player.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < m_orbiters.Count; i++)
        {
            Orbiter orbiter = m_orbiters[i];
            if (orbiter.Transform == null) continue;

            float phase = Mathf.PI * 2f * i / m_orbiters.Count;
            float angle = phase + Time.time * m_rotationSpeed * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * m_radius;
            orbiter.Transform.position = (Vector2)m_player.transform.position + offset;
            orbiter.Transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);

            if (Time.time >= orbiter.NextPulseTime)
            {
                orbiter.NextPulseTime = Time.time + m_tickInterval;
                DamageEnemiesAt(orbiter);
            }
        }
    }

    private void AddOrbiter()
    {
        GameObject visual = new GameObject("OrbitingFireTornado");
        visual.transform.SetParent(transform, false);
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = m_sprite;
        renderer.sharedMaterial = m_material;
        renderer.color = m_color;
        renderer.sortingLayerID = m_sortingLayerId;
        renderer.sortingOrder = m_sortingOrder;
        visual.transform.localScale = new Vector3(m_size, m_size * 0.6f, 1f);

        m_orbiters.Add(new Orbiter { Transform = visual.transform, Renderer = renderer });
    }

    private void DamageEnemiesAt(Orbiter orbiter)
    {
        Vector2 center = orbiter.Transform.position;
        int colliderCount = Physics2D.OverlapCircle(
            center, m_size * 0.5f, m_contactFilter, m_overlapResults);
        m_damagedThisPulse.Clear();
        int damage = Mathf.Max(1, Mathf.RoundToInt(m_damage * m_damageMultiplier));

        for (int i = 0; i < colliderCount; i++)
        {
            Collider2D collider = m_overlapResults[i];
            m_overlapResults[i] = null;
            if (collider == null) continue;

            BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
            if (enemy == null || !enemy.IsAlive || !m_damagedThisPulse.Add(enemy)) continue;
            if (orbiter.NextDamageTimes.TryGetValue(enemy, out float nextTime) && Time.time < nextTime)
                continue;

            orbiter.NextDamageTimes[enemy] = Time.time + m_tickInterval;
            enemy.TakeDamage(damage);
        }

        m_staleEnemies.Clear();
        foreach (BaseEnemy enemy in orbiter.NextDamageTimes.Keys)
        {
            if (enemy == null || !enemy.IsAlive)
                m_staleEnemies.Add(enemy);
        }

        foreach (BaseEnemy enemy in m_staleEnemies)
            orbiter.NextDamageTimes.Remove(enemy);
    }
}
