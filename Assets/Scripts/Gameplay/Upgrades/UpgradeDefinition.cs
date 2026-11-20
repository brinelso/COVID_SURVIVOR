using UnityEngine;

public enum UpgradeCategory
{
    SpellUnlock = 0, // Mantido para compatibilidade com assets antigos.
    SpellUpgrade = 1,
    Attribute = 2,
    Element = 3
}

public enum UpgradeEffect
{
    SpellUnlock,
    Damage,
    FireRate,
    Range,
    ProjectileSize,
    ProjectileCount,
    MovementSpeed,
    ExperiencePickupRange,
    ProjectileSpeed
}

// Cada asset descreve uma opção e encaminha seu efeito para o sistema correto.
[CreateAssetMenu(fileName = "Upgrade", menuName = "COVID SURVIVOR/Upgrade")]
public class UpgradeDefinition : ScriptableObject
{
    [SerializeField] private string m_upgradeName;
    [SerializeField, TextArea] private string m_description;
    [SerializeField] private UpgradeCategory m_category;
    [SerializeField] private UpgradeEffect m_effect;
    [SerializeField, Min(0f)] private float m_amount;
    [SerializeField, Min(0)] private int m_maxApplications;

    public string UpgradeName => m_upgradeName;
    public string Description => m_description;
    public UpgradeCategory Category => m_category;

    public bool IsEligible(PlayerStats playerStats, ElementalSpellController spellController,
        int applicationCount)
    {
        if (m_maxApplications > 0 && applicationCount >= m_maxApplications) return false;

        if (m_category == UpgradeCategory.Attribute)
            return playerStats != null && playerStats.CanApplyUpgrade(m_effect);

        if (m_category == UpgradeCategory.SpellUpgrade)
            return spellController != null && spellController.IsMagicUnlocked &&
                   spellController.CanApplyUpgrade(m_effect);

        return false;
    }

    public bool Apply(PlayerStats playerStats, ElementalSpellController spellController,
        int applicationCount)
    {
        if (!IsEligible(playerStats, spellController, applicationCount)) return false;

        if (m_category == UpgradeCategory.Attribute)
            return playerStats.ApplyUpgrade(m_effect, m_amount);

        if (m_category == UpgradeCategory.SpellUpgrade)
            return spellController.ApplyUpgrade(m_effect, m_amount);

        return false;
    }
}
