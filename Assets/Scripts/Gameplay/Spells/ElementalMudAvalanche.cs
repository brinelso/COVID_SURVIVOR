using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class ElementalMudAvalanche : MonoBehaviour
{
    private readonly HashSet<BaseEnemy> m_hitEnemies = new();
    private Rigidbody2D m_rigidbody;
    private BoxCollider2D m_collider;
    private SpriteRenderer m_renderer;
    private ElementalMudTrail m_trail;
    private ElementalSpellBehavior m_behavior;
    private Vector2 m_direction;
    private int m_damage;
    private float m_speed;
    private float m_distanceRemaining;
    private float m_distanceUntilNextTrail;
    private bool m_isInitialized;
    private const float TrailSpacing = 0.45f;

    public void Initialize(Vector2 direction, int damage, float speed,
        ElementalSpellBehavior behavior, Sprite sprite, Material material,
        int sortingLayerId, int sortingOrder, float sizeMultiplier)
    {
        if (direction.sqrMagnitude <= 0.0001f || damage <= 0 || speed <= 0f ||
            behavior == null || sprite == null || material == null ||
            behavior.MudAvalancheWidth <= 0f || behavior.MudAvalancheLength <= 0f ||
            behavior.MudAvalancheTravelDistance <= 0f || behavior.MudTrailDuration <= 0f ||
            sizeMultiplier <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        m_rigidbody = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<BoxCollider2D>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_behavior = behavior;
        m_direction = direction.normalized;
        m_damage = damage;
        m_speed = speed;
        m_distanceRemaining = behavior.MudAvalancheTravelDistance;

        m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_rigidbody.gravityScale = 0f;
        m_rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        m_collider.isTrigger = true;
        m_collider.size = sprite.bounds.size;
        m_renderer.sprite = sprite;
        m_renderer.sharedMaterial = material;
        m_renderer.color = new Color(0.43f, 0.27f, 0.13f, 1f);
        m_renderer.sortingLayerID = sortingLayerId;
        m_renderer.sortingOrder = sortingOrder;
        transform.localScale = new Vector3(
            behavior.MudAvalancheLength * sizeMultiplier / sprite.bounds.size.x,
            behavior.MudAvalancheWidth * sizeMultiplier / sprite.bounds.size.y, 1f);
        transform.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg);
        GameObject trailObject = new GameObject("MudAvalancheTrail");
        m_trail = trailObject.AddComponent<ElementalMudTrail>();
        m_trail.Initialize(transform.position, m_direction,
            (behavior.MudAvalancheWidth + 0.3f) * sizeMultiplier, behavior.MudTrailDuration,
            behavior.MudTrailSlowPercent, material, sortingLayerId, sortingOrder - 1);
        m_isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!m_isInitialized) return;
        Vector2 oldPosition = m_rigidbody.position;
        float travel = Mathf.Min(m_speed * Time.fixedDeltaTime, m_distanceRemaining);
        m_rigidbody.MovePosition(oldPosition + m_direction * travel);
        m_distanceRemaining -= travel;
        m_distanceUntilNextTrail += travel;

        while (m_distanceUntilNextTrail >= TrailSpacing)
        {
            m_distanceUntilNextTrail -= TrailSpacing;
            m_trail.AppendPoint(oldPosition + m_direction *
                (travel - m_distanceUntilNextTrail));
        }

        if (m_distanceRemaining <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null || !enemy.IsAlive || !m_hitEnemies.Add(enemy)) return;

        enemy.TakeDamage(m_damage);
        if (enemy.IsAlive && m_behavior.MudAvalancheKnockbackSpeed > 0f)
            enemy.ApplyKnockback(m_direction, m_behavior.MudAvalancheKnockbackSpeed, 0.35f);
    }

}
