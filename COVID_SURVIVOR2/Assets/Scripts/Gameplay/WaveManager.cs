using System;
using System.Collections.Generic;
using UnityEngine;

// Conta mortes confirmadas e calcula a vida aplicada aos vírus de cada onda.
public class WaveManager : MonoBehaviour
{
    [Header("Configuração das ondas")]
    [SerializeField, Min(1), Tooltip("Eliminações necessárias por nível de onda (onda × base).")]
    private int m_killsRequiredPerWave = 10;

    [Header("Vida dos vírus por onda")]
    [SerializeField, Min(1)] private int m_baseVirusHealth = 30;
    [SerializeField, Min(0)] private int m_healthIncreasePerWave = 10;

    private readonly HashSet<BaseEnemy> m_registeredEnemies = new HashSet<BaseEnemy>();

    public int CurrentWave { get; private set; } = 1;
    public int DefeatedThisWave { get; private set; }
    public int TotalDefeated { get; private set; }
    // O requisito cresce linearmente: onda atual multiplicada pela base do Inspector.
    public int KillsRequiredPerWave => (int)Math.Min(int.MaxValue,
        (long)CurrentWave * m_killsRequiredPerWave);

    // A interface recebe a onda e o progresso sempre que algum valor muda.
    public event Action<int, int, int> WaveChanged;

    private void Awake()
    {
        if (m_killsRequiredPerWave <= 0)
        {
            Debug.LogError("A quantidade de eliminações por onda precisa ser maior que zero.", this);
            m_killsRequiredPerWave = 10;
        }

        if (m_baseVirusHealth <= 0)
        {
            Debug.LogError("A vida-base dos vírus precisa ser maior que zero.", this);
            m_baseVirusHealth = 30;
        }

        m_healthIncreasePerWave = Mathf.Max(0, m_healthIncreasePerWave);
    }

    public int GetVirusHealthForCurrentWave()
    {
        long health = m_baseVirusHealth + (long)(CurrentWave - 1) * m_healthIncreasePerWave;
        return (int)Math.Min(int.MaxValue, health);
    }

    public void RegisterEnemy(BaseEnemy enemy)
    {
        if (enemy == null || !m_registeredEnemies.Add(enemy)) return;

        enemy.Died += HandleEnemyDied;
    }

    private void HandleEnemyDied(BaseCharacter deadCharacter)
    {
        BaseEnemy enemy = deadCharacter as BaseEnemy;
        if (enemy == null || !m_registeredEnemies.Remove(enemy)) return;

        enemy.Died -= HandleEnemyDied;
        TotalDefeated++;
        DefeatedThisWave++;

        if (DefeatedThisWave >= KillsRequiredPerWave)
        {
            CurrentWave++;
            DefeatedThisWave = 0;
        }

        WaveChanged?.Invoke(CurrentWave, DefeatedThisWave, KillsRequiredPerWave);
    }

    private void OnDestroy()
    {
        foreach (BaseEnemy enemy in m_registeredEnemies)
        {
            if (enemy != null)
                enemy.Died -= HandleEnemyDied;
        }
    }
}
