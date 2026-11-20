using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player), typeof(PlayerStats))]
public class ElementalSpellController : MonoBehaviour
{
    [SerializeField] private ElementalRecipeCatalog m_catalog;

    [Header("Ataque elemental")]
    [SerializeField] private ElementalProjectile m_projectilePrefab;
    [SerializeField, Min(1)] private int m_baseDamage = 10;
    [SerializeField, Min(0.1f)] private float m_searchRange = 6f;
    [SerializeField, Min(0.1f)] private float m_fireInterval = 0.75f;
    [SerializeField, Min(0.1f)] private float m_projectileSpeed = 7f;
    [SerializeField, Min(0.1f)] private float m_projectileLifetime = 3f;
    [SerializeField, Min(0.1f)] private float m_projectileSizeMultiplier = 1f;
    [SerializeField, Min(1)] private int m_baseProjectileCount = 1;
    [SerializeField, Min(1)] private int m_maxProjectileCount = 10;
    [SerializeField, Min(0f)] private float m_projectileSpreadAngle = 12f;
    [SerializeField, Min(0.1f)] private float m_minimumFireInterval = 0.1f;

    [Header("Melhorias da magia atual")]
    [SerializeField] private int m_damageBonus;
    [SerializeField, Min(0f)] private float m_fireIntervalReduction;
    [SerializeField, Min(0f)] private float m_searchRangeBonus;
    [SerializeField, Min(0f)] private float m_projectileSpeedBonus;
    [SerializeField, Min(0f)] private float m_projectileSizeBonus;
    [SerializeField, Min(0)] private int m_projectileCountBonus;

    private Player m_player;
    private PlayerStats m_playerStats;
    private float m_nextFireTime;
    private bool m_hasReportedMissingPrefab;
    private ElementalFlamethrower m_flamethrower;
    private ElementalOrbitingFire m_orbitingFire;
    private ElementalInfernoEffect m_infernoEffect;
    private ElementalBlizzardEffect m_blizzard;
    private readonly List<Component> m_transientEffects = new();

    public ElementalRecipeCatalog Catalog => m_catalog;
    public int DamageBonus => m_damageBonus;
    public float FireIntervalReduction => m_fireIntervalReduction;
    public float SearchRangeBonus => m_searchRangeBonus;
    public float ProjectileSpeedBonus => m_projectileSpeedBonus;
    public float ProjectileSizeBonus => m_projectileSizeBonus;
    public int ProjectileCountBonus => m_projectileCountBonus;

    // Estes campos existem apenas nesta instância do jogador durante a partida.
    private ElementRecipe m_currentRecipe;
    private ElementalSpellDefinition m_currentDefinition;

    public ElementRecipe CurrentRecipe => m_currentRecipe;
    public ElementalSpellDefinition CurrentDefinition => m_currentDefinition;
    public int ElementsChosen => m_currentRecipe.TotalCount;
    public bool IsMagicUnlocked => ElementsChosen > 0 && m_currentDefinition != null;
    public bool CanChooseMoreElements => ElementsChosen < 3;
    public bool IsFinalEvolution => IsMagicUnlocked && ElementsChosen == 3;

    private void Awake()
    {
        m_player = GetComponent<Player>();
        m_playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!IsMagicUnlocked || m_player == null)
        {
            ClearPersistentEffects();
            ClearTransientEffects();
            return;
        }

        if (!m_player.IsAlive)
        {
            ClearPersistentEffects();
            ClearTransientEffects();
            return;
        }

        ElementalSpellBehavior behavior = m_currentDefinition.Behavior;
        ElementalAttackMode currentMode = behavior == null
            ? ElementalAttackMode.Projectile
            : behavior.AttackMode;
        if (currentMode == ElementalAttackMode.ContinuousCone ||
            currentMode == ElementalAttackMode.OrbitingFire ||
            currentMode == ElementalAttackMode.InfernoAura ||
            currentMode == ElementalAttackMode.Blizzard)
        {
            if (!EnsureProjectileVisualReferences(out Sprite sprite, out Material material,
                out int sortingLayerId, out int sortingOrder))
                return;

            int damage = behavior.ModifyDamage(GetCurrentDamage());
            Color color = m_catalog.GetRecipeColor(m_currentRecipe);

            if (currentMode == ElementalAttackMode.ContinuousCone)
            {
                ClearOrbitAndInferno();
                ClearBlizzard();
                if (m_flamethrower == null)
                {
                    GameObject effectObject = new GameObject("ContinuousFlamethrower");
                    m_flamethrower = effectObject.AddComponent<ElementalFlamethrower>();
                }
                m_flamethrower.Configure(m_player, damage, behavior.ForwardConeLength,
                    behavior.ForwardConeAngle, behavior.FlamethrowerVisualWidthMultiplier,
                    behavior.FlamethrowerBurstInterval,
                    behavior.FlamethrowerBurstDuration, behavior.FlamethrowerPulseCount,
                    behavior.FlamethrowerPulseInterval, behavior.FlamethrowerDamageMultiplier,
                    behavior.FlamethrowerTurnSpeed, color, material,
                    sortingLayerId, sortingOrder + 1);
            }
            else if (currentMode == ElementalAttackMode.OrbitingFire)
            {
                ClearFlamethrowerAndInferno();
                ClearBlizzard();
                if (m_orbitingFire == null)
                {
                    GameObject effectObject = new GameObject("OrbitingFireController");
                    m_orbitingFire = effectObject.AddComponent<ElementalOrbitingFire>();
                }

                int count = behavior.ModifyProjectileCount(GetCurrentProjectileCount());
                count = Mathf.Clamp(count, 1, Mathf.Max(1, m_maxProjectileCount));
                m_orbitingFire.Configure(m_player, damage, count,
                    behavior.OrbitRadius + m_searchRangeBonus,
                    behavior.OrbitSpeedDegrees, behavior.OrbitTickInterval,
                    behavior.OrbitDamageMultiplier, behavior.OrbitSize, color, sprite,
                    material, sortingLayerId, sortingOrder + 1);
            }
            else if (currentMode == ElementalAttackMode.Blizzard)
            {
                ClearFlamethrowerAndOrbit();
                if (m_infernoEffect != null) Destroy(m_infernoEffect.gameObject);
                m_infernoEffect = null;
                if (m_blizzard == null && Time.time >= m_nextFireTime)
                {
                    GameObject effectObject = new GameObject("BlizzardEffect");
                    m_blizzard = effectObject.AddComponent<ElementalBlizzardEffect>();
                    m_nextFireTime = Time.time + GetCurrentFireInterval();
                }
                if (m_blizzard != null)
                    m_blizzard.Configure(m_player, damage, behavior, sprite, material,
                        color, behavior.AreaRadius + m_searchRangeBonus,
                        sortingLayerId, sortingOrder + 1);
            }
            else
            {
                ClearFlamethrowerAndOrbit();
                ClearBlizzard();
                if (m_infernoEffect == null)
                {
                    GameObject effectObject = new GameObject("InfernoEffect");
                    m_infernoEffect = effectObject.AddComponent<ElementalInfernoEffect>();
                }
                m_infernoEffect.Configure(m_player, damage, behavior, sprite, material,
                    color, sortingLayerId, sortingOrder + 1);
            }

            return;
        }

        ClearPersistentEffects();
        CleanupTransientEffects();
        if (Time.time < m_nextFireTime) return;

        if (m_projectilePrefab == null)
        {
            if (!m_hasReportedMissingPrefab)
            {
                Debug.LogError("Associe o prefab ElementalProjectile no Inspector do ElementalSpellController.", this);
                m_hasReportedMissingPrefab = true;
            }
            return;
        }

        m_nextFireTime = Time.time + GetCurrentFireInterval();
        if (behavior != null && behavior.AttackMode == ElementalAttackMode.ForwardCone)
        {
            FireAt(null);
            return;
        }

        BaseEnemy target = FindClosestTarget();
        if (target == null) return;

        FireAt(target);
    }

    public bool TryChooseElement(ElementType element)
    {
        if (!CanChooseMoreElements) return false;

        if (m_catalog == null)
        {
            Debug.LogError("Associe o ElementalRecipeCatalog no Inspector do ElementalSpellController.", this);
            return false;
        }

        if (!m_currentRecipe.TryAdd(element, out ElementRecipe nextRecipe))
            return false;

        // Primeiro confirme que a receita existe; só depois altere o estado da partida.
        if (!m_catalog.TryFindDefinition(nextRecipe, out ElementalSpellDefinition nextDefinition))
        {
            Debug.LogError($"O catálogo não contém a receita: {nextRecipe}.", this);
            return false;
        }

        m_currentRecipe = nextRecipe;
        m_currentDefinition = nextDefinition;
        if (nextDefinition.Behavior == null ||
            (nextDefinition.Behavior.AttackMode != ElementalAttackMode.ContinuousCone &&
             nextDefinition.Behavior.AttackMode != ElementalAttackMode.OrbitingFire &&
             nextDefinition.Behavior.AttackMode != ElementalAttackMode.InfernoAura &&
             nextDefinition.Behavior.AttackMode != ElementalAttackMode.Blizzard))
            ClearPersistentEffects();
        return true;
    }

    private bool EnsureProjectileVisualReferences(out Sprite sprite, out Material material,
        out int sortingLayerId, out int sortingOrder)
    {
        sprite = null;
        material = null;
        sortingLayerId = 0;
        sortingOrder = 0;
        if (m_projectilePrefab == null)
        {
            if (!m_hasReportedMissingPrefab)
            {
                Debug.LogError("Associe o prefab ElementalProjectile no Inspector do ElementalSpellController.", this);
                m_hasReportedMissingPrefab = true;
            }
            return false;
        }

        SpriteRenderer renderer = m_projectilePrefab.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null || renderer.sharedMaterial == null)
        {
            Debug.LogError("O prefab ElementalProjectile precisa de SpriteRenderer, sprite e material.", this);
            return false;
        }

        sprite = renderer.sprite;
        material = renderer.sharedMaterial;
        sortingLayerId = renderer.sortingLayerID;
        sortingOrder = renderer.sortingOrder;
        return true;
    }

    private void ClearPersistentEffects()
    {
        ClearFlamethrowerAndOrbit();
        ClearFlamethrowerAndInferno();
        ClearOrbitAndInferno();
        ClearBlizzard();
    }

    private void ClearBlizzard()
    {
        if (m_blizzard != null) Destroy(m_blizzard.gameObject);
        m_blizzard = null;
    }

    private void CleanupTransientEffects()
    {
        for (int i = m_transientEffects.Count - 1; i >= 0; i--)
        {
            if (m_transientEffects[i] == null)
                m_transientEffects.RemoveAt(i);
        }
    }

    private void ClearTransientEffects()
    {
        foreach (Component effect in m_transientEffects)
        {
            if (effect != null) Destroy(effect.gameObject);
        }
        m_transientEffects.Clear();
    }

    private void ClearFlamethrowerAndOrbit()
    {
        if (m_flamethrower != null) Destroy(m_flamethrower.gameObject);
        if (m_orbitingFire != null) Destroy(m_orbitingFire.gameObject);
        m_flamethrower = null;
        m_orbitingFire = null;
    }

    private void ClearFlamethrowerAndInferno()
    {
        if (m_flamethrower != null) Destroy(m_flamethrower.gameObject);
        if (m_infernoEffect != null) Destroy(m_infernoEffect.gameObject);
        m_flamethrower = null;
        m_infernoEffect = null;
    }

    private void ClearOrbitAndInferno()
    {
        if (m_orbitingFire != null) Destroy(m_orbitingFire.gameObject);
        if (m_infernoEffect != null) Destroy(m_infernoEffect.gameObject);
        m_orbitingFire = null;
        m_infernoEffect = null;
    }

    private void OnDisable()
    {
        ClearPersistentEffects();
        ClearTransientEffects();
    }

    public bool TryPreviewElement(ElementType element, out ElementalSpellDefinition definition)
    {
        definition = null;
        if (!CanChooseMoreElements || m_catalog == null ||
            !m_currentRecipe.TryAdd(element, out ElementRecipe nextRecipe))
            return false;

        return m_catalog.TryFindDefinition(nextRecipe, out definition);
    }

    public bool CanApplyUpgrade(UpgradeEffect effect)
    {
        if (!IsMagicUnlocked) return false;

        switch (effect)
        {
            case UpgradeEffect.Damage:
            case UpgradeEffect.Range:
            case UpgradeEffect.ProjectileSpeed:
            case UpgradeEffect.ProjectileSize:
                return true;
            case UpgradeEffect.FireRate:
                return GetCurrentFireInterval() >
                    m_minimumFireInterval * GetCurrentAttackIntervalMultiplier();
            case UpgradeEffect.ProjectileCount:
                return GetCurrentProjectileCount() < Mathf.Max(1, m_maxProjectileCount);
            default:
                return false;
        }
    }

    public bool ApplyUpgrade(UpgradeEffect effect, float amount)
    {
        if (amount <= 0f || !CanApplyUpgrade(effect)) return false;

        switch (effect)
        {
            case UpgradeEffect.Damage:
                m_damageBonus += Mathf.Max(1, Mathf.RoundToInt(amount));
                break;
            case UpgradeEffect.FireRate:
                float availableReduction = GetCurrentFireInterval() -
                    m_minimumFireInterval * GetCurrentAttackIntervalMultiplier();
                if (availableReduction <= 0f) return false;
                m_fireIntervalReduction += Mathf.Min(amount, availableReduction) /
                    GetCurrentAttackIntervalMultiplier();
                break;
            case UpgradeEffect.Range:
                m_searchRangeBonus += amount;
                break;
            case UpgradeEffect.ProjectileSpeed:
                m_projectileSpeedBonus += amount;
                break;
            case UpgradeEffect.ProjectileSize:
                m_projectileSizeBonus += amount;
                break;
            case UpgradeEffect.ProjectileCount:
                int availableProjectiles = Mathf.Max(0, m_maxProjectileCount - GetCurrentProjectileCount());
                if (availableProjectiles == 0) return false;
                m_projectileCountBonus += Mathf.Min(availableProjectiles,
                    Mathf.Max(1, Mathf.RoundToInt(amount)));
                break;
            default:
                return false;
        }

        return true;
    }

    private BaseEnemy FindClosestTarget()
    {
        float range = GetCurrentSearchRange();

        float closestDistanceSquared = range * range;
        BaseEnemy closestEnemy = null;
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>();

        foreach (BaseEnemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;

            float distanceSquared = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distanceSquared > closestDistanceSquared) continue;

            closestDistanceSquared = distanceSquared;
            closestEnemy = enemy;
        }

        return closestEnemy;
    }

    private float GetCurrentSearchRange()
    {
        float range = Mathf.Max(0f, m_searchRange + m_searchRangeBonus);
        if (m_currentDefinition != null && m_currentDefinition.Behavior != null)
            range = m_currentDefinition.Behavior.ModifySearchRange(range);

        return range;
    }

    private void FireAt(BaseEnemy target)
    {
        ElementalSpellBehavior behavior = m_currentDefinition.Behavior;
        ElementalAttackMode attackMode = behavior == null
            ? ElementalAttackMode.Projectile
            : behavior.AttackMode;

        Vector2 direction = attackMode == ElementalAttackMode.ForwardCone
            ? m_player.FacingDirection
            : attackMode == ElementalAttackMode.Geyser
                ? Vector2.up
                : attackMode == ElementalAttackMode.Meteorite
                    ? Vector2.down
                    : (Vector2)target.transform.position - (Vector2)transform.position;
        if (direction == Vector2.zero) return;
        direction.Normalize();

        int damage = GetCurrentDamage();
        float speed = Mathf.Max(0.1f, m_projectileSpeed + m_projectileSpeedBonus);
        float lifetime = m_projectileLifetime;
        float sizeMultiplier = Mathf.Max(0.1f, m_projectileSizeMultiplier + m_projectileSizeBonus);
        if (behavior != null)
        {
            damage = behavior.ModifyDamage(damage);
            speed = behavior.ModifyProjectileSpeed(speed);
            lifetime = behavior.ModifyProjectileLifetime(lifetime);
        }

        int projectileCount = GetCurrentProjectileCount();
        if (behavior != null)
            projectileCount = behavior.ModifyProjectileCount(projectileCount);
        projectileCount = Mathf.Clamp(projectileCount, 1, Mathf.Max(1, m_maxProjectileCount));

        Color projectileColor = m_catalog.GetRecipeColor(m_currentRecipe);

        if (attackMode == ElementalAttackMode.SandstormVortex ||
            attackMode == ElementalAttackMode.Tornado)
        {
            if (!EnsureProjectileVisualReferences(out Sprite vortexSprite, out Material vortexMaterial,
                out int vortexLayer, out int vortexOrder)) return;

            float speedUpgrade = behavior.ModifyProjectileSpeed(m_projectileSpeed + m_projectileSpeedBonus) -
                                 behavior.ModifyProjectileSpeed(m_projectileSpeed);
            float vortexSpeed = Mathf.Max(0.1f, behavior.VortexMovementSpeed + speedUpgrade);
            float startingOffset = -m_projectileSpreadAngle * (projectileCount - 1) * 0.5f;
            for (int i = 0; i < projectileCount; i++)
            {
                Vector2 vortexDirection = Quaternion.Euler(0f, 0f,
                    startingOffset + m_projectileSpreadAngle * i) * direction;
                GameObject vortexObject = new GameObject(
                    attackMode == ElementalAttackMode.Tornado ? "Tornado" : "SandstormVortex");
                vortexObject.transform.position = transform.position;
                ElementalMovingVortex vortex = vortexObject.AddComponent<ElementalMovingVortex>();
                vortex.Configure(m_player, target, vortexDirection, damage, vortexSpeed,
                    GetCurrentSearchRange(), behavior, vortexSprite, vortexMaterial,
                    projectileColor, vortexLayer, vortexOrder + 1,
                    sizeMultiplier, behavior.VortexAttractionRadius + m_searchRangeBonus);
                m_transientEffects.Add(vortex);
            }
            return;
        }

        if (attackMode == ElementalAttackMode.RockWhirlwind)
        {
            if (!EnsureProjectileVisualReferences(out Sprite rockSprite, out Material rockMaterial,
                out int rockLayer, out int rockOrder)) return;

            GameObject whirlwindObject = new GameObject("RockWhirlwind");
            whirlwindObject.transform.position = transform.position;
            ElementalRockWhirlwind whirlwind = whirlwindObject.AddComponent<ElementalRockWhirlwind>();
            int stoneCount = Mathf.Clamp(behavior.RockWhirlwindStoneCount + m_projectileCountBonus, 1, 32);
            whirlwind.Configure(m_player, direction, damage, speed, stoneCount,
                behavior, rockSprite, rockMaterial, projectileColor, rockLayer, rockOrder + 1,
                sizeMultiplier, m_searchRangeBonus);
            m_transientEffects.Add(whirlwind);
            return;
        }

        if (attackMode == ElementalAttackMode.MudAvalanche)
        {
            if (!EnsureProjectileVisualReferences(out Sprite mudSprite, out Material mudMaterial,
                out int mudLayer, out int mudOrder)) return;

            float startOffset = -m_projectileSpreadAngle * (projectileCount - 1) * 0.5f;
            for (int i = 0; i < projectileCount; i++)
            {
                Vector2 mudDirection = Quaternion.Euler(0f, 0f,
                    startOffset + m_projectileSpreadAngle * i) * direction;
                GameObject avalancheObject = new GameObject("MudAvalanche");
                avalancheObject.transform.position = transform.position;
                ElementalMudAvalanche avalanche = avalancheObject.AddComponent<ElementalMudAvalanche>();
                avalanche.Initialize(mudDirection, damage, speed, behavior, mudSprite,
                    mudMaterial, mudLayer, mudOrder + 1, sizeMultiplier);
                m_transientEffects.Add(avalanche);
            }
            return;
        }

        if (attackMode == ElementalAttackMode.DeepSwamp ||
            attackMode == ElementalAttackMode.Colossus)
            projectileCount = 1;
        float startingAngle = -m_projectileSpreadAngle * (projectileCount - 1) * 0.5f;

        HashSet<BaseEnemy> plantVolleyHitEnemies = attackMode == ElementalAttackMode.Plant
            ? new HashSet<BaseEnemy>()
            : null;
        Vector3 spawnPosition = transform.position;
        if (target != null && (attackMode == ElementalAttackMode.StationaryArea ||
            attackMode == ElementalAttackMode.Geyser || attackMode == ElementalAttackMode.FragmentBurst ||
            attackMode == ElementalAttackMode.DeepSwamp ||
            attackMode == ElementalAttackMode.Colossus))
            spawnPosition = target.transform.position;
        else if (target != null && attackMode == ElementalAttackMode.Meteorite)
            spawnPosition = target.transform.position + Vector3.up * behavior.MeteorSpawnHeight;

        for (int i = 0; i < projectileCount; i++)
        {
            Vector2 projectileDirection = Quaternion.Euler(
                0f, 0f, startingAngle + m_projectileSpreadAngle * i) * direction;
            ElementalProjectile projectile = Instantiate(
                m_projectilePrefab, spawnPosition, Quaternion.identity);
            if (attackMode == ElementalAttackMode.ForwardCone ||
                attackMode == ElementalAttackMode.Geyser ||
                attackMode == ElementalAttackMode.FragmentBurst)
                projectileDirection = direction;
            Vector2 projectileScale = Vector2.one * sizeMultiplier;
            if (behavior != null)
                projectileScale = behavior.ModifyProjectileScale(projectileScale);

            projectile.Initialize(projectileDirection, damage, speed, lifetime,
                projectileColor, projectileScale, behavior, target, GetCurrentSearchRange(),
                plantVolleyHitEnemies);
        }
    }

    private int GetCurrentDamage()
    {
        long generalBonus = m_playerStats == null ? 0L : Mathf.RoundToInt(m_playerStats.DamageBonus);
        long damage = (long)m_baseDamage + m_damageBonus + generalBonus;
        return (int)System.Math.Min(System.Math.Max(damage, 1L), int.MaxValue);
    }

    private float GetCurrentFireInterval()
    {
        float generalReduction = m_playerStats == null ? 0f : m_playerStats.AttackIntervalReduction;
        float interval = Mathf.Max(m_minimumFireInterval,
            m_fireInterval - m_fireIntervalReduction - generalReduction);
        return interval * GetCurrentAttackIntervalMultiplier();
    }

    private float GetCurrentAttackIntervalMultiplier()
    {
        ElementalSpellBehavior behavior = m_currentDefinition == null
            ? null
            : m_currentDefinition.Behavior;
        return behavior == null ? 1f : Mathf.Max(0.1f, behavior.AttackIntervalMultiplier);
    }

    private int GetCurrentProjectileCount()
    {
        long count = (long)m_baseProjectileCount + m_projectileCountBonus;
        return (int)System.Math.Min(System.Math.Max(count, 1L), Mathf.Max(1, m_maxProjectileCount));
    }

    // Ferramenta simples de verificação, disponível no menu do componente no Inspector.
    [ContextMenu("Run Elemental Structure Checks")]
    private void RunStructureChecks()
    {
        if (m_catalog == null)
        {
            Debug.LogError("Associe o ElementalRecipeCatalog antes de executar os testes.", this);
            return;
        }

        bool catalogIsValid = m_catalog.ValidateCatalog(out string catalogReport);
        Debug.Log(catalogReport, this);

        ElementRecipe previousRecipe = m_currentRecipe;
        ElementalSpellDefinition previousDefinition = m_currentDefinition;
        bool controllerIsValid;

        try
        {
            m_currentRecipe = default;
            m_currentDefinition = null;

            bool startsWithoutMagic = !IsMagicUnlocked && ElementsChosen == 0;
            bool firstChoiceUnlocks = TryChooseElement(ElementType.Water) &&
                                      IsMagicUnlocked && ElementsChosen == 1;
            bool repeatedChoiceWorks = TryChooseElement(ElementType.Water) &&
                                       CurrentRecipe.WaterCount == 2 && ElementsChosen == 2;
            bool thirdChoiceEvolves = TryChooseElement(ElementType.Earth) &&
                                      IsFinalEvolution && ElementsChosen == 3;
            ElementRecipe completedRecipe = CurrentRecipe;
            bool fourthChoiceIsBlocked = !TryChooseElement(ElementType.Fire) &&
                                         CurrentRecipe.Equals(completedRecipe);

            controllerIsValid = startsWithoutMagic && firstChoiceUnlocks && repeatedChoiceWorks &&
                                thirdChoiceEvolves && fourthChoiceIsBlocked;
        }
        finally
        {
            // O teste não altera a receita atual da partida.
            m_currentRecipe = previousRecipe;
            m_currentDefinition = previousDefinition;
        }

        if (controllerIsValid && catalogIsValid)
            Debug.Log("Testes do controlador aprovados: inicia sem magia, desbloqueia na primeira escolha, aceita repetição, evolui na terceira e bloqueia a quarta.", this);
        else
            Debug.LogError("Um ou mais testes do controlador ou do catálogo falharam.", this);
    }
}
