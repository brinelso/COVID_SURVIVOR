using UnityEngine;

public enum ElementalAttackMode
{
    Projectile,
    MovingArea,
    StationaryArea,
    ImpactArea,
    ConeBurst,
    MudArea,
    MagmaImpact,
    Geyser,
    ExplosionImpact,
    FissureSequence,
    FragmentImpact,
    ForwardCone,
    FragmentBurst,
    Meteorite,
    ContinuousCone,
    OrbitingFire,
    InfernoAura,
    Tsunami,
    DeepSwamp,
    GlacialStorm,
    Plant,
    Colossus,
    SandstormVortex,
    RockWhirlwind,
    Tornado,
    MudAvalanche,
    Blizzard
}

// Define ajustes opcionais que uma receita aplica ao ataque genérico.
// As receitas sem comportamento usam os valores recebidos sem alterações.
public abstract class ElementalSpellBehavior : ScriptableObject
{
    public virtual int ModifyDamage(int damage) => damage;
    public virtual float ModifySearchRange(float searchRange) => searchRange;
    public virtual float ModifyProjectileSpeed(float speed) => speed;
    public virtual float ModifyProjectileLifetime(float lifetime) => lifetime;
    public virtual Vector2 ModifyProjectileScale(Vector2 scale) => scale;
    public virtual Color ModifyProjectileColor(Color color) => color;
    public virtual int ModifyProjectileCount(int count) => count;
    public virtual bool CanPierce => false;
    public virtual bool HomesInOnTarget => false;
    public virtual float HomingTurnSpeed => 0f;
    public virtual void ApplyOnHit(BaseEnemy enemy) { }
    public virtual ElementalAttackMode AttackMode => ElementalAttackMode.Projectile;
    public virtual float ActiveDuration => 0f;
    public virtual float AreaRadius => 0f;
    public virtual float DamageTickInterval => 0f;
    public virtual float ProgressiveSlowMaximumPercent => 0f;
    public virtual float ProgressiveSlowDuration => 0f;
    public virtual float ImpactGrowthStartScale => 0.25f;
    public virtual float ImpactGrowthDuration => 1f;
    public virtual float ConeLength => 0f;
    public virtual float ConeAngle => 0f;
    public virtual float ConeDamageMultiplier => 1f;
    public virtual float ConeVisualDuration => 0f;
    public virtual float MudProjectileLength => 1f;
    public virtual float MudProjectileWidth => 1f;
    public virtual float MudSlowPercent => 0f;
    public virtual float AreaDamageMultiplier => 1f;
    public virtual float ImpactDamageMultiplier => 1f;
    public virtual int FragmentCount => 0;
    public virtual float FragmentDamageMultiplier => 0.5f;
    public virtual float FragmentSpeed => 5f;
    public virtual float FragmentLifetime => 1f;
    public virtual float FragmentSize => 0.2f;
    public virtual float FragmentSpreadAngle => 360f;
    public virtual int SequenceCount => 0;
    public virtual float SequenceInterval => 0.25f;
    public virtual float SequenceBlastRadius => 0.75f;
    public virtual float GeyserWidth => 0.8f;
    public virtual float GeyserHeight => 2.8f;
    public virtual float GeyserRiseDuration => 0.2f;
    public virtual float ForwardConeLength => 2f;
    public virtual float ForwardConeAngle => 60f;
    public virtual float MeteorSpawnHeight => 3f;
    public virtual float FlamethrowerBurstInterval => 0.55f;
    public virtual float FlamethrowerBurstDuration => 0.3f;
    public virtual int FlamethrowerPulseCount => 3;
    public virtual float FlamethrowerPulseInterval => 0.1f;
    public virtual float FlamethrowerDamageMultiplier => 0.25f;
    public virtual float FlamethrowerTurnSpeed => 1440f;
    public virtual float FlamethrowerVisualWidthMultiplier => 1f;
    public virtual float OrbitRadius => 1.5f;
    public virtual float OrbitSpeedDegrees => 180f;
    public virtual float OrbitTickInterval => 0.5f;
    public virtual float OrbitDamageMultiplier => 0.25f;
    public virtual float OrbitSize => 0.65f;
    public virtual float InfernoAuraRadius => 3f;
    public virtual float InfernoAuraTickInterval => 1f;
    public virtual float InfernoAuraDamageMultiplier => 0.2f;
    public virtual float InfernoWaveInterval => 3f;
    public virtual float InfernoWaveRadius => 5f;
    public virtual float InfernoWaveExpansionSpeed => 8f;
    public virtual float InfernoWaveDamageMultiplier => 1.5f;
    public virtual float InfernoWaveLineWidth => 0.12f;
    public virtual float AttackIntervalMultiplier => 1f;
    public virtual float TsunamiWidth => 4f;
    public virtual float TsunamiTravelDistance => 12f;
    public virtual float TsunamiKnockbackSpeed => 3f;
    public virtual float AreaSlowPercent => 0f;
    public virtual float StormFreezeChance => 0f;
    public virtual float StormFreezeDuration => 1f;
    public virtual float GlacialTravelDistance => 12f;
    public virtual float GlacialSpeedMultiplier => 1f;
    public virtual float GlacialContactSpeedMultiplier => 0.4f;
    public virtual float GlacialFreezeAttemptInterval => 0.45f;
    public virtual float PlantImpactDamageMultiplier => 0.8f;
    public virtual float PlantSpreadRadius => 2f;
    public virtual int PlantMaximumEnemies => 6;
    public virtual float PlantRootDuration => 1f;
    public virtual float PlantPoisonDuration => 3f;
    public virtual float PlantPoisonDamageMultiplier => 0.25f;
    public virtual float PlantPoisonTickInterval => 0.5f;
    public virtual float ColossusWindupDuration => 0.75f;
    public virtual float ColossusKnockbackSpeed => 4f;
    public virtual float VortexAttractionRadius => 3f;
    public virtual float VortexAttractionSpeed => 3f;
    public virtual float VortexMovementSpeed => 3f;
    public virtual float VortexDamageMultiplier => 0.35f;
    public virtual float VortexTargetSearchInterval => 0.25f;
    public virtual float TornadoInitialRadius => 0.7f;
    public virtual float TornadoMaximumRadius => 2f;
    public virtual float TornadoGrowthDuration => 2f;
    public virtual float RockWhirlwindOrbitRadius => 1.2f;
    public virtual float RockWhirlwindRotationSpeed => 540f;
    public virtual int RockWhirlwindStoneCount => 8;
    public virtual float RockWhirlwindDamageMultiplier => 0.35f;
    public virtual float RockWhirlwindDamageInterval => 0.5f;
    public virtual float RockWhirlwindGroupRadius => 3f;
    public virtual float MudAvalancheWidth => 2.5f;
    public virtual float MudAvalancheLength => 2f;
    public virtual float MudAvalancheTravelDistance => 10f;
    public virtual float MudAvalancheKnockbackSpeed => 4f;
    public virtual float MudTrailDuration => 5f;
    public virtual float MudTrailSlowPercent => 0.5f;
    public virtual float BlizzardFreezeInitialChance => 0.05f;
    public virtual float BlizzardFreezeChanceIncreasePerSecond => 0.1f;
    public virtual float BlizzardFreezeMaximumChance => 0.5f;
    public virtual float BlizzardFreezeAttemptInterval => 1f;
    public virtual float BlizzardFreezeDuration => 1.25f;
}
