using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class ElementalBlizzardEffect : MonoBehaviour
{
    private class EnemyContact
    {
        public readonly HashSet<Collider2D> Colliders = new();
        public float TimeInside;
        public float LastUpdateTime;
        public float NextDamageTime;
        public float NextFreezeAttemptTime;
    }

    private readonly Dictionary<BaseEnemy, EnemyContact> m_contacts = new();
    private readonly List<BaseEnemy> m_staleEnemies = new();
    private Player m_player;
    private ElementalSpellBehavior m_behavior;
    private SpriteRenderer m_renderer;
    private float m_damage;
    private float m_remaining;
    private float m_radius;
    private float m_damageInterval;
    private bool m_isConfigured;

    public void Configure(Player player, int damage, ElementalSpellBehavior behavior,
        Sprite sprite, Material material, Color color, float radius,
        int sortingLayerId, int sortingOrder)
    {
        if (player == null || behavior == null || sprite == null || material == null ||
            damage <= 0 || radius <= 0f || behavior.ActiveDuration <= 0f ||
            behavior.DamageTickInterval <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        m_player = player;
        transform.position = player.transform.position;
        m_behavior = behavior;
        m_damage = damage;
        m_radius = radius;
        m_damageInterval = behavior.DamageTickInterval;
        m_renderer = GetComponent<SpriteRenderer>();
        m_renderer.sprite = sprite;
        m_renderer.sharedMaterial = material;
        m_renderer.color = new Color(color.r, color.g, color.b, 0.32f);
        m_renderer.sortingLayerID = sortingLayerId;
        m_renderer.sortingOrder = sortingOrder;
        transform.localScale = new Vector3(2f * radius / sprite.bounds.size.x,
            2f * radius / sprite.bounds.size.y, 1f);

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        circle.isTrigger = true;
        circle.radius = 0.5f;

        if (!m_isConfigured)
            m_remaining = behavior.ActiveDuration;
        m_isConfigured = true;
    }

    private void LateUpdate()
    {
        if (!m_isConfigured || m_player == null || !m_player.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = m_player.transform.position;
        m_remaining -= Time.deltaTime;
        if (m_remaining <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RegisterContact(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        BaseEnemy enemy = RegisterContact(other);
        if (enemy == null || !enemy.IsAlive) return;
        EnemyContact contact = m_contacts[enemy];

        float deltaTime = Mathf.Max(0f, Time.time - contact.LastUpdateTime);
        contact.LastUpdateTime = Time.time;
        contact.TimeInside += deltaTime;

        if (Time.time >= contact.NextDamageTime)
        {
            contact.NextDamageTime = Time.time + m_damageInterval;
            int damage = Mathf.Max(1,
                Mathf.RoundToInt(m_damage * m_behavior.AreaDamageMultiplier));
            enemy.TakeDamage(damage);
        }

        if (enemy.IsAlive && Time.time >= contact.NextFreezeAttemptTime)
        {
            contact.NextFreezeAttemptTime = Time.time + m_behavior.BlizzardFreezeAttemptInterval;
            float chance = Mathf.Min(m_behavior.BlizzardFreezeMaximumChance,
                m_behavior.BlizzardFreezeInitialChance +
                contact.TimeInside * m_behavior.BlizzardFreezeChanceIncreasePerSecond);
            if (Random.value < chance)
                enemy.ApplyFreeze(m_behavior.BlizzardFreezeDuration);
        }
    }

    private BaseEnemy RegisterContact(Collider2D other)
    {
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null || !enemy.IsAlive) return null;
        if (!m_contacts.TryGetValue(enemy, out EnemyContact contact))
        {
            contact = new EnemyContact
            {
                LastUpdateTime = Time.time,
                NextDamageTime = Time.time,
                NextFreezeAttemptTime = Time.time
            };
            m_contacts.Add(enemy, contact);
        }
        contact.Colliders.Add(other);
        return enemy;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null || !m_contacts.TryGetValue(enemy, out EnemyContact contact)) return;
        contact.Colliders.Remove(other);
        if (contact.Colliders.Count == 0)
            m_contacts.Remove(enemy);
    }

    private void Update()
    {
        m_staleEnemies.Clear();
        foreach (BaseEnemy enemy in m_contacts.Keys)
        {
            if (enemy == null || !enemy.IsAlive)
                m_staleEnemies.Add(enemy);
        }
        foreach (BaseEnemy enemy in m_staleEnemies)
            m_contacts.Remove(enemy);
    }
}
