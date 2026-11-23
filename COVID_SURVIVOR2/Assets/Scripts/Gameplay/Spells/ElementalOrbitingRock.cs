using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class ElementalOrbitingRock : MonoBehaviour
{
    private readonly Dictionary<BaseEnemy, float> m_nextDamageTimes = new();
    private ElementalRockWhirlwind m_owner;
    private SpriteRenderer m_renderer;
    private int m_damage;
    private float m_damageMultiplier;
    private float m_damageInterval;
    private float m_size;
    private bool m_isConfigured;

    public void Configure(ElementalRockWhirlwind owner, int damage, float damageMultiplier,
        float damageInterval, Sprite sprite, Material material, Color color,
        int sortingLayerId, int sortingOrder, float size)
    {
        m_owner = owner;
        m_damage = damage;
        m_damageMultiplier = damageMultiplier;
        m_damageInterval = damageInterval;
        m_size = size;
        m_renderer = GetComponent<SpriteRenderer>();
        m_renderer.sprite = sprite;
        m_renderer.sharedMaterial = material;
        m_renderer.color = color;
        m_renderer.sortingLayerID = sortingLayerId;
        m_renderer.sortingOrder = sortingOrder;
        transform.localScale = Vector3.one * size;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        CircleCollider2D hitbox = GetComponent<CircleCollider2D>();
        hitbox.isTrigger = true;
        m_isConfigured = true;
    }

    public void UpdateOrbit(float angleDegrees, float radius)
    {
        if (!m_isConfigured || m_owner == null) return;
        float angle = angleDegrees * Mathf.Deg2Rad;
        transform.position = m_owner.Center +
            new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null || !enemy.IsAlive) return;
        if (m_nextDamageTimes.TryGetValue(enemy, out float nextTime) && Time.time < nextTime)
            return;

        m_nextDamageTimes[enemy] = Time.time + m_damageInterval;
        enemy.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(m_damage * m_damageMultiplier)));
    }

    private void OnDestroy()
    {
        m_nextDamageTimes.Clear();
    }
}
