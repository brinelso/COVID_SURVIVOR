using UnityEngine;

public class VaccineSpell : BaseSpell
{
    [SerializeField] private VaccineProjectile m_projectilePrefab;
    [SerializeField, Min(1)] private int m_projectileCount = 1;
    [SerializeField, Min(0.1f)] private float m_projectileSizeMultiplier = 1f;

    public bool CanIncreaseFireRate => CanReduceFireInterval(0.1f);

    public void ApplyUpgrade(UpgradeEffect effect, float amount)
    {
        switch (effect)
        {
            case UpgradeEffect.Damage:
                AddDamage(Mathf.Max(1, Mathf.RoundToInt(amount)));
                break;
            case UpgradeEffect.FireRate:
                ReduceFireInterval(amount);
                break;
            case UpgradeEffect.Range:
                AddSearchRange(amount);
                break;
            case UpgradeEffect.ProjectileSize:
                m_projectileSizeMultiplier += amount;
                break;
            case UpgradeEffect.ProjectileCount:
                m_projectileCount += Mathf.Max(1, Mathf.RoundToInt(amount));
                break;
        }
    }

    public bool CanApplyUpgrade(UpgradeEffect effect)
    {
        return effect != UpgradeEffect.FireRate || CanIncreaseFireRate;
    }

    protected override void Cast(BaseEnemy target)
    {
        if (m_projectilePrefab == null)
        {
            Debug.LogError("O prefab do projétil precisa ser associado no Inspector.", this);
            return;
        }

        Vector2 direction = target.transform.position - transform.position;
        if (direction == Vector2.zero) return;

        direction.Normalize();
        float spreadAngle = 12f;
        float startingAngle = -spreadAngle * (m_projectileCount - 1) * 0.5f;

        for (int i = 0; i < m_projectileCount; i++)
        {
            Vector2 projectileDirection = Quaternion.Euler(0f, 0f, startingAngle + spreadAngle * i) * direction;
            VaccineProjectile projectile = Instantiate(
                m_projectilePrefab,
                transform.position,
                Quaternion.identity);

            projectile.transform.localScale *= m_projectileSizeMultiplier;
            projectile.Initialize(projectileDirection, m_damage, m_projectileSpeed, m_projectileLifetime);
        }
    }
}
