using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerStats : MonoBehaviour
{
    private Player m_player;

    public float DamageBonus { get; private set; }
    public float AttackIntervalReduction { get; private set; }
    public float MovementSpeedBonusPercent { get; private set; }
    public float ExperiencePickupRangeBonus { get; private set; }

    private void Awake()
    {
        m_player = GetComponent<Player>();
    }

    public bool CanApplyUpgrade(UpgradeEffect effect)
    {
        return effect == UpgradeEffect.Damage || effect == UpgradeEffect.FireRate ||
               effect == UpgradeEffect.MovementSpeed || effect == UpgradeEffect.ExperiencePickupRange;
    }

    public bool ApplyUpgrade(UpgradeEffect effect, float amount)
    {
        if (amount <= 0f || m_player == null || !CanApplyUpgrade(effect)) return false;

        switch (effect)
        {
            case UpgradeEffect.Damage:
                DamageBonus += Mathf.Max(1, Mathf.RoundToInt(amount));
                break;
            case UpgradeEffect.FireRate:
                AttackIntervalReduction += amount;
                break;
            case UpgradeEffect.MovementSpeed:
                MovementSpeedBonusPercent += amount;
                m_player.AddMovementSpeedBonus(amount);
                break;
            case UpgradeEffect.ExperiencePickupRange:
                ExperiencePickupRangeBonus += amount;
                m_player.AddExperiencePickupRange(amount);
                break;
            default:
                return false;
        }

        return true;
    }
}
