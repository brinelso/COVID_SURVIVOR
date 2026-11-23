using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : BaseCharacter
{
    [SerializeField] private PlayerInput m_playerInput;

    [Header("Coleta de experiência")]
    [field: SerializeField] public float ExperiencePickupRange { get; private set; } = 0f;

    // Sistemas futuros podem receber o XP sem que o jogador precise armazená-lo agora.
    public event Action<int> ExperienceGained;

    private InputAction m_moveAction;
    private Vector2 m_movementDirection;
    private Vector2 m_facingDirection = Vector2.down;

    // Controla os sprites do médico (parado, andando e morte).
    private SpriteAnimator m_animator;
    private SpriteRenderer m_spriteRenderer;

    [Header("Feedback visual do ataque")]
    [SerializeField, Min(0.1f)] private float m_attackAnimationCooldown = 0.68f;
    [SerializeField, Min(0.1f)] private float m_elementalAttackAnimationCooldown = 1.05f;
    private float m_nextAttackAnimationTime;

    public Vector2 FacingDirection => m_facingDirection;

    private void Start()
    {
        Initialize();
    }

    protected override void Setup()
    {
        // Alcances negativos não fazem sentido: tratamos como zero.
        ExperiencePickupRange = Mathf.Max(0f, ExperiencePickupRange);

        // Adiciona o animador em código: a pasta de frames do médico é "Characters/Doctor".
        m_animator = gameObject.AddComponent<SpriteAnimator>();
        m_animator.Configure("Characters/Doctor");
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        // O quadro inicial é aplicado antes do primeiro Update (sem placeholder).
        m_animator.Play("Idle", 6f, true);

        if (m_playerInput == null)
        {
            Debug.LogError("PlayerInput precisa ser associado no Inspector.", this);
            return;
        }

        m_moveAction = m_playerInput.actions.FindAction("Player/Move", false);
        if (m_moveAction == null)
        {
            Debug.LogError("A ação Player/Move não foi encontrada no asset de entrada.", this);
            return;
        }

        m_moveAction.performed += HandleMovement;
        m_moveAction.canceled += HandleMovementCanceled;
    }

    public void GainExperience(int amount)
    {
        if (!IsAlive || amount <= 0) return;

        // Por enquanto, apenas comunicamos o ganho; não há XP acumulado nesta etapa.
        Debug.Log($"Player recebeu {amount} XP.", this);
        ExperienceGained?.Invoke(amount);
    }

    public void AddExperiencePickupRange(float amount)
    {
        if (amount <= 0f) return;

        ExperiencePickupRange += amount;
    }

    private void HandleMovement(InputAction.CallbackContext context)
    {
        m_movementDirection = context.ReadValue<Vector2>();
        if (m_movementDirection != Vector2.zero)
            m_facingDirection = m_movementDirection.normalized;
    }

    private void HandleMovementCanceled(InputAction.CallbackContext context)
    {
        m_movementDirection = Vector2.zero;
    }

    // O ataque visual nao precisa acompanhar cada projétil: sua frequencia tem limite proprio.
    // Antes da primeira magia, a vacina chama este metodo; depois, so o elemental chama.
    public void PlayAttackAnimation(Vector2 attackDirection, bool elemental = false)
    {
        if (!IsAlive || m_animator == null || Time.time < m_nextAttackAnimationTime) return;
        if (m_animator.IsPlayingClip("Attack")) return;

        // Magias rapidas deixam mais espaco para caminhar entre as animacoes.
        float cooldown = elemental ? m_elementalAttackAnimationCooldown : m_attackAnimationCooldown;
        m_nextAttackAnimationTime = Time.time + Mathf.Max(0.1f, cooldown);

        // A nova arte mira para a direita; espelhar somente ao atacar para a esquerda.
        if (m_spriteRenderer != null && Mathf.Abs(attackDirection.x) > 0.05f)
            m_spriteRenderer.flipX = attackDirection.x < 0f;

        // Com alta cadência não reiniciamos o clipe a cada projétil: evita tremulação.
        m_animator.PlayOnce("Attack", 14f);
    }

    protected override void UpdateLogic()
    {
        bool isMoving = m_movementDirection != Vector2.zero;

        // Caminhar e lançar magia podem ocorrer ao mesmo tempo; somente o sprite é priorizado.
        if (!m_animator.IsPlayingClip("Attack"))
        {
            if (m_spriteRenderer != null && Mathf.Abs(m_facingDirection.x) > 0.05f)
                m_spriteRenderer.flipX = m_facingDirection.x < 0f;
            m_animator.Play(isMoving ? "Walk" : "Idle", isMoving ? 10f : 6f, true);
        }

        if (!isMoving) return;

        // A normalização mantém a mesma velocidade nas diagonais.
        Vector2 direction = m_movementDirection.normalized;
        Vector3 movement = new Vector3(direction.x, direction.y, 0f)
            * MovementSpeed
            * Time.deltaTime;

        transform.Translate(movement, Space.World);
    }

    protected override void OnDeath()
    {
        // O áudio pode tocar enquanto o GameRunManager aguarda a última imagem.
        // A morte interrompe o ataque e é exibida antes da cena de derrota.
        if (m_animator != null)
            m_animator.PlayOnce("Death", 8f);
    }

    private void OnDestroy()
    {
        if (m_moveAction == null) return;

        m_moveAction.performed -= HandleMovement;
        m_moveAction.canceled -= HandleMovementCanceled;
    }
}
