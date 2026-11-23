using UnityEngine;

// Ajusta os atributos do ataque genérico para a receita Água Pressurizada.
[CreateAssetMenu(fileName = "PressurizedWaterBehavior", menuName = "COVID SURVIVOR/Behaviors/Pressurized Water")]
public class PressurizedWaterBehavior : ElementalSpellBehavior
{
    [Header("Atributos do jato")]
    [SerializeField, Min(1f)] private float m_damageMultiplier = 1.5f;
    [SerializeField, Min(1f)] private float m_searchRangeMultiplier = 1.5f;
    [SerializeField, Min(1f)] private float m_projectileSpeedMultiplier = 1.5f;

    [Header("Formato provisório")]
    [SerializeField, Min(0.1f)] private float m_lengthMultiplier = 1.8f;
    [SerializeField, Min(0.1f)] private float m_widthMultiplier = 0.45f;
    [SerializeField] private bool m_canPierce = true;

    public override int ModifyDamage(int damage)
    {
        return Mathf.Max(1, Mathf.RoundToInt(damage * m_damageMultiplier));
    }

    public override float ModifySearchRange(float searchRange)
    {
        return Mathf.Max(0.1f, searchRange * m_searchRangeMultiplier);
    }

    public override float ModifyProjectileSpeed(float speed)
    {
        return Mathf.Max(0.1f, speed * m_projectileSpeedMultiplier);
    }

    public override Vector2 ModifyProjectileScale(Vector2 scale)
    {
        return new Vector2(scale.x * m_lengthMultiplier, scale.y * m_widthMultiplier);
    }

    public override bool CanPierce => m_canPierce;
}
