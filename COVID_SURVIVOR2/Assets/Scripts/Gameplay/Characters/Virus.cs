using System.Collections;
using UnityEngine;

// Um único comportamento para as variedades: as diferenças ficam no prefab.
// Isso preserva os sistemas existentes de dano elemental, XP e contagem de ondas.
public class Virus : BaseEnemy
{
    [Header("Identidade e progressão")]
    [SerializeField] private string m_animationFolder = "Characters/Virus";
    [SerializeField, Min(1)] private int m_experienceReward = 5;
    [SerializeField, Min(0.2f)] private float m_healthMultiplier = 1f;
    [SerializeField, Min(1f)] private float m_walkFramesPerSecond = 10f;

    [Header("Pulso especial (mini-boss e boss)")]
    [SerializeField] private bool m_enablePulseAttack = false;
    [SerializeField, Min(0.5f)] private float m_pulseRange = 2.4f;
    [SerializeField, Min(1)] private int m_pulseDamage = 14;
    [SerializeField, Min(1f)] private float m_pulseInterval = 7f;
    [SerializeField, Min(1f)] private float m_attackFramesPerSecond = 8f;
    [SerializeField, Min(0.1f)] private float m_pulseWindup = 0.5f;

    private bool m_hasGrantedExperience;
    private bool m_isCasting;
    private float m_nextPulseTime;
    private SpriteAnimator m_animator;

    public float HealthMultiplier => Mathf.Max(0.2f, m_healthMultiplier);

    protected override void Setup()
    {
        base.Setup();
        m_animator = GetComponent<SpriteAnimator>();
        if (m_animator == null)
            m_animator = gameObject.AddComponent<SpriteAnimator>();

        m_animator.Configure(m_animationFolder);
        m_animator.Play("Walk", m_walkFramesPerSecond, true);
        // O chefe dá tempo para o jogador reagir antes do primeiro pulso.
        m_nextPulseTime = Time.time + 3f;
    }

    protected override void UpdateEnemyLogic()
    {
        if (m_player == null || !m_player.IsAlive || m_isCasting)
            return;

        Vector2 direction = (Vector2)m_player.transform.position - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        // Só chefes têm o ataque de área; nenhum projétil extra é criado.
        if (m_enablePulseAttack && CurrentMovementSpeed > 0f &&
            Time.time >= m_nextPulseTime &&
            direction.sqrMagnitude <= m_pulseRange * m_pulseRange &&
            m_animator != null && m_animator.HasClip("Attack"))
        {
            m_isCasting = true;
            m_nextPulseTime = Time.time + m_pulseInterval;
            StartCoroutine(CastPulse());
            return;
        }

        // Animação de movimento é mantida sem reiniciar a cada Update.
        if (m_animator != null)
            m_animator.Play("Walk", m_walkFramesPerSecond, true);

        // Os modificadores de velocidade (gelo, lentidão etc.) continuam ativos.
        Vector3 movement = (Vector3)(direction.normalized * CurrentMovementSpeed * Time.deltaTime);
        transform.Translate(movement, Space.World);
    }

    private IEnumerator CastPulse()
    {
        // A animação avisa do golpe antes de aplicar dano: há tempo para fugir.
        m_animator.PlayOnce("Attack", m_attackFramesPerSecond);
        yield return new WaitForSeconds(m_pulseWindup);

        if (IsAlive && m_player != null && m_player.IsAlive)
        {
            float distance = Vector2.Distance(m_player.transform.position, transform.position);
            if (distance <= m_pulseRange)
                m_player.TakeDamage(m_pulseDamage);
        }

        // Completa os quadros do ataque antes de voltar a caminhar.
        float fullAnimationTime = 0.8f;
        yield return new WaitForSeconds(Mathf.Max(0.1f, fullAnimationTime - m_pulseWindup));
        m_isCasting = false;
        if (IsAlive && m_animator != null)
            m_animator.Play("Walk", m_walkFramesPerSecond, true);
    }

    protected override void OnDeath()
    {
        StopAllCoroutines();
        m_isCasting = false;
        if (!m_hasGrantedExperience)
        {
            m_hasGrantedExperience = true;
            if (m_player != null)
                m_player.GainExperience(m_experienceReward);
        }

        // BaseEnemy reproduz o clipe Death, remove colisões e destrói o objeto.
        base.OnDeath();
    }
}
