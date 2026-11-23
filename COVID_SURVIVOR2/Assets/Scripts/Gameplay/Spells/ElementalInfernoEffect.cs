using UnityEngine;

// Mantém a aura junto ao jogador e cria ondas expansivas em intervalos regulares.
public class ElementalInfernoEffect : MonoBehaviour
{
    private Player m_player;
    private SpriteRenderer m_auraRenderer;
    private Sprite m_sprite;
    private Material m_material;
    private Color m_color;
    private int m_sortingLayerId;
    private int m_sortingOrder;
    private int m_damage;
    private float m_auraRadius;
    private float m_auraTickInterval;
    private float m_auraDamageMultiplier;
    private float m_waveInterval;
    private float m_waveRadius;
    private float m_waveSpeed;
    private float m_waveDamageMultiplier;
    private float m_waveLineWidth;
    private float m_nextAuraTick;
    private float m_nextWaveTime;
    private bool m_isConfigured;

    public void Configure(Player player, int damage, ElementalSpellBehavior behavior,
        Sprite sprite, Material material, Color color, int sortingLayerId, int sortingOrder)
    {
        if (player == null || behavior == null || sprite == null || material == null)
            return;

        bool firstConfiguration = !m_isConfigured;
        m_player = player;
        m_damage = damage;
        m_sprite = sprite;
        m_material = material;
        m_color = color;
        m_sortingLayerId = sortingLayerId;
        m_sortingOrder = sortingOrder;
        m_auraRadius = behavior.InfernoAuraRadius;
        m_auraTickInterval = behavior.InfernoAuraTickInterval;
        m_auraDamageMultiplier = behavior.InfernoAuraDamageMultiplier;
        m_waveInterval = behavior.InfernoWaveInterval;
        m_waveRadius = behavior.InfernoWaveRadius;
        m_waveSpeed = behavior.InfernoWaveExpansionSpeed;
        m_waveDamageMultiplier = behavior.InfernoWaveDamageMultiplier;
        m_waveLineWidth = behavior.InfernoWaveLineWidth;

        if (m_auraRenderer == null)
        {
            GameObject aura = new GameObject("InfernoAura");
            aura.transform.SetParent(transform, false);
            m_auraRenderer = aura.AddComponent<SpriteRenderer>();
        }

        m_auraRenderer.sprite = m_sprite;
        m_auraRenderer.sharedMaterial = m_material;
        m_auraRenderer.sortingLayerID = sortingLayerId;
        m_auraRenderer.sortingOrder = sortingOrder - 2;
        m_auraRenderer.color = new Color(color.r, color.g, color.b, 0.22f);
        Vector2 spriteSize = m_sprite.bounds.size;
        m_auraRenderer.transform.localScale = new Vector3(
            m_auraRadius * 2f / spriteSize.x, m_auraRadius * 2f / spriteSize.y, 1f);
        if (firstConfiguration)
        {
            m_nextAuraTick = Time.time;
            m_nextWaveTime = Time.time + m_waveInterval;
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

        Vector2 playerPosition = m_player.transform.position;
        m_auraRenderer.transform.position = playerPosition;

        if (Time.time >= m_nextAuraTick)
        {
            m_nextAuraTick = Time.time + m_auraTickInterval;
            DamageAura(playerPosition);
        }

        if (Time.time >= m_nextWaveTime)
        {
            m_nextWaveTime = Time.time + m_waveInterval;
            CreateWave(playerPosition);
        }
    }

    private void DamageAura(Vector2 center)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, m_auraRadius);
        int damage = Mathf.Max(1, Mathf.RoundToInt(m_damage * m_auraDamageMultiplier));
        System.Collections.Generic.HashSet<BaseEnemy> damagedEnemies = new();

        foreach (Collider2D collider in colliders)
        {
            BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
            if (enemy == null || !enemy.IsAlive || !damagedEnemies.Add(enemy)) continue;
            if (((Vector2)enemy.transform.position - center).magnitude > m_auraRadius) continue;
            enemy.TakeDamage(damage);
        }
    }

    private void CreateWave(Vector2 center)
    {
        GameObject waveObject = new GameObject("InfernoFireWave");
        waveObject.transform.SetParent(transform, false);
        waveObject.transform.position = center;
        ElementalFireWave wave = waveObject.AddComponent<ElementalFireWave>();
        int damage = Mathf.Max(1, Mathf.RoundToInt(m_damage * m_waveDamageMultiplier));
        wave.Initialize(center, m_waveRadius, m_waveSpeed, m_waveLineWidth,
            damage, m_color, m_material, m_sortingLayerId, m_sortingOrder + 1);
    }
}
