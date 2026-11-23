using UnityEngine;
using System.Collections.Generic;

// Projétil provisório pequeno criado por rochas e erupções elementais.
public class ElementalFragment : MonoBehaviour
{
    private Rigidbody2D m_rigidbody;
    private CircleCollider2D m_collider;
    private Vector2 m_direction;
    private float m_speed;
    private int m_damage;
    private bool m_initialized;
    private HashSet<BaseEnemy> m_activationHitEnemies;

    public static void Spawn(Vector2 position, Vector2 direction, int damage, float speed,
        float lifetime, float size, Color color, Sprite sprite, Material material,
        int sortingLayerId, int sortingOrder, HashSet<BaseEnemy> activationHitEnemies)
    {
        if (direction == Vector2.zero || damage <= 0 || speed <= 0f || lifetime <= 0f ||
            size <= 0f || sprite == null || material == null)
            return;

        GameObject fragment = new GameObject("ElementalFragment");
        fragment.transform.position = position;
        fragment.transform.localScale = Vector3.one * size;

        SpriteRenderer renderer = fragment.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.color = color;
        renderer.sortingLayerID = sortingLayerId;
        renderer.sortingOrder = sortingOrder;

        ElementalFragment behaviour = fragment.AddComponent<ElementalFragment>();
        behaviour.Initialize(direction, damage, speed, lifetime, activationHitEnemies);
    }

    private void Awake()
    {
        m_rigidbody = gameObject.AddComponent<Rigidbody2D>();
        m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_rigidbody.gravityScale = 0f;
        m_rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        m_collider = gameObject.AddComponent<CircleCollider2D>();
        m_collider.isTrigger = true;
        m_collider.radius = 0.5f;
    }

    private void Initialize(Vector2 direction, int damage, float speed, float lifetime,
        HashSet<BaseEnemy> activationHitEnemies)
    {
        m_direction = direction.normalized;
        m_damage = damage;
        m_speed = speed;
        m_activationHitEnemies = activationHitEnemies;
        m_initialized = true;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!m_initialized) return;

        m_rigidbody.MovePosition(
            m_rigidbody.position + m_direction * m_speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null || !enemy.IsAlive) return;
        if (m_activationHitEnemies != null && !m_activationHitEnemies.Add(enemy))
        {
            Destroy(gameObject);
            return;
        }

        enemy.TakeDamage(m_damage);
        Destroy(gameObject);
    }
}
