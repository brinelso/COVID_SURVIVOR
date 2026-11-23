using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EdgeCollider2D), typeof(LineRenderer))]
public class ElementalMudTrail : MonoBehaviour
{
    private readonly List<Vector2> m_points = new();
    private readonly Dictionary<BaseEnemy, HashSet<Collider2D>> m_contacts = new();
    private readonly List<BaseEnemy> m_contactCleanup = new();
    private EdgeCollider2D m_collider;
    private LineRenderer m_line;
    private float m_remaining;
    private float m_slowPercent;

    public void Initialize(Vector2 start, Vector2 direction, float width,
        float duration, float slowPercent, Material material, int sortingLayerId, int sortingOrder)
    {
        if (direction.sqrMagnitude <= 0.0001f || width <= 0f || duration <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.zero;
        m_remaining = duration;
        m_slowPercent = slowPercent;
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        m_collider = GetComponent<EdgeCollider2D>();
        m_collider.isTrigger = true;
        m_collider.edgeRadius = width * 0.5f;
        m_line = GetComponent<LineRenderer>();
        m_line.useWorldSpace = true;
        m_line.sharedMaterial = material;
        m_line.startColor = new Color(0.38f, 0.23f, 0.11f, 0.8f);
        m_line.endColor = m_line.startColor;
        m_line.startWidth = width;
        m_line.endWidth = width;
        m_line.sortingLayerID = sortingLayerId;
        m_line.sortingOrder = sortingOrder;

        Vector2 normalizedDirection = direction.normalized;
        m_points.Add(start);
        m_points.Add(start + normalizedDirection * 0.01f);
        UpdatePath();
    }

    public void AppendPoint(Vector2 point)
    {
        if (m_points.Count == 0 || (m_points[^1] - point).sqrMagnitude < 0.01f) return;
        m_points.Add(point);
        UpdatePath();
    }

    private void UpdatePath()
    {
        m_collider.SetPoints(m_points);
        m_line.positionCount = m_points.Count;
        for (int i = 0; i < m_points.Count; i++)
            m_line.SetPosition(i, m_points[i]);
    }

    private void Update()
    {
        m_remaining -= Time.deltaTime;
        if (m_remaining <= 0f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RegisterEnemy(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        BaseEnemy enemy = RegisterEnemy(other);
        if (enemy != null && enemy.IsAlive)
            enemy.EnterSlowArea(this, m_slowPercent);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null || !m_contacts.TryGetValue(enemy, out HashSet<Collider2D> colliders)) return;
        colliders.Remove(other);
        if (colliders.Count > 0) return;
        m_contacts.Remove(enemy);
        enemy.ExitSlowArea(this);
    }

    private BaseEnemy RegisterEnemy(Collider2D other)
    {
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null || !enemy.IsAlive) return null;
        if (!m_contacts.TryGetValue(enemy, out HashSet<Collider2D> colliders))
        {
            colliders = new HashSet<Collider2D>();
            m_contacts.Add(enemy, colliders);
        }
        colliders.Add(other);
        return enemy;
    }

    private void OnDestroy()
    {
        m_contactCleanup.Clear();
        foreach (BaseEnemy enemy in m_contacts.Keys)
            m_contactCleanup.Add(enemy);
        foreach (BaseEnemy enemy in m_contactCleanup)
        {
            if (enemy != null) enemy.ExitSlowArea(this);
        }
        m_contacts.Clear();
    }
}
