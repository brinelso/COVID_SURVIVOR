using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class ElementalProjectile : MonoBehaviour
{
    private Rigidbody2D m_rigidbody;
    private CapsuleCollider2D m_capsuleCollider;
    private CircleCollider2D m_circleCollider;
    private BoxCollider2D m_boxCollider;
    private SpriteRenderer m_spriteRenderer;
    private Sprite m_originalSprite;
    private Sprite m_mudProjectileSprite;

    private Vector2 m_direction;
    private int m_damage;
    private float m_speed;
    private bool m_isInitialized;
    private bool m_canPierce;
    private bool m_hasHit;

    private ElementalSpellBehavior m_behavior;
    private BaseEnemy m_target;
    private float m_targetSearchRange;
    private float m_homingTurnSpeed;

    private ElementalAttackMode m_attackMode;
    private bool m_isImpactArea;
    private bool m_isConeBurst;
    private bool m_isMudArea;
    private bool m_isMagmaImpact;
    private bool m_isGeyser;
    private bool m_isExplosionImpact;
    private bool m_isFissureSequence;
    private bool m_isFragmentImpact;
    private bool m_isForwardCone;
    private bool m_isFragmentBurst;
    private bool m_isMeteorite;
    private bool m_isTsunami;
    private bool m_isGlacialStorm;
    private bool m_isPlant;
    private bool m_isColossus;
    private bool m_mudImpactResolved;
    private bool m_specialImpactResolved;
    private bool m_areaActivated;
    private bool m_pendingImpact;
    private Vector2 m_pendingImpactPosition;
    private float m_flightRemaining;
    private float m_areaRemaining;
    private bool m_usesDamageArea;
    private bool m_movesAsArea;
    private Vector3 m_impactAreaFinalScale;
    private float m_impactGrowthStartScale;
    private float m_impactGrowthDuration;
    private float m_impactGrowthElapsed;
    private float m_areaTickInterval;
    private float m_areaRadius;
    private float m_geyserElapsed;
    private Vector3 m_geyserFinalScale;
    private float m_tsunamiDistanceRemaining;
    private float m_glacialTravelRemaining;
    private readonly Dictionary<BaseEnemy, float> m_nextDamageTime = new();
    private readonly Dictionary<BaseEnemy, float> m_nextFreezeAttemptTime = new();
    private readonly Dictionary<BaseEnemy, HashSet<Collider2D>> m_areaContacts = new();
    private readonly HashSet<BaseEnemy> m_hitEnemies = new();
    private readonly HashSet<BaseEnemy> m_activationHitEnemies = new();
    private readonly List<BaseEnemy> m_staleEnemies = new();
    private HashSet<BaseEnemy> m_plantVolleyHitEnemies;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_capsuleCollider = GetComponent<CapsuleCollider2D>();
        m_circleCollider = GetComponent<CircleCollider2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_originalSprite = m_spriteRenderer.sprite;

        m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_rigidbody.gravityScale = 0f;
        m_rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        m_capsuleCollider.direction = CapsuleDirection2D.Horizontal;
        m_capsuleCollider.isTrigger = true;
        m_circleCollider.isTrigger = true;
        m_circleCollider.enabled = false;
    }

    public void Initialize(Vector2 direction, int damage, float speed, float lifetime, Color color,
        Vector2 scaleMultiplier, ElementalSpellBehavior behavior, BaseEnemy target,
        float targetSearchRange, HashSet<BaseEnemy> plantVolleyHitEnemies = null)
    {
        if (direction == Vector2.zero || damage <= 0 || speed <= 0f || lifetime <= 0f ||
            scaleMultiplier.x <= 0f || scaleMultiplier.y <= 0f)
        {
            Debug.LogError("Direção, dano, velocidade, escala e tempo de vida do projétil elemental devem ser válidos.", this);
            Destroy(gameObject);
            return;
        }

        m_attackMode = behavior == null
            ? ElementalAttackMode.Projectile
            : behavior.AttackMode;
        m_isImpactArea = m_attackMode == ElementalAttackMode.ImpactArea;
        m_isConeBurst = m_attackMode == ElementalAttackMode.ConeBurst;
        m_isMudArea = m_attackMode == ElementalAttackMode.MudArea;
        m_isMagmaImpact = m_attackMode == ElementalAttackMode.MagmaImpact;
        m_isGeyser = m_attackMode == ElementalAttackMode.Geyser;
        m_isExplosionImpact = m_attackMode == ElementalAttackMode.ExplosionImpact;
        m_isFissureSequence = m_attackMode == ElementalAttackMode.FissureSequence;
        m_isFragmentImpact = m_attackMode == ElementalAttackMode.FragmentImpact;
        m_isForwardCone = m_attackMode == ElementalAttackMode.ForwardCone;
        m_isFragmentBurst = m_attackMode == ElementalAttackMode.FragmentBurst;
        m_isMeteorite = m_attackMode == ElementalAttackMode.Meteorite;
        m_isTsunami = m_attackMode == ElementalAttackMode.Tsunami;
        bool isDeepSwamp = m_attackMode == ElementalAttackMode.DeepSwamp;
        m_isGlacialStorm = m_attackMode == ElementalAttackMode.GlacialStorm;
        m_isPlant = m_attackMode == ElementalAttackMode.Plant;
        m_isColossus = m_attackMode == ElementalAttackMode.Colossus;
        m_usesDamageArea = m_attackMode == ElementalAttackMode.MovingArea ||
                           m_attackMode == ElementalAttackMode.StationaryArea || m_isGeyser ||
                           isDeepSwamp || m_isGlacialStorm;
        m_movesAsArea = m_attackMode == ElementalAttackMode.MovingArea || m_isGlacialStorm;

        bool isAreaAttack = m_usesDamageArea || m_isImpactArea || m_isMudArea ||
                            m_isMagmaImpact || m_isMeteorite || m_isExplosionImpact ||
                            m_isFragmentBurst || m_isGlacialStorm || m_isColossus;
        float activeDuration = m_usesDamageArea ? behavior.ActiveDuration : lifetime;
        m_areaRadius = isAreaAttack
            ? behavior.AreaRadius * (m_isGlacialStorm ? scaleMultiplier.x : 1f)
            : 0f;
        m_areaTickInterval = isAreaAttack ? behavior.DamageTickInterval : 0f;
        if ((m_usesDamageArea || m_isImpactArea || m_isMagmaImpact || m_isMeteorite) &&
            (m_areaRadius <= 0f || m_areaTickInterval <= 0f ||
             ((m_usesDamageArea && !m_isGlacialStorm || m_isMeteorite) &&
              behavior.ActiveDuration <= 0f) ||
             (m_isImpactArea && (behavior.ActiveDuration <= 0f || speed <= 0f))))
        {
            Debug.LogError("Duração, raio e intervalo de dano da área precisam ser maiores que zero.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isMudArea && (m_areaRadius <= 0f || behavior.ActiveDuration <= 0f ||
            behavior.MudSlowPercent <= 0f || behavior.MudProjectileLength <= 0f ||
            behavior.MudProjectileWidth <= 0f))
        {
            Debug.LogError("Os parâmetros da área e do projétil de lama precisam ser maiores que zero.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isConeBurst && (behavior.ConeLength <= 0f || behavior.ConeAngle <= 0f ||
            behavior.ConeVisualDuration <= 0f))
        {
            Debug.LogError("Os parâmetros do cone de vapor precisam ser maiores que zero.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isExplosionImpact && (m_areaRadius <= 0f || behavior.ConeVisualDuration <= 0f))
        {
            Debug.LogError("O raio e a duração visual da explosão precisam ser válidos.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isForwardCone && (behavior.ForwardConeLength <= 0f ||
            behavior.ForwardConeAngle <= 0f || behavior.ConeVisualDuration <= 0f))
        {
            Debug.LogError("Os parâmetros do cone frontal precisam ser válidos.", this);
            Destroy(gameObject);
            return;
        }

        if (behavior != null && behavior.ProgressiveSlowMaximumPercent > 0f &&
            behavior.ProgressiveSlowDuration <= 0f)
        {
            Debug.LogError("A duração da lentidão progressiva precisa ser maior que zero.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isImpactArea && (behavior.ImpactGrowthStartScale <= 0f ||
            behavior.ImpactGrowthStartScale > 1f || behavior.ImpactGrowthDuration <= 0f))
        {
            Debug.LogError("A escala inicial e a duração do crescimento da área precisam ser válidas.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isGeyser && (behavior.ActiveDuration <= 0f || behavior.DamageTickInterval <= 0f ||
            behavior.GeyserWidth <= 0f || behavior.GeyserHeight <= 0f ||
            behavior.GeyserRiseDuration <= 0f))
        {
            Debug.LogError("Os parâmetros da coluna do Gêiser precisam ser maiores que zero.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isFissureSequence && (target == null || behavior.SequenceCount <= 0 ||
            behavior.SequenceInterval <= 0f || behavior.SequenceBlastRadius <= 0f))
        {
            Debug.LogError("O ataque de fissura precisa de alvo e parâmetros válidos.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isTsunami && (behavior.TsunamiWidth <= 0f ||
            behavior.TsunamiTravelDistance <= 0f || behavior.TsunamiKnockbackSpeed < 0f))
        {
            Debug.LogError("Os parâmetros do Tsunami precisam ser válidos.", this);
            Destroy(gameObject);
            return;
        }

        if (isDeepSwamp &&
            (behavior.ActiveDuration <= 0f || behavior.AreaRadius <= 0f ||
             behavior.DamageTickInterval <= 0f))
        {
            Debug.LogError("Duração, raio e intervalo da área precisam ser maiores que zero.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isGlacialStorm && (behavior.AreaRadius <= 0f ||
            behavior.DamageTickInterval <= 0f || behavior.GlacialTravelDistance <= 0f ||
            behavior.GlacialSpeedMultiplier <= 0f || behavior.GlacialSpeedMultiplier > 1f ||
            behavior.GlacialContactSpeedMultiplier <= 0f ||
            behavior.GlacialContactSpeedMultiplier > 1f || behavior.StormFreezeChance < 0f ||
            behavior.StormFreezeDuration <= 0f || behavior.GlacialFreezeAttemptInterval <= 0f))
        {
            Debug.LogError("Os parâmetros da Tempestade Glacial precisam ser válidos.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isPlant && (behavior.PlantImpactDamageMultiplier < 0f ||
            behavior.PlantSpreadRadius <= 0f || behavior.PlantMaximumEnemies <= 0 ||
            behavior.PlantRootDuration <= 0f || behavior.PlantPoisonDuration <= 0f ||
            behavior.PlantPoisonDamageMultiplier < 0f || behavior.PlantPoisonTickInterval <= 0f))
        {
            Debug.LogError("Os parâmetros de propagação, raízes e veneno da Planta precisam ser válidos.", this);
            Destroy(gameObject);
            return;
        }

        if (m_isColossus && (behavior.AreaRadius <= 0f || behavior.ColossusWindupDuration < 0f ||
            behavior.ImpactDamageMultiplier < 0f))
        {
            Debug.LogError("Os parâmetros do Colosso precisam ser válidos.", this);
            Destroy(gameObject);
            return;
        }

        m_direction = direction.normalized;
        m_damage = damage;
        m_speed = m_isGlacialStorm ? speed * behavior.GlacialSpeedMultiplier : speed;
        m_behavior = behavior;
        m_canPierce = behavior != null && behavior.CanPierce;
        m_target = behavior != null && behavior.HomesInOnTarget ? target : null;
        m_targetSearchRange = Mathf.Max(0f, targetSearchRange);
        m_homingTurnSpeed = behavior == null ? 0f : Mathf.Max(0f, behavior.HomingTurnSpeed);
        m_plantVolleyHitEnemies = plantVolleyHitEnemies;

        if (m_isGeyser)
        {
            ConfigureGeyser(behavior);
        }
        else if (m_usesDamageArea)
        {
            ConfigureAreaShape();
        }
        else if (m_isImpactArea)
        {
            m_capsuleCollider.enabled = true;
            m_circleCollider.enabled = false;
            m_impactGrowthStartScale = behavior.ImpactGrowthStartScale;
            m_impactGrowthDuration = behavior.ImpactGrowthDuration;
            m_impactAreaFinalScale = CalculateAreaScale();
            transform.localScale = m_impactAreaFinalScale * m_impactGrowthStartScale;
            transform.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg);
        }
        else if (m_isMudArea)
        {
            ConfigureMudProjectile(behavior);
        }
        else if (m_isTsunami)
        {
            ConfigureTsunami(behavior, scaleMultiplier);
            m_areaActivated = true;
        }
        else if (m_isColossus || m_isFissureSequence ||
                 m_isForwardCone || m_isFragmentBurst)
        {
            m_capsuleCollider.enabled = false;
            m_circleCollider.enabled = false;
            if (m_isColossus)
                transform.localScale = new Vector3(0.2f, 0.3f, 1f);
        }
        else
        {
            m_circleCollider.enabled = false;
            m_capsuleCollider.enabled = true;
            transform.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg);
            transform.localScale = Vector3.Scale(transform.localScale,
                new Vector3(scaleMultiplier.x, scaleMultiplier.y, 1f));
        }

        m_spriteRenderer.color = m_isMudArea
            ? new Color(0.43f, 0.25f, 0.11f, 1f)
            : behavior == null ? color : behavior.ModifyProjectileColor(color);
        m_isInitialized = true;
        if (m_isGlacialStorm)
            ShowGlacialVisual();
        if (m_isGeyser)
        {
            m_areaActivated = true;
            m_areaRemaining = behavior.ActiveDuration;
        }
        else if (m_isFissureSequence)
        {
            StartCoroutine(RunFissureSequence(target.transform.position));
        }
        else if (m_isForwardCone)
        {
            ActivateForwardCone();
        }
        else if (m_isFragmentBurst)
        {
            ActivateFragmentBurst();
        }
        else if (m_isGlacialStorm)
        {
            m_areaActivated = true;
            m_glacialTravelRemaining = behavior.GlacialTravelDistance;
        }
        else if (m_isColossus)
        {
            StartCoroutine(RunColossusAttack());
        }
        else if (m_isTsunami)
        {
            m_tsunamiDistanceRemaining = behavior.TsunamiTravelDistance;
        }
        else if (m_usesDamageArea)
        {
            m_areaActivated = true;
            m_areaRemaining = behavior.ActiveDuration;
        }
        else if (m_isImpactArea || m_isMudArea || m_isMagmaImpact || m_isMeteorite)
            m_flightRemaining = lifetime;
        else
            Destroy(gameObject, activeDuration);
    }

    private void Update()
    {
        if (m_isInitialized && (m_usesDamageArea || m_canPierce || m_isMudArea ||
            m_isMagmaImpact || m_isMeteorite))
            RemoveDestroyedEnemyReferences();

        if (!m_isInitialized) return;

        if (m_isGeyser && m_areaActivated)
        {
            m_areaRemaining -= Time.deltaTime;
            UpdateGeyserAppearance();
            if (m_areaRemaining <= 0f)
                Destroy(gameObject);
            return;
        }

        if (m_usesDamageArea && !m_isGeyser && !m_isGlacialStorm)
        {
            m_areaRemaining -= Time.deltaTime;
            if (m_areaRemaining <= 0f)
                Destroy(gameObject);
            return;
        }

        if (!m_isImpactArea && !m_isMudArea && !m_isMagmaImpact && !m_isMeteorite) return;

        if (!m_areaActivated)
        {
            if (!m_pendingImpact)
            {
                if (m_isImpactArea)
                    UpdateImpactGrowth();
                m_flightRemaining -= Time.deltaTime;
                if (m_flightRemaining <= 0f)
                    Destroy(gameObject);
            }

            return;
        }

        m_areaRemaining -= Time.deltaTime;
        if (m_areaRemaining <= 0f)
            Destroy(gameObject);
    }

    private void UpdateImpactGrowth()
    {
        m_impactGrowthElapsed = Mathf.Min(
            m_impactGrowthDuration, m_impactGrowthElapsed + Time.deltaTime);
        float progress = m_impactGrowthElapsed / m_impactGrowthDuration;
        float scaleFactor = Mathf.Lerp(m_impactGrowthStartScale, 1f, progress);
        transform.localScale = m_impactAreaFinalScale * scaleFactor;
    }

    private void FixedUpdate()
    {
        if (!m_isInitialized) return;

        if (m_pendingImpact)
        {
            if (m_isMudArea)
                ActivateMudArea();
            else
                ActivateImpactArea();
            return;
        }

        if (m_isTsunami)
        {
            float travelDistance = m_speed * Time.fixedDeltaTime;
            m_rigidbody.MovePosition(m_rigidbody.position + m_direction * travelDistance);
            m_tsunamiDistanceRemaining -= travelDistance;
            if (m_tsunamiDistanceRemaining <= 0f)
                Destroy(gameObject);
            return;
        }

        if (m_isGlacialStorm)
        {
            float speedMultiplier = m_areaContacts.Count > 0
                ? m_behavior.GlacialContactSpeedMultiplier
                : 1f;
            float travelDistance = m_speed * speedMultiplier * Time.fixedDeltaTime;
            m_rigidbody.MovePosition(m_rigidbody.position + m_direction * travelDistance);
            m_glacialTravelRemaining -= travelDistance;
            if (m_glacialTravelRemaining <= 0f)
                Destroy(gameObject);
            return;
        }

        if (m_hasHit || (m_usesDamageArea && !m_movesAsArea) ||
            (m_isMudArea && m_areaActivated))
            return;

        if (!m_usesDamageArea && m_behavior != null && m_behavior.HomesInOnTarget &&
            !UpdateHomingDirection())
        {
            Destroy(gameObject);
            return;
        }

        m_rigidbody.MovePosition(
            m_rigidbody.position + m_direction * m_speed * Time.fixedDeltaTime);
    }

    private bool UpdateHomingDirection()
    {
        if (m_target == null || !m_target.IsAlive || m_hitEnemies.Contains(m_target))
        {
            m_target = null;
            m_target = FindClosestLivingEnemy();
        }

        if (m_target == null) return false;

        Vector2 targetDirection = (Vector2)m_target.transform.position - m_rigidbody.position;
        if (targetDirection == Vector2.zero) return true;

        float angleDifference = Vector2.SignedAngle(m_direction, targetDirection.normalized);
        float maximumTurn = m_homingTurnSpeed * Time.fixedDeltaTime;
        float turn = Mathf.Clamp(angleDifference, -maximumTurn, maximumTurn);
        m_direction = (Quaternion.Euler(0f, 0f, turn) * m_direction).normalized;
        transform.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg);
        return true;
    }

    private BaseEnemy FindClosestLivingEnemy()
    {
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>();
        BaseEnemy closestEnemy = null;
        float closestDistanceSquared = m_targetSearchRange * m_targetSearchRange;

        foreach (BaseEnemy enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive || m_hitEnemies.Contains(enemy)) continue;

            float distanceSquared = ((Vector2)enemy.transform.position - m_rigidbody.position).sqrMagnitude;
            if (distanceSquared > closestDistanceSquared) continue;

            closestDistanceSquared = distanceSquared;
            closestEnemy = enemy;
        }

        return closestEnemy;
    }

    private void ConfigureAreaShape()
    {
        m_capsuleCollider.enabled = false;
        m_circleCollider.enabled = true;
        m_circleCollider.radius = 0.5f;

        transform.localScale = CalculateAreaScale();
    }

    private void ConfigureTsunami(ElementalSpellBehavior behavior, Vector2 scaleMultiplier)
    {
        m_capsuleCollider.enabled = false;
        m_circleCollider.enabled = false;
        m_boxCollider = GetComponent<BoxCollider2D>();
        if (m_boxCollider == null)
            m_boxCollider = gameObject.AddComponent<BoxCollider2D>();

        m_boxCollider.isTrigger = true;
        m_boxCollider.size = Vector2.one;
        m_boxCollider.enabled = true;
        transform.localScale = new Vector3(
            1.4f * scaleMultiplier.x,
            behavior.TsunamiWidth * scaleMultiplier.y,
            1f);
        transform.rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg);
    }

    private void ConfigureGeyser(ElementalSpellBehavior behavior)
    {
        m_capsuleCollider.enabled = false;
        m_circleCollider.enabled = false;
        m_boxCollider = GetComponent<BoxCollider2D>();
        if (m_boxCollider == null)
            m_boxCollider = gameObject.AddComponent<BoxCollider2D>();
        m_boxCollider.isTrigger = true;
        m_boxCollider.size = Vector2.one;
        m_boxCollider.enabled = true;
        m_geyserFinalScale = new Vector3(behavior.GeyserWidth, behavior.GeyserHeight, 1f);
        transform.localScale = m_geyserFinalScale * 0.1f;
        transform.rotation = Quaternion.identity;
    }

    private void UpdateGeyserAppearance()
    {
        m_geyserElapsed += Time.deltaTime;
        float rise = Mathf.Clamp01(m_geyserElapsed / m_behavior.GeyserRiseDuration);
        float fade = m_areaRemaining < m_behavior.GeyserRiseDuration
            ? Mathf.Clamp01(m_areaRemaining / m_behavior.GeyserRiseDuration)
            : 1f;
        float height = Mathf.Min(rise, fade);
        transform.localScale = new Vector3(
            m_geyserFinalScale.x * Mathf.Max(height, 0.1f),
            m_geyserFinalScale.y * height,
            1f);
    }

    private void ConfigureMudProjectile(ElementalSpellBehavior behavior)
    {
        m_capsuleCollider.enabled = false;
        m_circleCollider.enabled = false;

        m_boxCollider = GetComponent<BoxCollider2D>();
        if (m_boxCollider == null)
            m_boxCollider = gameObject.AddComponent<BoxCollider2D>();
        m_boxCollider.isTrigger = true;
        m_boxCollider.size = Vector2.one;
        m_boxCollider.enabled = true;

        m_mudProjectileSprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 2f, 2f),
            new Vector2(0.5f, 0.5f),
            2f);
        m_spriteRenderer.sprite = m_mudProjectileSprite;
        m_spriteRenderer.color = new Color(0.43f, 0.25f, 0.11f, 1f);
        transform.localScale = new Vector3(
            behavior.MudProjectileLength, behavior.MudProjectileWidth, 1f);
        transform.localRotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg);
    }

    private Vector3 CalculateAreaScale()
    {
        Vector2 spriteSize = m_spriteRenderer.sprite.bounds.size;
        return new Vector3(
            m_areaRadius * 2f / spriteSize.x,
            m_areaRadius * 2f / spriteSize.y,
            1f);
    }

    private void ActivateImpactArea()
    {
        m_pendingImpact = false;
        m_areaActivated = true;
        m_usesDamageArea = true;
        m_movesAsArea = false;
        m_areaRemaining = m_behavior.ActiveDuration;
        m_nextDamageTime.Clear();

        transform.position = m_pendingImpactPosition;
        transform.rotation = Quaternion.identity;
        m_rigidbody.position = m_pendingImpactPosition;
        ConfigureAreaShape();
    }

    private void ActivateMudArea()
    {
        m_pendingImpact = false;
        m_areaActivated = true;
        m_areaRemaining = m_behavior.ActiveDuration;

        transform.position = m_pendingImpactPosition;
        transform.rotation = Quaternion.identity;
        m_rigidbody.position = m_pendingImpactPosition;
        m_boxCollider.enabled = false;
        m_spriteRenderer.sprite = m_originalSprite;
        m_spriteRenderer.color = new Color(0.43f, 0.25f, 0.11f, 0.8f);
        m_spriteRenderer.sortingOrder = Mathf.Min(m_spriteRenderer.sortingOrder, 0);
        ConfigureAreaShape();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleTrigger(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (m_usesDamageArea || (m_isMudArea && m_areaActivated))
            HandleTrigger(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!m_areaActivated) return;

        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy != null)
            UnregisterAreaContact(enemy, other);
    }

    private void HandleTrigger(Collider2D other)
    {
        if (!m_isInitialized || m_hasHit) return;

        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null || !enemy.IsAlive) return;

        if (m_isTsunami)
        {
            if (!m_hitEnemies.Add(enemy)) return;
            enemy.TakeDamage(m_damage);
            if (enemy.IsAlive && m_behavior.TsunamiKnockbackSpeed > 0f)
                enemy.ApplyKnockback(m_direction, m_behavior.TsunamiKnockbackSpeed, 0.35f);
            return;
        }

        if (m_isPlant)
        {
            ApplyPlantChain(enemy);
            return;
        }

        if ((m_isImpactArea || m_isMudArea || m_isMeteorite) &&
            !m_areaActivated)
        {
            if (!m_pendingImpact)
            {
                m_pendingImpact = true;
                m_pendingImpactPosition = other.ClosestPoint(transform.position);
                if (m_isMudArea)
                    ApplyMudImpact(enemy);
                else if (m_isMeteorite)
                    ApplyMeteoriteImpact(enemy);
            }
            return;
        }

        if (m_isMagmaImpact)
        {
            ApplyMagmaImpact(other, enemy);
            return;
        }

        if (m_isConeBurst)
        {
            ApplySteamCone(other, enemy);
            return;
        }

        if (m_isExplosionImpact)
        {
            ApplyExplosionImpact(other, enemy);
            return;
        }

        if (m_isFragmentImpact)
        {
            ApplyFragmentImpact(other, enemy);
            return;
        }

        if (m_isMudArea && m_areaActivated)
        {
            RegisterAreaContact(enemy, other);
            return;
        }

        if (m_usesDamageArea)
        {
            RegisterAreaContact(enemy, other);
            TryDamageAreaTarget(enemy);
            if (m_isGlacialStorm)
                TryApplyGlacialFreeze(enemy);
            return;
        }

        if (!m_hitEnemies.Add(enemy)) return;

        enemy.TakeDamage(m_damage);
        if (enemy.IsAlive)
            m_behavior?.ApplyOnHit(enemy);

        if (!m_canPierce)
        {
            m_hasHit = true;
            Destroy(gameObject);
        }
    }

    private void ApplyPlantChain(BaseEnemy firstEnemy)
    {
        if (firstEnemy == null || !firstEnemy.IsAlive) return;

        m_specialImpactResolved = true;
        m_hasHit = true;
        HashSet<BaseEnemy> hitEnemies = m_plantVolleyHitEnemies ?? m_activationHitEnemies;
        if (hitEnemies.Count >= m_behavior.PlantMaximumEnemies || !hitEnemies.Add(firstEnemy))
        {
            Destroy(gameObject);
            return;
        }

        Queue<Vector2> propagationQueue = new();
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>();
        Vector2 firstPosition = firstEnemy.transform.position;
        ApplyPlantEffect(firstEnemy);
        ShowRootLine(transform.position, firstPosition);
        propagationQueue.Enqueue(firstPosition);

        float spreadRadiusSquared = m_behavior.PlantSpreadRadius * m_behavior.PlantSpreadRadius;
        while (propagationQueue.Count > 0 &&
               hitEnemies.Count < m_behavior.PlantMaximumEnemies)
        {
            Vector2 sourcePosition = propagationQueue.Dequeue();

            foreach (BaseEnemy candidate in enemies)
            {
                if (hitEnemies.Count >= m_behavior.PlantMaximumEnemies) break;
                if (candidate == null || !candidate.IsAlive || hitEnemies.Contains(candidate))
                    continue;

                Vector2 candidatePosition = candidate.transform.position;
                if ((candidatePosition - sourcePosition).sqrMagnitude > spreadRadiusSquared)
                    continue;

                if (!hitEnemies.Add(candidate)) continue;
                ApplyPlantEffect(candidate);
                ShowRootLine(sourcePosition, candidatePosition);
                propagationQueue.Enqueue(candidatePosition);
            }
        }

        Destroy(gameObject);
    }

    private void ApplyPlantEffect(BaseEnemy enemy)
    {
        int impactDamage = Mathf.Max(1,
            Mathf.RoundToInt(m_damage * m_behavior.PlantImpactDamageMultiplier));
        enemy.TakeDamage(impactDamage);
        if (!enemy.IsAlive) return;

        int poisonDamage = Mathf.Max(1,
            Mathf.RoundToInt(m_damage * m_behavior.PlantPoisonDamageMultiplier));
        enemy.ApplyFreeze(m_behavior.PlantRootDuration);
        enemy.ApplyPoison(poisonDamage, m_behavior.PlantPoisonDuration,
            m_behavior.PlantPoisonTickInterval);
        ShowBurst(enemy.transform.position, 0.24f, 0.25f);
    }

    private void ShowRootLine(Vector2 start, Vector2 end)
    {
        ElementalBurstVisual.ShowLine(m_originalSprite, m_spriteRenderer.sharedMaterial,
            start, end, 0.09f, m_behavior.PlantRootDuration,
            m_spriteRenderer.color, m_spriteRenderer.sortingLayerID,
            m_spriteRenderer.sortingOrder + 1);
    }

    private void ShowGlacialVisual()
    {
        Vector2 center = transform.position;
        Vector2[] directions = { Vector2.right, Vector2.up, Vector2.one.normalized,
            new Vector2(1f, -1f).normalized };

        foreach (Vector2 direction in directions)
        {
            ElementalBurstVisual.ShowLine(m_originalSprite, m_spriteRenderer.sharedMaterial,
                center - direction * m_areaRadius, center + direction * m_areaRadius,
                0.045f, 0.35f, m_spriteRenderer.color,
                m_spriteRenderer.sortingLayerID, m_spriteRenderer.sortingOrder + 1);
        }
    }

    private void ApplySteamCone(Collider2D impactCollider, BaseEnemy directTarget)
    {
        if (!m_hitEnemies.Add(directTarget)) return;
        m_hasHit = true;

        Vector2 impactPoint = impactCollider.ClosestPoint(transform.position);
        directTarget.TakeDamage(m_damage);
        if (directTarget.IsAlive)
            m_behavior?.ApplyOnHit(directTarget);

        float coneLength = m_behavior.ConeLength;
        float coneAngle = m_behavior.ConeAngle;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(impactPoint, coneLength);
        int coneDamage = Mathf.Max(0,
            Mathf.RoundToInt(m_damage * m_behavior.ConeDamageMultiplier));
        if (coneDamage > 0)
            ApplyDamageInCone(impactPoint, m_direction, coneLength, coneAngle, coneDamage, directTarget);

        ElementalConeVisual.Show(impactPoint, m_direction, coneLength, coneAngle,
            m_behavior.ConeVisualDuration, m_spriteRenderer.sharedMaterial,
            m_spriteRenderer.sortingLayerID, m_spriteRenderer.sortingOrder + 1);
        Destroy(gameObject);
    }

    private void ApplyMudImpact(BaseEnemy directTarget)
    {
        if (m_mudImpactResolved || directTarget == null || !directTarget.IsAlive) return;
        m_mudImpactResolved = true;

        m_hitEnemies.Add(directTarget);
        directTarget.TakeDamage(m_damage);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            m_pendingImpactPosition, m_areaRadius);
        foreach (Collider2D collider in colliders)
        {
            BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
            if (enemy == null || !enemy.IsAlive || !m_hitEnemies.Add(enemy)) continue;

            Vector2 toEnemy = (Vector2)enemy.transform.position - m_pendingImpactPosition;
            if (toEnemy.magnitude > m_areaRadius)
            {
                m_hitEnemies.Remove(enemy);
                continue;
            }

            enemy.TakeDamage(m_damage);
        }
    }

    private void ApplyMagmaImpact(Collider2D impactCollider, BaseEnemy directTarget)
    {
        if (m_specialImpactResolved || directTarget == null || !directTarget.IsAlive) return;
        m_specialImpactResolved = true;
        m_activationHitEnemies.Add(directTarget);
        directTarget.TakeDamage(m_damage);

        Vector2 impactPoint = impactCollider.ClosestPoint(transform.position);
        int explosionDamage = Mathf.Max(1,
            Mathf.RoundToInt(m_damage * m_behavior.ImpactDamageMultiplier));
        DamageEnemiesInCircle(impactPoint, m_areaRadius, explosionDamage,
            directTarget, m_activationHitEnemies);
        ShowBurst(impactPoint, m_areaRadius, m_behavior.ConeVisualDuration);
        Destroy(gameObject);
    }

    private void ApplyMeteoriteImpact(BaseEnemy directTarget)
    {
        if (m_specialImpactResolved || directTarget == null || !directTarget.IsAlive) return;
        m_specialImpactResolved = true;
        m_activationHitEnemies.Add(directTarget);

        directTarget.TakeDamage(m_damage);
        int explosionDamage = Mathf.Max(1,
            Mathf.RoundToInt(m_damage * m_behavior.ImpactDamageMultiplier));
        DamageEnemiesInCircle(m_pendingImpactPosition, m_areaRadius, explosionDamage,
            directTarget, m_activationHitEnemies);
        ShowBurst(m_pendingImpactPosition, m_areaRadius, 0.9f);
        SpawnFragments(m_pendingImpactPosition, m_activationHitEnemies);
    }

    private void ApplyExplosionImpact(Collider2D impactCollider, BaseEnemy directTarget)
    {
        if (m_specialImpactResolved || directTarget == null || !directTarget.IsAlive) return;
        m_specialImpactResolved = true;
        m_activationHitEnemies.Add(directTarget);

        Vector2 impactPoint = impactCollider.ClosestPoint(transform.position);
        directTarget.TakeDamage(m_damage);
        int explosionDamage = Mathf.Max(1,
            Mathf.RoundToInt(m_damage * m_behavior.ImpactDamageMultiplier));
        DamageEnemiesInCircle(impactPoint, m_areaRadius, explosionDamage,
            directTarget, m_activationHitEnemies);
        ShowBurst(impactPoint, m_areaRadius, m_behavior.ConeVisualDuration);
        Destroy(gameObject);
    }

    private void ApplyFragmentImpact(Collider2D impactCollider, BaseEnemy directTarget)
    {
        if (m_specialImpactResolved || directTarget == null || !directTarget.IsAlive) return;
        m_specialImpactResolved = true;
        m_activationHitEnemies.Add(directTarget);

        Vector2 impactPoint = impactCollider.ClosestPoint(transform.position);
        directTarget.TakeDamage(m_damage);
        SpawnFragments(impactPoint, m_activationHitEnemies);
        ShowBurst(impactPoint, 0.6f, 0.25f);
        Destroy(gameObject);
    }

    private void ActivateForwardCone()
    {
        ApplyDamageInCone(transform.position, m_direction, m_behavior.ForwardConeLength,
            m_behavior.ForwardConeAngle, m_damage);
        ElementalConeVisual.Show(transform.position, m_direction,
            m_behavior.ForwardConeLength, m_behavior.ForwardConeAngle,
            m_behavior.ConeVisualDuration, m_spriteRenderer.sharedMaterial,
            m_spriteRenderer.sortingLayerID, m_spriteRenderer.sortingOrder + 1);
        Destroy(gameObject);
    }

    private void ActivateFragmentBurst()
    {
        int blastDamage = Mathf.Max(1,
            Mathf.RoundToInt(m_damage * m_behavior.ImpactDamageMultiplier));
        DamageEnemiesInCircle(transform.position, m_areaRadius, blastDamage,
            null, m_activationHitEnemies);
        SpawnFragments(transform.position, m_activationHitEnemies);
        ShowBurst(transform.position, m_areaRadius, 0.45f);
        Destroy(gameObject);
    }

    private IEnumerator RunColossusAttack()
    {
        float windup = m_behavior.ColossusWindupDuration;
        float elapsed = 0f;
        Vector2 impactCenter = transform.position;
        Vector2 raisedPosition = impactCenter + Vector2.up * m_behavior.AreaRadius;
        Vector3 finalScale = new Vector3(m_behavior.AreaRadius * 0.55f,
            m_behavior.AreaRadius * 1.5f, 1f);

        while (elapsed < windup)
        {
            elapsed += Time.deltaTime;
            float progress = windup <= 0f ? 1f : Mathf.Clamp01(elapsed / windup);
            transform.localScale = Vector3.Lerp(new Vector3(0.2f, 0.3f, 1f),
                finalScale, progress);
            transform.position = Vector2.Lerp(raisedPosition, impactCenter, progress);
            yield return null;
        }

        transform.position = impactCenter;

        int impactDamage = Mathf.Max(1,
            Mathf.RoundToInt(m_damage * m_behavior.ImpactDamageMultiplier));
        DamageEnemiesInCircle(transform.position, m_areaRadius, impactDamage,
            null, m_activationHitEnemies, knockbackSpeed: m_behavior.ColossusKnockbackSpeed,
            knockbackDuration: 0.3f, radialKnockback: true);
        ShowBurst(transform.position, m_behavior.AreaRadius, 0.35f);
        ShowBurst(transform.position, m_behavior.AreaRadius * 0.55f, 0.2f);
        Destroy(gameObject);
    }

    private IEnumerator RunFissureSequence(Vector2 endPosition)
    {
        Vector2 startPosition = transform.position;
        m_capsuleCollider.enabled = false;
        m_circleCollider.enabled = false;
        m_spriteRenderer.enabled = false;
        m_rigidbody.simulated = false;

        float visualDuration = m_behavior.SequenceCount * m_behavior.SequenceInterval;
        ElementalBurstVisual.ShowLine(m_originalSprite, m_spriteRenderer.sharedMaterial,
            startPosition, endPosition, 0.12f, visualDuration,
            new Color(1f, 0.35f, 0.08f, 0.75f),
            m_spriteRenderer.sortingLayerID, m_spriteRenderer.sortingOrder + 1);

        for (int i = 0; i < m_behavior.SequenceCount; i++)
        {
            yield return new WaitForSeconds(m_behavior.SequenceInterval);
            float progress = (i + 1f) / m_behavior.SequenceCount;
            Vector2 explosionPosition = Vector2.Lerp(startPosition, endPosition, progress);
            int damage = Mathf.Max(1,
                Mathf.RoundToInt(m_damage * m_behavior.AreaDamageMultiplier));
            DamageEnemiesInCircle(explosionPosition,
                m_behavior.SequenceBlastRadius, damage, null, m_activationHitEnemies);
            ShowBurst(explosionPosition, m_behavior.SequenceBlastRadius, 0.2f);
        }

        Destroy(gameObject);
    }

    private void DamageEnemiesInCircle(Vector2 center, float radius, int damage,
        BaseEnemy excluded, HashSet<BaseEnemy> alreadyHit, float freezeChance = 0f,
        float freezeDuration = 0f, float knockbackSpeed = 0f,
        float knockbackDuration = 0f, bool radialKnockback = false)
    {
        if (radius <= 0f || damage <= 0) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
        HashSet<BaseEnemy> damagedEnemies = alreadyHit;
        if (excluded != null)
            damagedEnemies.Add(excluded);

        foreach (Collider2D collider in colliders)
        {
            BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
            if (enemy == null || !enemy.IsAlive ||
                ((Vector2)enemy.transform.position - center).magnitude > radius ||
                !damagedEnemies.Add(enemy))
                continue;

            enemy.TakeDamage(damage);
            if (!enemy.IsAlive) continue;

            if (freezeChance > 0f && Random.value < freezeChance)
                enemy.ApplyFreeze(freezeDuration);

            if (knockbackSpeed > 0f)
            {
                Vector2 direction = radialKnockback
                    ? (Vector2)enemy.transform.position - center
                    : m_direction;
                if (direction == Vector2.zero) direction = Vector2.up;
                enemy.ApplyKnockback(direction, knockbackSpeed, knockbackDuration);
            }
        }
    }

    private void ApplyDamageInCone(Vector2 origin, Vector2 direction, float length,
        float angle, int damage, BaseEnemy excluded = null)
    {
        if (length <= 0f || angle <= 0f || damage <= 0) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, length);
        HashSet<BaseEnemy> damagedEnemies = new();
        if (excluded != null)
            damagedEnemies.Add(excluded);
        foreach (Collider2D collider in colliders)
        {
            BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
            if (enemy == null || !enemy.IsAlive || !damagedEnemies.Add(enemy)) continue;

            Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
            if (toEnemy.sqrMagnitude <= 0f || toEnemy.magnitude > length ||
                Vector2.Angle(direction, toEnemy) > angle * 0.5f)
                continue;

            enemy.TakeDamage(damage);
        }
    }

    private void SpawnFragments(Vector2 origin, HashSet<BaseEnemy> alreadyHit)
    {
        int count = m_behavior.FragmentCount;
        if (count <= 0) return;

        int damage = Mathf.RoundToInt(m_damage * m_behavior.FragmentDamageMultiplier);
        if (damage <= 0) return;

        float spread = m_behavior.FragmentSpreadAngle;
        float startAngle = count == 1 ? 0f : -spread * 0.5f;
        float angleStep = count == 1 ? 0f : spread / (count - 1);
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.right;
            ElementalFragment.Spawn(origin, direction, damage,
                m_behavior.FragmentSpeed, m_behavior.FragmentLifetime,
                m_behavior.FragmentSize, m_spriteRenderer.color,
                m_originalSprite, m_spriteRenderer.sharedMaterial,
                m_spriteRenderer.sortingLayerID, m_spriteRenderer.sortingOrder + 1,
                alreadyHit);
        }
    }

    private void ShowBurst(Vector2 position, float radius, float duration)
    {
        ElementalBurstVisual.Show(m_originalSprite, m_spriteRenderer.sharedMaterial,
            position, radius, duration, m_spriteRenderer.color,
            m_spriteRenderer.sortingLayerID, m_spriteRenderer.sortingOrder + 1);
    }

    private void TryDamageAreaTarget(BaseEnemy enemy)
    {
        if (m_nextDamageTime.TryGetValue(enemy, out float nextDamageTime) && Time.time < nextDamageTime)
            return;

        m_nextDamageTime[enemy] = Time.time + m_areaTickInterval;
        int areaDamage = Mathf.Max(1,
            Mathf.RoundToInt(m_damage * m_behavior.AreaDamageMultiplier));
        enemy.TakeDamage(areaDamage);
        if (m_isGlacialStorm && enemy.IsAlive)
            ShowBurst(enemy.transform.position, 0.22f, 0.12f);
        if (enemy.IsAlive)
            m_behavior?.ApplyOnHit(enemy);
    }

    private void TryApplyGlacialFreeze(BaseEnemy enemy)
    {
        if (enemy == null || !enemy.IsAlive || m_behavior.StormFreezeChance <= 0f)
            return;

        if (m_nextFreezeAttemptTime.TryGetValue(enemy, out float nextAttemptTime) &&
            Time.time < nextAttemptTime)
            return;

        m_nextFreezeAttemptTime[enemy] = Time.time + m_behavior.GlacialFreezeAttemptInterval;
        if (Random.value < m_behavior.StormFreezeChance)
            enemy.ApplyFreeze(m_behavior.StormFreezeDuration);
    }

    private void RegisterAreaContact(BaseEnemy enemy, Collider2D enemyCollider)
    {
        if (!m_areaContacts.TryGetValue(enemy, out HashSet<Collider2D> colliders))
        {
            colliders = new HashSet<Collider2D>();
            m_areaContacts.Add(enemy, colliders);
        }

        if (!colliders.Add(enemyCollider) || colliders.Count > 1) return;

        if (m_behavior.ProgressiveSlowMaximumPercent > 0f)
        {
            enemy.EnterProgressiveSlowArea(
                this,
                m_behavior.ProgressiveSlowDuration,
                m_behavior.ProgressiveSlowMaximumPercent);
        }
        else if (m_isMudArea && m_behavior.MudSlowPercent > 0f)
        {
            enemy.EnterSlowArea(this, m_behavior.MudSlowPercent);
        }
        else if (m_behavior.AreaSlowPercent > 0f)
        {
            enemy.EnterSlowArea(this, m_behavior.AreaSlowPercent);
        }

    }

    private void UnregisterAreaContact(BaseEnemy enemy, Collider2D enemyCollider)
    {
        if (!m_areaContacts.TryGetValue(enemy, out HashSet<Collider2D> colliders)) return;
        colliders.Remove(enemyCollider);
        if (colliders.Count > 0) return;

        m_areaContacts.Remove(enemy);
        if (m_isGlacialStorm)
            m_nextFreezeAttemptTime.Remove(enemy);
        if (m_behavior.ProgressiveSlowMaximumPercent > 0f)
            enemy.ExitProgressiveSlowArea(this);
        if (m_isMudArea && m_behavior.MudSlowPercent > 0f)
            enemy.ExitSlowArea(this);
        if (m_behavior.AreaSlowPercent > 0f)
            enemy.ExitSlowArea(this);
    }

    private void RemoveDestroyedEnemyReferences()
    {
        m_staleEnemies.Clear();

        foreach (BaseEnemy enemy in m_hitEnemies)
        {
            if (enemy == null || !enemy.IsAlive)
                m_staleEnemies.Add(enemy);
        }

        foreach (BaseEnemy enemy in m_staleEnemies)
            m_hitEnemies.Remove(enemy);

        m_staleEnemies.Clear();
        foreach (KeyValuePair<BaseEnemy, float> entry in m_nextDamageTime)
        {
            if (entry.Key == null || !entry.Key.IsAlive)
                m_staleEnemies.Add(entry.Key);
        }

        foreach (BaseEnemy enemy in m_staleEnemies)
            m_nextDamageTime.Remove(enemy);

        m_staleEnemies.Clear();
        foreach (KeyValuePair<BaseEnemy, float> entry in m_nextFreezeAttemptTime)
        {
            if (entry.Key == null || !entry.Key.IsAlive)
                m_staleEnemies.Add(entry.Key);
        }

        foreach (BaseEnemy enemy in m_staleEnemies)
            m_nextFreezeAttemptTime.Remove(enemy);

        m_staleEnemies.Clear();
        foreach (BaseEnemy enemy in m_areaContacts.Keys)
        {
            if (enemy == null || !enemy.IsAlive)
                m_staleEnemies.Add(enemy);
        }

        foreach (BaseEnemy enemy in m_staleEnemies)
        {
            if (enemy != null)
            {
                enemy.ExitProgressiveSlowArea(this);
                enemy.ExitSlowArea(this);
                enemy.ExitFreezeArea(this);
            }
            m_areaContacts.Remove(enemy);
            m_nextDamageTime.Remove(enemy);
        }
    }

    private void OnDestroy()
    {
        foreach (BaseEnemy enemy in m_areaContacts.Keys)
        {
            if (enemy != null)
            {
                enemy.ExitProgressiveSlowArea(this);
                enemy.ExitSlowArea(this);
                enemy.ExitFreezeArea(this);
            }
        }

        m_areaContacts.Clear();
        if (m_mudProjectileSprite != null)
            Destroy(m_mudProjectileSprite);
    }
}
