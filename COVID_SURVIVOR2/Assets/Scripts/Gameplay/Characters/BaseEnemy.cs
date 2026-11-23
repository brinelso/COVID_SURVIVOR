using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyStatusEffect
{
    Burning,
    Slowed,
    Frozen,
    Poisoned
}

public abstract class BaseEnemy : BaseCharacter
{
    // Referência ao médico que o inimigo deve perseguir.
    protected Player m_player;

    [Header("Dano por contato")]
    [SerializeField] private int m_contactDamage = 10;
    [SerializeField] private float m_contactDamageInterval = 1f;

    private float m_nextContactDamageTime;

    private bool m_isBurning;
    private int m_burnDamagePerTick;
    private float m_burnRemaining;
    private float m_burnTickInterval;
    private float m_burnTickTimer;

    private bool m_isPoisoned;
    private int m_poisonDamagePerTick;
    private float m_poisonRemaining;
    private float m_poisonTickInterval;
    private float m_poisonTickTimer;

    private bool m_isSlowed;
    private float m_slowRemaining;
    private float m_slowMovementMultiplier = 1f;

    private bool m_isFrozen;
    private float m_freezeRemaining;
    private readonly HashSet<ElementalProjectile> m_freezeAreaSources = new();
    private Vector2 m_knockbackDirection;
    private float m_knockbackSpeed;
    private float m_knockbackRemaining;

    private readonly HashSet<ElementalProjectile> m_progressiveSlowSources = new();
    private readonly Dictionary<Component, float> m_slowAreaSources = new();
    private readonly List<Component> m_destroyedSlowSources = new();
    private readonly Dictionary<Component, float> m_attractionSources = new();
    private readonly List<Component> m_destroyedAttractionSources = new();
    private float m_progressiveSlowElapsed;
    private float m_progressiveSlowDuration;
    private float m_progressiveSlowMaximumPercent;

    // A apresentação futura pode reagir a aplicação, renovação e término dos estados.
    public event Action<EnemyStatusEffect> StatusEffectAppliedOrRefreshed;
    public event Action<EnemyStatusEffect> StatusEffectEnded;

    public float CurrentMovementSpeed
    {
        get
        {
            if (m_isFrozen || m_freezeAreaSources.Count > 0 || m_knockbackRemaining > 0f)
                return 0f;

            float movementMultiplier = m_isSlowed ? m_slowMovementMultiplier : 1f;
            if (m_progressiveSlowSources.Count > 0 && m_progressiveSlowDuration > 0f)
            {
                float progress = Mathf.Clamp01(m_progressiveSlowElapsed / m_progressiveSlowDuration);
                float progressiveMultiplier = 1f - m_progressiveSlowMaximumPercent * progress;
                movementMultiplier = Mathf.Min(movementMultiplier, progressiveMultiplier);
            }

            foreach (float areaMultiplier in m_slowAreaSources.Values)
                movementMultiplier = Mathf.Min(movementMultiplier, areaMultiplier);

            return m_speed * movementMultiplier;
        }
    }

    protected override void Setup()
    {
        m_player = FindAnyObjectByType<Player>();

        if (m_contactDamageInterval <= 0f)
        {
            Debug.LogError("O intervalo de dano por contato deve ser maior que zero.", this);
            m_contactDamageInterval = 1f;
        }
    }

    protected sealed override void UpdateLogic()
    {
        UpdateStatusEffects();
        if (IsAlive)
        {
            UpdateEnemyLogic();
            UpdateKnockback();
            UpdateAttraction();
        }
    }

    protected abstract void UpdateEnemyLogic();

    public void ApplyKnockback(Vector2 direction, float speed, float duration)
    {
        if (!IsAlive || direction == Vector2.zero || speed <= 0f || duration <= 0f) return;
        m_knockbackDirection = direction.normalized;
        m_knockbackSpeed = speed;
        m_knockbackRemaining = Mathf.Max(m_knockbackRemaining, duration);
    }

    private void UpdateKnockback()
    {
        if (m_knockbackRemaining <= 0f) return;
        float activeTime = Mathf.Min(Time.deltaTime, m_knockbackRemaining);
        transform.Translate(m_knockbackDirection * m_knockbackSpeed * activeTime, Space.World);
        m_knockbackRemaining -= activeTime;
    }

    public void SetAttraction(Component source, float movementSpeed)
    {
        if (!IsAlive || source == null || movementSpeed <= 0f) return;
        m_attractionSources[source] = movementSpeed;
    }

    public void RemoveAttraction(Component source)
    {
        if (source != null)
            m_attractionSources.Remove(source);
    }

    private void UpdateAttraction()
    {
        if (m_attractionSources.Count == 0 || m_isFrozen ||
            m_freezeAreaSources.Count > 0 || m_knockbackRemaining > 0f)
            return;

        Component strongestSource = null;
        float strongestSpeed = 0f;
        m_destroyedAttractionSources.Clear();

        foreach (KeyValuePair<Component, float> entry in m_attractionSources)
        {
            if (entry.Key == null)
            {
                m_destroyedAttractionSources.Add(entry.Key);
                continue;
            }

            if (entry.Value > strongestSpeed)
            {
                strongestSpeed = entry.Value;
                strongestSource = entry.Key;
            }
        }

        foreach (Component source in m_destroyedAttractionSources)
            m_attractionSources.Remove(source);

        if (strongestSource == null) return;
        Vector2 direction = (Vector2)strongestSource.transform.position - (Vector2)transform.position;
        if (direction.sqrMagnitude > 0.0001f)
            transform.Translate(direction.normalized * strongestSpeed * Time.deltaTime, Space.World);
    }

    public void EnterFreezeArea(ElementalProjectile source)
    {
        if (!IsAlive || source == null || !m_freezeAreaSources.Add(source)) return;
        if (!m_isFrozen && m_freezeAreaSources.Count == 1)
            StatusEffectAppliedOrRefreshed?.Invoke(EnemyStatusEffect.Frozen);
    }

    public void ExitFreezeArea(ElementalProjectile source)
    {
        if (!m_freezeAreaSources.Remove(source)) return;
        if (!m_isFrozen && m_freezeAreaSources.Count == 0)
            StatusEffectEnded?.Invoke(EnemyStatusEffect.Frozen);
    }

    public void ApplyBurn(int damagePerTick, float duration, float tickInterval)
    {
        if (!IsAlive || damagePerTick <= 0 || duration <= 0f || tickInterval <= 0f) return;

        if (!m_isBurning)
            m_burnTickTimer = 0f;

        m_isBurning = true;
        m_burnDamagePerTick = damagePerTick;
        m_burnRemaining = duration;
        m_burnTickInterval = tickInterval;
        StatusEffectAppliedOrRefreshed?.Invoke(EnemyStatusEffect.Burning);
    }

    public void ApplyPoison(int damagePerTick, float duration, float tickInterval)
    {
        if (!IsAlive || damagePerTick <= 0 || duration <= 0f || tickInterval <= 0f) return;

        bool isNewPoison = !m_isPoisoned;
        if (isNewPoison)
            m_poisonTickTimer = 0f;

        m_isPoisoned = true;
        m_poisonDamagePerTick = isNewPoison
            ? damagePerTick
            : Mathf.Max(m_poisonDamagePerTick, damagePerTick);
        m_poisonRemaining = Mathf.Max(m_poisonRemaining, duration);
        m_poisonTickInterval = isNewPoison
            ? tickInterval
            : Mathf.Min(m_poisonTickInterval, tickInterval);
        StatusEffectAppliedOrRefreshed?.Invoke(EnemyStatusEffect.Poisoned);
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        if (!IsAlive || slowPercent <= 0f || duration <= 0f) return;

        float newMovementMultiplier = 1f - Mathf.Clamp(slowPercent, 0f, 0.95f);
        if (m_isSlowed)
        {
            // Mantém só a redução mais forte, sem multiplicar lentidões entre si.
            m_slowMovementMultiplier = Mathf.Min(m_slowMovementMultiplier, newMovementMultiplier);
        }
        else
        {
            m_slowMovementMultiplier = newMovementMultiplier;
        }

        m_isSlowed = true;
        m_slowRemaining = duration;
        StatusEffectAppliedOrRefreshed?.Invoke(EnemyStatusEffect.Slowed);
    }

    public void ApplyFreeze(float duration)
    {
        if (!IsAlive || duration <= 0f) return;

        m_isFrozen = true;
        m_freezeRemaining = Mathf.Max(m_freezeRemaining, duration);
        StatusEffectAppliedOrRefreshed?.Invoke(EnemyStatusEffect.Frozen);
    }

    public void EnterProgressiveSlowArea(ElementalProjectile source, float duration, float maximumSlowPercent)
    {
        if (!IsAlive || source == null || duration <= 0f || maximumSlowPercent <= 0f) return;
        if (!m_progressiveSlowSources.Add(source)) return;

        if (m_progressiveSlowSources.Count == 1)
        {
            m_progressiveSlowElapsed = 0f;
            m_progressiveSlowDuration = duration;
            m_progressiveSlowMaximumPercent = Mathf.Clamp01(maximumSlowPercent);
            StatusEffectAppliedOrRefreshed?.Invoke(EnemyStatusEffect.Slowed);
        }
        else
        {
            // Áreas sobrepostas compartilham a mesma progressão em vez de somar reduções.
            m_progressiveSlowDuration = Mathf.Max(m_progressiveSlowDuration, duration);
            m_progressiveSlowMaximumPercent = Mathf.Max(
                m_progressiveSlowMaximumPercent, Mathf.Clamp01(maximumSlowPercent));
        }
    }

    public void ExitProgressiveSlowArea(ElementalProjectile source)
    {
        if (!m_progressiveSlowSources.Remove(source) || m_progressiveSlowSources.Count > 0) return;

        EndProgressiveSlow();
    }

    public void EnterSlowArea(Component source, float slowPercent)
    {
        if (!IsAlive || source == null || slowPercent <= 0f) return;

        float movementMultiplier = 1f - Mathf.Clamp(slowPercent, 0f, 0.95f);
        bool isNewSource = !m_slowAreaSources.ContainsKey(source);
        if (isNewSource)
            StatusEffectAppliedOrRefreshed?.Invoke(EnemyStatusEffect.Slowed);

        // Mantém a maior redução dessa fonte sem multiplicar efeitos de áreas sobrepostas.
        if (m_slowAreaSources.TryGetValue(source, out float currentMultiplier))
            movementMultiplier = Mathf.Min(currentMultiplier, movementMultiplier);

        m_slowAreaSources[source] = movementMultiplier;
    }

    public void ExitSlowArea(Component source)
    {
        if (!m_slowAreaSources.Remove(source)) return;

        if (m_slowAreaSources.Count == 0 && !m_isSlowed && m_progressiveSlowSources.Count == 0)
            StatusEffectEnded?.Invoke(EnemyStatusEffect.Slowed);
    }

    private void UpdateStatusEffects()
    {
        UpdateBurn();
        if (!IsAlive) return;

        UpdatePoison();
        if (!IsAlive) return;

        RemoveDestroyedFreezeSources();

        if (m_isSlowed)
        {
            m_slowRemaining -= Time.deltaTime;
            if (m_slowRemaining <= 0f)
                EndSlow();
        }

        if (m_isFrozen)
        {
            m_freezeRemaining -= Time.deltaTime;
            if (m_freezeRemaining <= 0f)
                EndFreeze();
        }

        UpdateProgressiveSlow();
        UpdateSlowAreaSources();
    }

    private void RemoveDestroyedFreezeSources()
    {
        bool hadAreaFreeze = m_freezeAreaSources.Count > 0;
        m_freezeAreaSources.RemoveWhere(source => source == null);
        if (hadAreaFreeze && m_freezeAreaSources.Count == 0 && !m_isFrozen)
            StatusEffectEnded?.Invoke(EnemyStatusEffect.Frozen);
    }

    private void UpdateProgressiveSlow()
    {
        m_destroyedSlowSources.Clear();
        foreach (ElementalProjectile source in m_progressiveSlowSources)
        {
            if (source == null)
                m_destroyedSlowSources.Add(source);
        }

        foreach (ElementalProjectile source in m_destroyedSlowSources)
            m_progressiveSlowSources.Remove(source);

        if (m_progressiveSlowSources.Count == 0)
        {
            EndProgressiveSlow();
            return;
        }

        m_progressiveSlowElapsed = Mathf.Min(
            m_progressiveSlowDuration, m_progressiveSlowElapsed + Time.deltaTime);
    }

    private void UpdateSlowAreaSources()
    {
        m_destroyedSlowSources.Clear();
        foreach (Component source in m_slowAreaSources.Keys)
        {
            if (source == null)
                m_destroyedSlowSources.Add(source);
        }

        foreach (ElementalProjectile source in m_destroyedSlowSources)
            m_slowAreaSources.Remove(source);

        if (m_destroyedSlowSources.Count > 0 && m_slowAreaSources.Count == 0 &&
            !m_isSlowed && m_progressiveSlowSources.Count == 0)
            StatusEffectEnded?.Invoke(EnemyStatusEffect.Slowed);
    }

    private void UpdateBurn()
    {
        if (!m_isBurning) return;

        float activeTimeThisFrame = Mathf.Min(Time.deltaTime, m_burnRemaining);
        m_burnRemaining -= Time.deltaTime;
        m_burnTickTimer += activeTimeThisFrame;

        while (m_burnTickTimer >= m_burnTickInterval)
        {
            m_burnTickTimer -= m_burnTickInterval;
            TakeDamage(m_burnDamagePerTick);
            if (!IsAlive) return;
        }

        if (m_burnRemaining <= 0f)
            EndBurn();
    }

    private void UpdatePoison()
    {
        if (!m_isPoisoned) return;

        float activeTimeThisFrame = Mathf.Min(Time.deltaTime, m_poisonRemaining);
        m_poisonRemaining -= Time.deltaTime;
        m_poisonTickTimer += activeTimeThisFrame;

        while (m_poisonTickTimer >= m_poisonTickInterval)
        {
            m_poisonTickTimer -= m_poisonTickInterval;
            TakeDamage(m_poisonDamagePerTick);
            if (!IsAlive) return;
        }

        if (m_poisonRemaining <= 0f)
            EndPoison();
    }

    private void EndPoison()
    {
        if (!m_isPoisoned) return;

        m_isPoisoned = false;
        m_poisonRemaining = 0f;
        m_poisonTickTimer = 0f;
        StatusEffectEnded?.Invoke(EnemyStatusEffect.Poisoned);
    }

    private void EndBurn()
    {
        if (!m_isBurning) return;

        m_isBurning = false;
        m_burnRemaining = 0f;
        m_burnTickTimer = 0f;
        StatusEffectEnded?.Invoke(EnemyStatusEffect.Burning);
    }

    private void EndSlow()
    {
        if (!m_isSlowed) return;

        m_isSlowed = false;
        m_slowRemaining = 0f;
        m_slowMovementMultiplier = 1f;
        StatusEffectEnded?.Invoke(EnemyStatusEffect.Slowed);
    }

    private void ClearStatusEffects()
    {
        bool hadSlowAreaSources = m_slowAreaSources.Count > 0;
        bool hadFreezeAreaSources = m_freezeAreaSources.Count > 0;
        EndBurn();
        EndPoison();
        EndSlow();
        EndFreeze();
        m_progressiveSlowSources.Clear();
        EndProgressiveSlow();
        m_slowAreaSources.Clear();
        m_attractionSources.Clear();
        m_freezeAreaSources.Clear();
        m_knockbackRemaining = 0f;
        if (hadFreezeAreaSources)
            StatusEffectEnded?.Invoke(EnemyStatusEffect.Frozen);
        if (hadSlowAreaSources)
            StatusEffectEnded?.Invoke(EnemyStatusEffect.Slowed);
    }

    private void EndProgressiveSlow()
    {
        if (m_progressiveSlowSources.Count > 0) return;

        bool wasSlowed = m_progressiveSlowDuration > 0f;
        m_progressiveSlowElapsed = 0f;
        m_progressiveSlowDuration = 0f;
        m_progressiveSlowMaximumPercent = 0f;
        if (wasSlowed)
            StatusEffectEnded?.Invoke(EnemyStatusEffect.Slowed);
    }

    private void EndFreeze()
    {
        if (!m_isFrozen) return;

        m_isFrozen = false;
        m_freezeRemaining = 0f;
        if (m_freezeAreaSources.Count == 0)
            StatusEffectEnded?.Invoke(EnemyStatusEffect.Frozen);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void TryDamagePlayer(GameObject otherObject)
    {
        if (!IsAlive || m_contactDamage <= 0) return;
        if (Time.time < m_nextContactDamageTime) return;

        Player player = otherObject.GetComponentInParent<Player>();
        if (player == null || !player.IsAlive) return;

        player.TakeDamage(m_contactDamage);
        m_nextContactDamageTime = Time.time + m_contactDamageInterval;
    }

    protected override void OnDeath()
    {
        CovidAudioManager.Play(SoundCue.EnemyDeath);
        ClearStatusEffects();

        // Desativa contato imediatamente; a morte já foi anunciada para futuros ouvintes.
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D enemyCollider in colliders)
        {
            enemyCollider.enabled = false;
        }

        Rigidbody2D[] rigidbodies = GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D body in rigidbodies)
        {
            body.simulated = false;
        }

        // Se o inimigo tem animação de morte, ela toca antes da destruição.
        SpriteAnimator animator = GetComponent<SpriteAnimator>();
        if (animator != null && animator.HasClip("Death"))
        {
            animator.PlayOnce("Death", 10f, () => Destroy(gameObject));
            return;
        }

        // Sem animação: mantém o comportamento anterior (visual some e objeto é destruído).
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            spriteRenderer.enabled = false;
        }

        Destroy(gameObject);
    }
}
