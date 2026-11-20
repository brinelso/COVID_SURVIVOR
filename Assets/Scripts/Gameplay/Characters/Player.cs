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

    public Vector2 FacingDirection => m_facingDirection;

    private void Start()
    {
        Initialize();
    }

    protected override void Setup()
    {
        // Alcances negativos não fazem sentido: tratamos como zero.
        ExperiencePickupRange = Mathf.Max(0f, ExperiencePickupRange);

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

    protected override void UpdateLogic()
    {
        if (m_movementDirection == Vector2.zero) return;

        // A normalização mantém a mesma velocidade nas diagonais.
        Vector2 direction = m_movementDirection.normalized;
        Vector3 movement = new Vector3(direction.x, direction.y, 0f)
            * MovementSpeed
            * Time.deltaTime;

        transform.Translate(movement, Space.World);
    }

    private void OnDestroy()
    {
        if (m_moveAction == null) return;

        m_moveAction.performed -= HandleMovement;
        m_moveAction.canceled -= HandleMovementCanceled;
    }
}
