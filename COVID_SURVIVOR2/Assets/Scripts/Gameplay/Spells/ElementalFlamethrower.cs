using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Ejecuta rajadas breves delante del médico, con pulsos de daño separados.
public class ElementalFlamethrower : MonoBehaviour
{
    private readonly Collider2D[] m_overlapResults = new Collider2D[128];
    private readonly HashSet<BaseEnemy> m_hitThisPulse = new();
    private ContactFilter2D m_contactFilter;

    private Player m_player;
    private Camera m_camera;
    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;
    private Mesh m_mesh;
    private Vector2 m_smoothedDirection = Vector2.down;
    private Vector2 m_lastValidAimDirection = Vector2.down;
    private float m_length;
    private float m_angle;
    private float m_visualWidthMultiplier;
    private float m_burstInterval;
    private float m_burstDuration;
    private float m_pulseInterval;
    private float m_damageMultiplier;
    private float m_turnSpeed;
    private float m_nextBurstTime;
    private float m_nextPulseTime;
    private float m_burstEndTime;
    private float m_visualPulseStartTime;
    private int m_pulsesPerBurst;
    private int m_pulsesRemaining;
    private int m_damage;
    private Color m_color;
    private bool m_isConfigured;
    private bool m_isBurstActive;

    public void Configure(Player player, int damage, float length, float angle,
        float visualWidthMultiplier,
        float burstInterval, float burstDuration, int pulseCount, float pulseInterval,
        float damageMultiplier, float turnSpeed, Color color, Material material,
        int sortingLayerId, int sortingOrder)
    {
        if (player == null || material == null || length <= 0f || angle <= 0f ||
            burstInterval <= 0f || burstDuration <= 0f || pulseCount < 1 ||
            pulseInterval <= 0f || turnSpeed <= 0f)
            return;

        bool firstConfiguration = !m_isConfigured;
        m_contactFilter.useTriggers = true;
        m_contactFilter.useLayerMask = false;
        m_player = player;
        if (firstConfiguration)
        {
            m_camera = Camera.main;
            if (m_camera == null)
                Debug.LogError("Furacão Flamejante precisa da câmera MainCamera para mirar pelo mouse.", this);
        }
        m_damage = damage;
        m_length = length;
        m_angle = angle;
        m_visualWidthMultiplier = Mathf.Clamp(visualWidthMultiplier, 0.1f, 2f);
        m_burstInterval = burstInterval;
        m_burstDuration = burstDuration;
        m_pulsesPerBurst = pulseCount;
        m_pulseInterval = pulseInterval;
        m_damageMultiplier = damageMultiplier;
        m_turnSpeed = turnSpeed;
        m_color = color;

        if (m_meshFilter == null)
        {
            m_meshFilter = gameObject.AddComponent<MeshFilter>();
            m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
            m_meshRenderer.sharedMaterial = material;
            m_meshRenderer.sortingLayerID = sortingLayerId;
            m_meshRenderer.sortingOrder = sortingOrder;
            m_meshRenderer.enabled = false;
            m_mesh = CreateConeMesh();
            m_meshFilter.sharedMesh = m_mesh;
        }

        if (firstConfiguration)
        {
            m_smoothedDirection = player.FacingDirection;
            if (m_smoothedDirection.sqrMagnitude > 0f)
                m_lastValidAimDirection = m_smoothedDirection;
            m_nextBurstTime = Time.time;
        }

        transform.position = player.transform.position;
        m_isConfigured = true;
    }

    private void Update()
    {
        if (!m_isConfigured || m_player == null || !m_player.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        UpdateDirection();
        UpdateBurst();
    }

    private void UpdateDirection()
    {
        if (m_camera != null && Mouse.current != null)
        {
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            float cameraDepth = Mathf.Abs(m_player.transform.position.z - m_camera.transform.position.z);
            Vector3 mouseWorldPosition = m_camera.ScreenToWorldPoint(new Vector3(
                mouseScreenPosition.x, mouseScreenPosition.y, cameraDepth));
            Vector2 mouseDirection = (Vector2)mouseWorldPosition -
                                     (Vector2)m_player.transform.position;

            // Sobre o médico não há direção nova; mantemos a última mira válida.
            if (mouseDirection.sqrMagnitude > 0.0001f)
                m_lastValidAimDirection = mouseDirection.normalized;
        }

        float maxTurnRadians = m_turnSpeed * Time.deltaTime * Mathf.Deg2Rad;
        m_smoothedDirection = Vector3.RotateTowards(
            m_smoothedDirection, m_lastValidAimDirection, maxTurnRadians, 0f).normalized;

        transform.position = m_player.transform.position;
        transform.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(m_smoothedDirection.y, m_smoothedDirection.x) * Mathf.Rad2Deg);
    }

    private void UpdateBurst()
    {
        if (!m_isBurstActive && Time.time >= m_nextBurstTime)
            BeginBurst();

        if (!m_isBurstActive) return;

        if (m_pulsesRemaining > 0 && Time.time >= m_nextPulseTime)
        {
            DamageEnemiesInCone();
            m_pulsesRemaining--;
            m_nextPulseTime = Time.time + m_pulseInterval;
            m_visualPulseStartTime = Time.time;
        }

        if (Time.time >= m_burstEndTime)
            EndBurst();
        else
        {
            float progress = Mathf.Clamp01((Time.time - m_visualPulseStartTime) / m_pulseInterval);
            float visualScale = Mathf.Lerp(0.82f, 1f, progress);
            transform.localScale = new Vector3(visualScale, visualScale, 1f);
        }
    }

    private void BeginBurst()
    {
        m_isBurstActive = true;
        m_pulsesRemaining = m_pulsesPerBurst;
        m_burstEndTime = Time.time + m_burstDuration;
        m_nextBurstTime = Time.time + m_burstInterval;
        m_nextPulseTime = Time.time;
        m_meshRenderer.enabled = true;
        m_visualPulseStartTime = Time.time;
        transform.localScale = Vector3.one * 0.82f;
    }

    private void EndBurst()
    {
        m_isBurstActive = false;
        m_meshRenderer.enabled = false;
        transform.localScale = Vector3.one;
    }

    private void DamageEnemiesInCone()
    {
        m_hitThisPulse.Clear();
        int count = Physics2D.OverlapCircle(
            transform.position, m_length, m_contactFilter, m_overlapResults);
        int damage = Mathf.Max(1, Mathf.RoundToInt(m_damage * m_damageMultiplier));

        for (int i = 0; i < count; i++)
        {
            Collider2D collider = m_overlapResults[i];
            m_overlapResults[i] = null;
            if (collider == null) continue;

            BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
            if (enemy == null || !enemy.IsAlive || !m_hitThisPulse.Add(enemy)) continue;

            Vector2 toEnemy = (Vector2)enemy.transform.position - (Vector2)transform.position;
            if (toEnemy.sqrMagnitude <= 0f || toEnemy.magnitude > m_length ||
                Vector2.Angle(m_smoothedDirection, toEnemy) > m_angle * 0.5f)
                continue;

            enemy.TakeDamage(damage);
        }
    }

    private Mesh CreateConeMesh()
    {
        const int segments = 24;
        Vector3[] vertices = new Vector3[segments + 2];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[segments * 3];
        vertices[0] = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float normalized = i / (float)segments;
            float radians = Mathf.Lerp(-m_angle * 0.5f, m_angle * 0.5f, normalized) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(
                Mathf.Cos(radians) * m_length,
                Mathf.Sin(radians) * m_length * m_visualWidthMultiplier,
                0f);
            colors[i + 1] = new Color(m_color.r, m_color.g, m_color.b, 0.38f);
        }

        colors[0] = new Color(m_color.r, m_color.g, m_color.b, 0.62f);
        for (int i = 0; i < segments; i++)
        {
            int index = i * 3;
            triangles[index] = 0;
            triangles[index + 1] = i + 1;
            triangles[index + 2] = i + 2;
        }

        Mesh mesh = new() { name = "TemporaryFlameBurstCone" };
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }

    private void OnDestroy()
    {
        if (m_mesh != null) Destroy(m_mesh);
    }
}
