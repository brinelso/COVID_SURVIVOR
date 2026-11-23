using UnityEngine;

// Aplica multiplicadores simples ao projétil compartilhado por uma receita.
[CreateAssetMenu(fileName = "ElementalProjectileBehavior", menuName = "COVID SURVIVOR/Behaviors/Projectile Modifiers")]
public class ElementalProjectileModifierBehavior : ElementalSpellBehavior
{
    [Header("Atributos do projétil")]
    [SerializeField, Min(0.1f)] private float m_damageMultiplier = 1f;
    [SerializeField, Min(0.1f)] private float m_searchRangeMultiplier = 1f;
    [SerializeField, Min(0.1f)] private float m_projectileSpeedMultiplier = 1f;
    [SerializeField, Min(0.1f)] private float m_projectileLifetimeMultiplier = 1f;
    [SerializeField] private bool m_canPierce;

    [Header("Formato provisório")]
    [SerializeField, Min(0.1f)] private float m_lengthMultiplier = 1f;
    [SerializeField, Min(0.1f)] private float m_widthMultiplier = 1f;

    [Header("Rajadas Cortantes")]
    [SerializeField, Min(0)] private int m_additionalProjectileCount;
    [SerializeField] private bool m_homesInOnTarget;
    [SerializeField, Min(1f)] private float m_homingTurnSpeed = 360f;

    [Header("Queimadura — Chamas Intensas")]
    [SerializeField] private bool m_appliesBurn;
    [SerializeField, Min(1)] private int m_burnDamagePerTick = 2;
    [SerializeField, Min(0.1f)] private float m_burnDuration = 2f;
    [SerializeField, Min(0.1f)] private float m_burnTickInterval = 0.5f;

    [Header("Lentidão — Rocha Pesada")]
    [SerializeField] private bool m_appliesSlow;
    [SerializeField, Range(0f, 0.95f)] private float m_slowPercent = 0.3f;
    [SerializeField, Min(0.1f)] private float m_slowDuration = 2f;

    [Header("Gelo Perfurante")]
    [SerializeField, Range(0f, 1f)] private float m_freezeChance;
    [SerializeField, Min(0.1f)] private float m_freezeDuration = 1.5f;

    [Header("Áreas persistentes")]
    [SerializeField] private ElementalAttackMode m_attackMode;
    [SerializeField, Min(0.1f)] private float m_activeDuration = 3f;
    [SerializeField, Min(0.1f)] private float m_areaRadius = 0.75f;
    [SerializeField, Min(0.1f)] private float m_damageTickInterval = 0.5f;

    [Header("Tempestade de Areia — lentidão progressiva")]
    [SerializeField, Range(0f, 1f)] private float m_progressiveSlowMaximumPercent = 1f;
    [SerializeField, Min(0.1f)] private float m_progressiveSlowDuration = 2f;

    [Header("Tempestade de Areia — crescimento no voo")]
    [SerializeField, Range(0.01f, 1f)] private float m_impactGrowthStartScale = 0.25f;
    [SerializeField, Min(0.1f)] private float m_impactGrowthDuration = 1f;

    [Header("Vapor Escaldante — explosão em cone")]
    [SerializeField, Min(0.1f)] private float m_coneLength = 3f;
    [SerializeField, Range(1f, 120f)] private float m_coneAngle = 70f;
    [SerializeField, Min(0f)] private float m_coneDamageMultiplier = 1f;
    [SerializeField, Min(0.05f)] private float m_coneVisualDuration = 0.3f;

    [Header("Lama — área e projétil")]
    [SerializeField, Min(0.1f)] private float m_mudProjectileLength = 0.8f;
    [SerializeField, Min(0.1f)] private float m_mudProjectileWidth = 1.5f;
    [SerializeField, Range(0f, 0.95f)] private float m_mudSlowPercent = 0.4f;

    [Header("Áreas, impactos e efeitos especiais")]
    [SerializeField, Min(0f)] private float m_areaDamageMultiplier = 1f;
    [SerializeField, Min(0f)] private float m_impactDamageMultiplier = 1f;
    [SerializeField, Min(0)] private int m_fragmentCount;
    [SerializeField, Min(0f)] private float m_fragmentDamageMultiplier = 0.5f;
    [SerializeField, Min(0.1f)] private float m_fragmentSpeed = 5f;
    [SerializeField, Min(0.1f)] private float m_fragmentLifetime = 1f;
    [SerializeField, Min(0.05f)] private float m_fragmentSize = 0.2f;
    [SerializeField, Range(0f, 360f)] private float m_fragmentSpreadAngle = 360f;
    [SerializeField, Min(1)] private int m_sequenceCount = 5;
    [SerializeField, Min(0.05f)] private float m_sequenceInterval = 0.25f;
    [SerializeField, Min(0.1f)] private float m_sequenceBlastRadius = 0.75f;
    [SerializeField, Min(0.1f)] private float m_geyserWidth = 0.8f;
    [SerializeField, Min(0.1f)] private float m_geyserHeight = 2.8f;
    [SerializeField, Min(0.05f)] private float m_geyserRiseDuration = 0.2f;
    [SerializeField, Min(0.1f)] private float m_forwardConeLength = 2f;
    [SerializeField, Range(1f, 120f)] private float m_forwardConeAngle = 60f;
    [SerializeField, Range(0.1f, 2f)] private float m_flamethrowerVisualWidthMultiplier = 1f;
    [SerializeField, Min(0.1f)] private float m_meteorSpawnHeight = 3f;

    [Header("Furacão Flamejante — rajadas")]
    [SerializeField, Min(0.05f)] private float m_flamethrowerBurstInterval = 0.55f;
    [SerializeField, Min(0.05f)] private float m_flamethrowerBurstDuration = 0.3f;
    [SerializeField, Min(1)] private int m_flamethrowerPulseCount = 3;
    [SerializeField, Min(0.01f)] private float m_flamethrowerPulseInterval = 0.1f;
    [SerializeField, Min(0f)] private float m_flamethrowerDamageMultiplier = 0.25f;
    [SerializeField, Min(1f)] private float m_flamethrowerTurnSpeed = 1440f;

    [Header("Órbita Ígnea")]
    [SerializeField, Min(0.1f)] private float m_orbitRadius = 1.5f;
    [SerializeField, Min(0f)] private float m_orbitSpeedDegrees = 180f;
    [SerializeField, Min(0.05f)] private float m_orbitTickInterval = 0.5f;
    [SerializeField, Min(0f)] private float m_orbitDamageMultiplier = 0.25f;
    [SerializeField, Min(0.1f)] private float m_orbitSize = 0.65f;

    [Header("Inferno")]
    [SerializeField, Min(0.1f)] private float m_infernoAuraRadius = 3f;
    [SerializeField, Min(0.05f)] private float m_infernoAuraTickInterval = 1f;
    [SerializeField, Min(0f)] private float m_infernoAuraDamageMultiplier = 0.2f;
    [SerializeField, Min(0.1f)] private float m_infernoWaveInterval = 3f;
    [SerializeField, Min(0.1f)] private float m_infernoWaveRadius = 5f;
    [SerializeField, Min(0.1f)] private float m_infernoWaveExpansionSpeed = 8f;
    [SerializeField, Min(0f)] private float m_infernoWaveDamageMultiplier = 1.5f;
    [SerializeField, Min(0.01f)] private float m_infernoWaveLineWidth = 0.12f;

    [Header("Cadência da receita")]
    [SerializeField, Min(0.1f)] private float m_attackIntervalMultiplier = 1f;

    [Header("Tsunami")]
    [SerializeField, Min(0.1f)] private float m_tsunamiWidth = 4f;
    [SerializeField, Min(0.1f)] private float m_tsunamiTravelDistance = 12f;
    [SerializeField, Min(0f)] private float m_tsunamiKnockbackSpeed = 3f;

    [Header("Área de lentidão")]
    [SerializeField, Range(0f, 0.95f)] private float m_areaSlowPercent;

    [Header("Tempestade Glacial")]
    [SerializeField, Range(0f, 1f)] private float m_stormFreezeChance = 0.25f;
    [SerializeField, Min(0.1f)] private float m_stormFreezeDuration = 1f;
    [SerializeField, Min(0.1f)] private float m_glacialTravelDistance = 12f;
    [SerializeField, Range(0.01f, 1f)] private float m_glacialContactSpeedMultiplier = 0.4f;
    [SerializeField, Min(0.05f)] private float m_glacialFreezeAttemptInterval = 0.45f;
    [SerializeField, Min(0.1f)] private float m_glacialSpeedMultiplier = 0.6f;

    [Header("Planta — raízes e veneno")]
    [SerializeField] private Color m_plantProjectileColor = new Color(0.25f, 0.78f, 0.32f, 1f);
    [SerializeField, Min(0f)] private float m_plantImpactDamageMultiplier = 0.8f;
    [SerializeField, Min(0.1f)] private float m_plantSpreadRadius = 2f;
    [SerializeField, Min(1)] private int m_plantMaximumEnemies = 6;
    [SerializeField, Min(0.1f)] private float m_plantRootDuration = 1f;
    [SerializeField, Min(0.1f)] private float m_plantPoisonDuration = 3f;
    [SerializeField, Min(0f)] private float m_plantPoisonDamageMultiplier = 0.25f;
    [SerializeField, Min(0.05f)] private float m_plantPoisonTickInterval = 0.5f;

    [Header("Vórtices de areia e vento")]
    [SerializeField, Min(0.1f)] private float m_vortexAttractionRadius = 3f;
    [SerializeField, Min(0f)] private float m_vortexAttractionSpeed = 3f;
    [SerializeField, Min(0.1f)] private float m_vortexMovementSpeed = 3f;
    [SerializeField, Min(0f)] private float m_vortexDamageMultiplier = 0.35f;
    [SerializeField, Min(0.05f)] private float m_vortexTargetSearchInterval = 0.25f;
    [SerializeField, Min(0.1f)] private float m_tornadoInitialRadius = 0.7f;
    [SerializeField, Min(0.1f)] private float m_tornadoMaximumRadius = 2f;
    [SerializeField, Min(0.1f)] private float m_tornadoGrowthDuration = 2f;

    [Header("Redemoinho de Rochas")]
    [SerializeField, Min(1)] private int m_rockWhirlwindStoneCount = 8;
    [SerializeField, Min(0.1f)] private float m_rockWhirlwindOrbitRadius = 1.2f;
    [SerializeField, Min(0f)] private float m_rockWhirlwindRotationSpeed = 540f;
    [SerializeField, Min(0f)] private float m_rockWhirlwindDamageMultiplier = 0.35f;
    [SerializeField, Min(0.05f)] private float m_rockWhirlwindDamageInterval = 0.5f;
    [SerializeField, Min(0.1f)] private float m_rockWhirlwindGroupRadius = 3f;

    [Header("Avalanche de Lama")]
    [SerializeField, Min(0.1f)] private float m_mudAvalancheWidth = 2.5f;
    [SerializeField, Min(0.1f)] private float m_mudAvalancheLength = 2f;
    [SerializeField, Min(0.1f)] private float m_mudAvalancheTravelDistance = 10f;
    [SerializeField, Min(0f)] private float m_mudAvalancheKnockbackSpeed = 4f;
    [SerializeField, Min(0.1f)] private float m_mudTrailDuration = 5f;
    [SerializeField, Range(0f, 0.95f)] private float m_mudTrailSlowPercent = 0.5f;

    [Header("Nevasca")]
    [SerializeField, Range(0f, 1f)] private float m_blizzardFreezeInitialChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float m_blizzardFreezeChanceIncreasePerSecond = 0.1f;
    [SerializeField, Range(0f, 1f)] private float m_blizzardFreezeMaximumChance = 0.5f;
    [SerializeField, Min(0.05f)] private float m_blizzardFreezeAttemptInterval = 1f;
    [SerializeField, Min(0.1f)] private float m_blizzardFreezeDuration = 1.25f;

    [Header("Colosso")]
    [SerializeField, Min(0f)] private float m_colossusWindupDuration = 0.75f;
    [SerializeField, Min(0f)] private float m_colossusKnockbackSpeed = 4f;

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

    public override float ModifyProjectileLifetime(float lifetime)
    {
        return Mathf.Max(0.1f, lifetime * m_projectileLifetimeMultiplier);
    }

    public override Vector2 ModifyProjectileScale(Vector2 scale)
    {
        return new Vector2(scale.x * m_lengthMultiplier, scale.y * m_widthMultiplier);
    }

    public override int ModifyProjectileCount(int count)
    {
        return Mathf.Max(1, count + m_additionalProjectileCount);
    }

    public override bool HomesInOnTarget => m_homesInOnTarget;
    public override bool CanPierce => m_canPierce;
    public override float HomingTurnSpeed => m_homingTurnSpeed;
    public override ElementalAttackMode AttackMode => m_attackMode;
    public override float ActiveDuration => m_activeDuration;
    public override float AreaRadius => m_areaRadius;
    public override float DamageTickInterval => m_damageTickInterval;
    public override float ProgressiveSlowMaximumPercent => m_progressiveSlowMaximumPercent;
    public override float ProgressiveSlowDuration => m_progressiveSlowDuration;
    public override float ImpactGrowthStartScale => m_impactGrowthStartScale;
    public override float ImpactGrowthDuration => m_impactGrowthDuration;
    public override float ConeLength => m_coneLength;
    public override float ConeAngle => m_coneAngle;
    public override float ConeDamageMultiplier => m_coneDamageMultiplier;
    public override float ConeVisualDuration => m_coneVisualDuration;
    public override float MudProjectileLength => m_mudProjectileLength;
    public override float MudProjectileWidth => m_mudProjectileWidth;
    public override float MudSlowPercent => m_mudSlowPercent;
    public override float AreaDamageMultiplier => m_areaDamageMultiplier;
    public override float ImpactDamageMultiplier => m_impactDamageMultiplier;
    public override int FragmentCount => m_fragmentCount;
    public override float FragmentDamageMultiplier => m_fragmentDamageMultiplier;
    public override float FragmentSpeed => m_fragmentSpeed;
    public override float FragmentLifetime => m_fragmentLifetime;
    public override float FragmentSize => m_fragmentSize;
    public override float FragmentSpreadAngle => m_fragmentSpreadAngle;
    public override int SequenceCount => m_sequenceCount;
    public override float SequenceInterval => m_sequenceInterval;
    public override float SequenceBlastRadius => m_sequenceBlastRadius;
    public override float GeyserWidth => m_geyserWidth;
    public override float GeyserHeight => m_geyserHeight;
    public override float GeyserRiseDuration => m_geyserRiseDuration;
    public override float ForwardConeLength => m_forwardConeLength;
    public override float ForwardConeAngle => m_forwardConeAngle;
    public override float FlamethrowerVisualWidthMultiplier => m_flamethrowerVisualWidthMultiplier;
    public override float MeteorSpawnHeight => m_meteorSpawnHeight;
    public override float FlamethrowerBurstInterval => m_flamethrowerBurstInterval;
    public override float FlamethrowerBurstDuration => m_flamethrowerBurstDuration;
    public override int FlamethrowerPulseCount => m_flamethrowerPulseCount;
    public override float FlamethrowerPulseInterval => m_flamethrowerPulseInterval;
    public override float FlamethrowerDamageMultiplier => m_flamethrowerDamageMultiplier;
    public override float FlamethrowerTurnSpeed => m_flamethrowerTurnSpeed;
    public override float OrbitRadius => m_orbitRadius;
    public override float OrbitSpeedDegrees => m_orbitSpeedDegrees;
    public override float OrbitTickInterval => m_orbitTickInterval;
    public override float OrbitDamageMultiplier => m_orbitDamageMultiplier;
    public override float OrbitSize => m_orbitSize;
    public override float InfernoAuraRadius => m_infernoAuraRadius;
    public override float InfernoAuraTickInterval => m_infernoAuraTickInterval;
    public override float InfernoAuraDamageMultiplier => m_infernoAuraDamageMultiplier;
    public override float InfernoWaveInterval => m_infernoWaveInterval;
    public override float InfernoWaveRadius => m_infernoWaveRadius;
    public override float InfernoWaveExpansionSpeed => m_infernoWaveExpansionSpeed;
    public override float InfernoWaveDamageMultiplier => m_infernoWaveDamageMultiplier;
    public override float InfernoWaveLineWidth => m_infernoWaveLineWidth;
    public override float AttackIntervalMultiplier => m_attackIntervalMultiplier;
    public override float TsunamiWidth => m_tsunamiWidth;
    public override float TsunamiTravelDistance => m_tsunamiTravelDistance;
    public override float TsunamiKnockbackSpeed => m_tsunamiKnockbackSpeed;
    public override float AreaSlowPercent => m_areaSlowPercent;
    public override float StormFreezeChance => m_stormFreezeChance;
    public override float StormFreezeDuration => m_stormFreezeDuration;
    public override float GlacialTravelDistance => m_glacialTravelDistance;
    public override float GlacialSpeedMultiplier => m_glacialSpeedMultiplier;
    public override float GlacialContactSpeedMultiplier => m_glacialContactSpeedMultiplier;
    public override float GlacialFreezeAttemptInterval => m_glacialFreezeAttemptInterval;
    public override float PlantImpactDamageMultiplier => m_plantImpactDamageMultiplier;
    public override float PlantSpreadRadius => m_plantSpreadRadius;
    public override int PlantMaximumEnemies => m_plantMaximumEnemies;
    public override float PlantRootDuration => m_plantRootDuration;
    public override float PlantPoisonDuration => m_plantPoisonDuration;
    public override float PlantPoisonDamageMultiplier => m_plantPoisonDamageMultiplier;
    public override float PlantPoisonTickInterval => m_plantPoisonTickInterval;
    public override float VortexAttractionRadius => m_vortexAttractionRadius;
    public override float VortexAttractionSpeed => m_vortexAttractionSpeed;
    public override float VortexMovementSpeed => m_vortexMovementSpeed;
    public override float VortexDamageMultiplier => m_vortexDamageMultiplier;
    public override float VortexTargetSearchInterval => m_vortexTargetSearchInterval;
    public override float TornadoInitialRadius => m_tornadoInitialRadius;
    public override float TornadoMaximumRadius => m_tornadoMaximumRadius;
    public override float TornadoGrowthDuration => m_tornadoGrowthDuration;
    public override int RockWhirlwindStoneCount => m_rockWhirlwindStoneCount;
    public override float RockWhirlwindOrbitRadius => m_rockWhirlwindOrbitRadius;
    public override float RockWhirlwindRotationSpeed => m_rockWhirlwindRotationSpeed;
    public override float RockWhirlwindDamageMultiplier => m_rockWhirlwindDamageMultiplier;
    public override float RockWhirlwindDamageInterval => m_rockWhirlwindDamageInterval;
    public override float RockWhirlwindGroupRadius => m_rockWhirlwindGroupRadius;
    public override float MudAvalancheWidth => m_mudAvalancheWidth;
    public override float MudAvalancheLength => m_mudAvalancheLength;
    public override float MudAvalancheTravelDistance => m_mudAvalancheTravelDistance;
    public override float MudAvalancheKnockbackSpeed => m_mudAvalancheKnockbackSpeed;
    public override float MudTrailDuration => m_mudTrailDuration;
    public override float MudTrailSlowPercent => m_mudTrailSlowPercent;
    public override float BlizzardFreezeInitialChance => m_blizzardFreezeInitialChance;
    public override float BlizzardFreezeChanceIncreasePerSecond => m_blizzardFreezeChanceIncreasePerSecond;
    public override float BlizzardFreezeMaximumChance => m_blizzardFreezeMaximumChance;
    public override float BlizzardFreezeAttemptInterval => m_blizzardFreezeAttemptInterval;
    public override float BlizzardFreezeDuration => m_blizzardFreezeDuration;

    public override Color ModifyProjectileColor(Color color)
    {
        return m_attackMode == ElementalAttackMode.Plant
            ? m_plantProjectileColor
            : color;
    }
    public override float ColossusWindupDuration => m_colossusWindupDuration;
    public override float ColossusKnockbackSpeed => m_colossusKnockbackSpeed;

    public override void ApplyOnHit(BaseEnemy enemy)
    {
        if (enemy == null) return;

        if (m_appliesBurn)
            enemy.ApplyBurn(m_burnDamagePerTick, m_burnDuration, m_burnTickInterval);

        if (m_appliesSlow && m_attackMode != ElementalAttackMode.ImpactArea)
            enemy.ApplySlow(m_slowPercent, m_slowDuration);

        if (m_freezeChance > 0f && Random.value < m_freezeChance)
            enemy.ApplyFreeze(m_freezeDuration);
    }
}
