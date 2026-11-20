using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class VaccineProjectile : MonoBehaviour
{
    private Rigidbody2D m_rigidbody;
    private Vector2 m_direction;
    private int m_damage;
    private float m_speed;
    private float m_lifetime;
    private bool m_isInitialized;
    private bool m_hasHit;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_rigidbody.gravityScale = 0f;
        m_rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    public void Initialize(Vector2 direction, int damage, float speed, float lifetime)
    {
        if (direction == Vector2.zero || damage <= 0 || speed <= 0f || lifetime <= 0f)
        {
            Debug.LogError("Direção, dano, velocidade e tempo de vida do projétil devem ser válidos.", this);
            Destroy(gameObject);
            return;
        }

        m_direction = direction.normalized;
        m_damage = damage;
        m_speed = speed;
        m_lifetime = lifetime;
        m_isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!m_isInitialized || m_hasHit) return;

        m_rigidbody.MovePosition(
            m_rigidbody.position + m_direction * m_speed * Time.fixedDeltaTime);

        m_lifetime -= Time.fixedDeltaTime;
        if (m_lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!m_isInitialized || m_hasHit) return;

        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null || !enemy.IsAlive) return;

        m_hasHit = true;
        enemy.TakeDamage(m_damage);
        Destroy(gameObject);
    }
}
