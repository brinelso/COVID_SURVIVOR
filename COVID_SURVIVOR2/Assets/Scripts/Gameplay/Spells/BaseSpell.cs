using UnityEngine;

public abstract class BaseSpell : MonoBehaviour
{
    [Header("Configuração da magia")]
    [SerializeField] protected Player m_player;
    [SerializeField] protected int m_damage = 10;
    [SerializeField] protected float m_searchRange = 6f;
    [SerializeField] protected float m_fireInterval = 0.75f;
    [SerializeField] protected float m_projectileSpeed = 7f;
    [SerializeField] protected float m_projectileLifetime = 3f;

    private float m_nextFireTime;
    private PlayerStats m_playerStats;

    protected virtual void Start()
    {
        if (m_player == null)
        {
            Debug.LogError("Player precisa ser associado à magia no Inspector.", this);
            enabled = false;
            return;
        }

        if (m_damage <= 0 || m_searchRange <= 0f || m_fireInterval <= 0f
            || m_projectileSpeed <= 0f || m_projectileLifetime <= 0f)
        {
            Debug.LogError("Os valores de dano, alcance, intervalo, velocidade e vida do projétil devem ser positivos.", this);
            enabled = false;
            return;
        }

        // Os atributos gerais devem beneficiar a vacina e a magia elemental.
        m_playerStats = m_player.GetComponent<PlayerStats>();
    }

    protected virtual void Update()
    {
        if (m_player == null || !m_player.IsAlive) return;
        if (Time.time < m_nextFireTime) return;

        BaseEnemy target = FindClosestTarget();
        m_nextFireTime = Time.time + GetCurrentFireInterval();
        if (target == null) return;

        Cast(target);
    }

    private BaseEnemy FindClosestTarget()
    {
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>();
        BaseEnemy closestEnemy = null;
        float closestDistanceSquared = m_searchRange * m_searchRange;

        foreach (BaseEnemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;

            float distanceSquared = (enemy.transform.position - m_player.transform.position).sqrMagnitude;
            if (distanceSquared > closestDistanceSquared) continue;

            closestDistanceSquared = distanceSquared;
            closestEnemy = enemy;
        }

        return closestEnemy;
    }

    protected abstract void Cast(BaseEnemy target);

    // Os upgrades de atributos sao globais; os upgrades elementais permanecem no controlador elemental.
    protected int GetCurrentDamage()
    {
        long bonus = m_playerStats == null ? 0L : Mathf.RoundToInt(m_playerStats.DamageBonus);
        long result = (long)m_damage + bonus;
        return (int)System.Math.Min(System.Math.Max(result, 1L), int.MaxValue);
    }

    protected float GetCurrentFireInterval()
    {
        float reduction = m_playerStats == null ? 0f : m_playerStats.AttackIntervalReduction;
        return Mathf.Max(0.1f, m_fireInterval - reduction);
    }

    // As melhorias alteram as configurações já usadas pela magia.
    public bool CanReduceFireInterval(float amount, float minimumInterval = 0.1f)
    {
        return amount > 0f && m_fireInterval > minimumInterval;
    }

    public void AddDamage(int amount)
    {
        if (amount > 0 && m_damage <= int.MaxValue - amount)
            m_damage += amount;
    }

    public void AddSearchRange(float amount)
    {
        if (amount > 0f) m_searchRange += amount;
    }

    public void ReduceFireInterval(float amount, float minimumInterval = 0.1f)
    {
        if (amount > 0f)
            m_fireInterval = Mathf.Max(minimumInterval, m_fireInterval - amount);
    }
}
