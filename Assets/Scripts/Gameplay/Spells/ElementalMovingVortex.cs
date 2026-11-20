using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class ElementalMovingVortex : MonoBehaviour
{
    private readonly Collider2D[] m_overlapResults = new Collider2D[128];
    private readonly Dictionary<BaseEnemy, float> m_nextDamageTimes = new();
    private readonly HashSet<BaseEnemy> m_attractedEnemies = new();
    private readonly HashSet<BaseEnemy> m_attractedThisStep = new();
    private readonly List<BaseEnemy> m_staleEnemies = new();
    private ContactFilter2D m_contactFilter;
    private Rigidbody2D m_rigidbody;
    private CircleCollider2D m_collider;
    private SpriteRenderer m_renderer;
    private Player m_player;
    private BaseEnemy m_target;
    private ElementalSpellBehavior m_behavior;
    private Vector2 m_direction;
    private float m_damage;
    private float m_movementSpeed;
    private float m_damageInterval;
    private float m_searchRange;
    private float m_attractionRadius;
    private float m_remaining;
    private float m_elapsed;
    private float m_nextTargetSearch;
    private float m_initialRadius;
    private float m_maximumRadius;
    private bool m_isTornado;
    private bool m_isConfigured;

    public bool IsActive => m_isConfigured && m_remaining > 0f;

    public void Configure(Player player, BaseEnemy target, Vector2 direction, int damage,
        float movementSpeed, float searchRange, ElementalSpellBehavior behavior,
        Sprite sprite, Material material, Color color, int sortingLayerId, int sortingOrder,
        float sizeMultiplier, float attractionRadius)
    {
        if (player == null || behavior == null || sprite == null || material == null ||
            direction.sqrMagnitude <= 0.0001f || damage <= 0 || movementSpeed <= 0f ||
            behavior.ActiveDuration <= 0f || behavior.AreaRadius <= 0f ||
            behavior.DamageTickInterval <= 0f || attractionRadius <= 0f ||
            sizeMultiplier <= 0f || (behavior.AttackMode == ElementalAttackMode.Tornado &&
            behavior.TornadoGrowthDuration <= 0f))
        {
            Destroy(gameObject);
            return;
        }

        m_player = player;
        m_target = target;
        m_direction = direction.normalized;
        m_damage = damage;
        m_movementSpeed = movementSpeed;
        m_searchRange = Mathf.Max(0f, searchRange);
        m_attractionRadius = attractionRadius;
        m_damageInterval = behavior.DamageTickInterval;
        m_behavior = behavior;
        m_isTornado = behavior.AttackMode == ElementalAttackMode.Tornado;
        m_remaining = behavior.ActiveDuration;
        m_initialRadius = (m_isTornado ? behavior.TornadoInitialRadius : behavior.AreaRadius) * sizeMultiplier;
        m_maximumRadius = (m_isTornado ? behavior.TornadoMaximumRadius : behavior.AreaRadius) * sizeMultiplier;
        if (m_maximumRadius < m_initialRadius)
            m_maximumRadius = m_initialRadius;

        m_rigidbody = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<CircleCollider2D>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_rigidbody.gravityScale = 0f;
        m_rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        m_collider.isTrigger = true;
        m_collider.radius = 0.5f;
        m_renderer.sprite = sprite;
        m_renderer.sharedMaterial = material;
        m_renderer.color = new Color(color.r, color.g, color.b, 0.55f);
        m_renderer.sortingLayerID = sortingLayerId;
        m_renderer.sortingOrder = sortingOrder;
        transform.rotation = Quaternion.identity;

        m_contactFilter.useTriggers = true;
        m_contactFilter.useLayerMask = false;
        m_isConfigured = true;
        SetVisualRadius(m_isTornado ? m_initialRadius : m_maximumRadius);
        if (m_isTornado)
            m_renderer.color = new Color(color.r, color.g, color.b, 0.72f);
    }

    private void Update()
    {
        if (!m_isConfigured || m_player == null || !m_player.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        m_elapsed += Time.deltaTime;
        m_remaining -= Time.deltaTime;
        if (m_isTornado)
        {
            float growth = Mathf.Clamp01(m_elapsed / m_behavior.TornadoGrowthDuration);
            SetVisualRadius(Mathf.Lerp(m_initialRadius, m_maximumRadius, growth));
        }

        transform.Rotate(0f, 0f, 100f * Time.deltaTime);
        if (m_remaining <= 0f)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (!m_isConfigured) return;

        if (m_isTornado)
            UpdateTornadoDirection();

        m_rigidbody.MovePosition(m_rigidbody.position + m_direction *
            m_movementSpeed * Time.fixedDeltaTime);
        UpdateAttraction();
    }

    private void UpdateTornadoDirection()
    {
        if (Time.time < m_nextTargetSearch && m_target != null && m_target.IsAlive) return;
        m_nextTargetSearch = Time.time + Mathf.Max(0.05f, m_behavior.VortexTargetSearchInterval);
        m_target = FindClosestEnemy();
        if (m_target == null) return;

        Vector2 toTarget = (Vector2)m_target.transform.position - m_rigidbody.position;
        if (toTarget.sqrMagnitude > 0.0001f)
            m_direction = Vector2.MoveTowards(m_direction, toTarget.normalized,
                4f * Time.fixedDeltaTime).normalized;
    }

    private BaseEnemy FindClosestEnemy()
    {
        BaseEnemy closest = null;
        float closestDistance = float.PositiveInfinity;
        foreach (BaseEnemy enemy in FindObjectsByType<BaseEnemy>())
        {
            if (enemy == null || !enemy.IsAlive) continue;
            float distance = ((Vector2)enemy.transform.position - m_rigidbody.position).sqrMagnitude;
            if (distance > m_searchRange * m_searchRange) continue;
            if (distance >= closestDistance) continue;
            closestDistance = distance;
            closest = enemy;
        }
        return closest;
    }

    private void SetVisualRadius(float radius)
    {
        Vector2 spriteSize = m_renderer.sprite.bounds.size;
        transform.localScale = new Vector3(2f * radius / spriteSize.x,
            2f * radius / spriteSize.y, 1f);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null || !enemy.IsAlive) return;
        if (m_nextDamageTimes.TryGetValue(enemy, out float nextTime) && Time.time < nextTime)
            return;

        m_nextDamageTimes[enemy] = Time.time + m_damageInterval;
        int damage = Mathf.Max(1, Mathf.RoundToInt(m_damage * m_behavior.VortexDamageMultiplier));
        enemy.TakeDamage(damage);
    }

    private void UpdateAttraction()
    {
        m_attractedThisStep.Clear();
        int count = Physics2D.OverlapCircle(m_rigidbody.position,
            m_attractionRadius, m_contactFilter, m_overlapResults);

        for (int i = 0; i < count; i++)
        {
            Collider2D overlap = m_overlapResults[i];
            m_overlapResults[i] = null;
            if (overlap == null) continue;

            BaseEnemy enemy = overlap.GetComponentInParent<BaseEnemy>();
            if (enemy == null || !enemy.IsAlive) continue;
            Vector2 difference = m_rigidbody.position - (Vector2)enemy.transform.position;
            if (difference.sqrMagnitude <= 0.0001f) continue;

            enemy.SetAttraction(this, m_behavior.VortexAttractionSpeed);
            m_attractedThisStep.Add(enemy);
        }

        foreach (BaseEnemy enemy in m_attractedEnemies)
        {
            if (enemy == null || !m_attractedThisStep.Contains(enemy))
                m_staleEnemies.Add(enemy);
        }

        foreach (BaseEnemy enemy in m_staleEnemies)
        {
            if (enemy != null) enemy.RemoveAttraction(this);
            m_attractedEnemies.Remove(enemy);
        }
        m_staleEnemies.Clear();

        foreach (BaseEnemy enemy in m_attractedThisStep)
            m_attractedEnemies.Add(enemy);
    }

    private void OnDestroy()
    {
        foreach (BaseEnemy enemy in m_attractedEnemies)
        {
            if (enemy != null) enemy.RemoveAttraction(this);
        }
    }
}
