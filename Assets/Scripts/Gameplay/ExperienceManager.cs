using System;
using System.Numerics;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    private const int ExperiencePerLevel = 100;

    [SerializeField] private Player m_player;

    private BigInteger m_currentLevel = BigInteger.One;
    private BigInteger m_currentExperience;

    // A interface recebe os valores prontos sempre que o progresso muda.
    public event Action<BigInteger, BigInteger, BigInteger, float> ExperienceChanged;

    // Outros sistemas poderão reagir a cada nível alcançado.
    public event Action<BigInteger> LevelUp;

    public BigInteger CurrentLevel => m_currentLevel;
    public BigInteger CurrentExperience => m_currentExperience;
    public BigInteger ExperienceRequired => m_currentLevel * ExperiencePerLevel;
    public float CurrentProgress => CalculateProgress();

    // Ferramenta temporária para abrir o fluxo normal de level-up pela interface.
    public void DebugGrantExperienceForNextLevel()
    {
        if (m_player == null || !m_player.IsAlive) return;

        BigInteger missingExperience = ExperienceRequired - m_currentExperience;
        if (missingExperience <= 0 || missingExperience > int.MaxValue) return;
        m_player.GainExperience((int)missingExperience);
    }

    private void OnEnable()
    {
        if (m_player == null)
        {
            m_player = GetComponent<Player>();
        }

        if (m_player == null)
        {
            Debug.LogError("ExperienceManager precisa de uma referência ao Player.", this);
            return;
        }

        m_player.ExperienceGained += HandleExperienceGained;
    }

    private void OnDisable()
    {
        if (m_player != null)
        {
            m_player.ExperienceGained -= HandleExperienceGained;
        }
    }

    private void HandleExperienceGained(int amount)
    {
        if (amount <= 0) return;

        m_currentExperience += amount;

        // Subtrair cada requisito preserva o excedente e permite vários níveis de uma vez.
        while (m_currentExperience >= ExperienceRequired)
        {
            m_currentExperience -= ExperienceRequired;
            m_currentLevel += BigInteger.One;
            LevelUp?.Invoke(m_currentLevel);
        }

        ExperienceChanged?.Invoke(
            m_currentLevel,
            m_currentExperience,
            ExperienceRequired,
            CalculateProgress());
    }

    private float CalculateProgress()
    {
        // A escala inteira evita converter números enormes diretamente para float.
        const int precision = 10000;
        BigInteger scaledProgress = m_currentExperience * precision / ExperienceRequired;
        return (float)scaledProgress / precision;
    }
}
