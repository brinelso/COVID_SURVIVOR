using System;
using UnityEngine;

public abstract class BaseCharacter : MonoBehaviour
{
    // A vida máxima e a vida inicial podem ser configuradas no Inspector.
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    [field: SerializeField] public int Health { get; private set; } = 100;
    public bool IsAlive { get; private set; }

    // O bônus fica separado da velocidade-base para não perder o valor original.
    private float m_speedBonusPercent;
    public float MovementSpeed => m_speed * (1f + m_speedBonusPercent / 100f);

    // A interface pode reagir à vida sem consultar o personagem a cada quadro.
    public event Action<int, int> HealthChanged;

    // Sistemas futuros de recompensa podem reagir à morte antes da limpeza do objeto.
    public event Action<BaseCharacter> Died;

    [SerializeField] protected float m_speed = 5f;

    private bool m_isInitialized;
    private bool m_deathHandled;

    public void Initialize()
    {
        if (m_isInitialized) return;

        if (MaxHealth <= 0)
        {
            Debug.LogError("MaxHealth precisa ser maior que zero.", this);
            return;
        }

        // Mantém a vida inicial dentro dos limites configurados.
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        IsAlive = Health > 0;

        Setup();
        m_isInitialized = true;

        HealthChanged?.Invoke(Health, MaxHealth);

        if (Health == 0)
        {
            Die();
        }
    }

    // Permite que sistemas externos definam a vida máxima antes da inicialização.
    public void Initialize(int maximumHealth)
    {
        if (m_isInitialized) return;

        if (maximumHealth <= 0)
        {
            Debug.LogError("A vida máxima de inicialização precisa ser maior que zero.", this);
            return;
        }

        MaxHealth = maximumHealth;
        Health = maximumHealth;
        Initialize();
    }

    protected abstract void Setup();

    public void AddMovementSpeedBonus(float percent)
    {
        if (percent <= 0f) return;

        m_speedBonusPercent += percent;
    }

    protected void Update()
    {
        if (!m_isInitialized || !IsAlive) return;

        UpdateLogic();
    }

    protected abstract void UpdateLogic();

    public void TakeDamage(int damage)
    {
        if (!m_isInitialized || !IsAlive || damage <= 0) return;

        Health = Mathf.Clamp(Health - damage, 0, MaxHealth);
        HealthChanged?.Invoke(Health, MaxHealth);

        if (Health == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (m_deathHandled) return;

        m_deathHandled = true;
        IsAlive = false;

        // Os ouvintes recebem o evento antes de o inimigo iniciar sua limpeza.
        Died?.Invoke(this);
        OnDeath();
    }

    protected virtual void OnDeath() { }
}
