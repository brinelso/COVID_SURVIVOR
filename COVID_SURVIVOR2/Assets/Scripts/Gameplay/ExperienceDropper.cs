using UnityEngine;

[RequireComponent(typeof(BaseEnemy))]
public class ExperienceDropper : MonoBehaviour
{
    [Header("Prefabs de cristal")]
    [SerializeField] private ExperienceOrb m_weakOrbPrefab;
    [SerializeField] private ExperienceOrb m_mediumOrbPrefab;
    [SerializeField] private ExperienceOrb m_strongOrbPrefab;

    [Header("Pesos do resultado (teste temporário)")]
    [SerializeField] private int m_noOrbWeight = 20;
    [SerializeField] private int m_weakOrbWeight = 45;
    [SerializeField] private int m_mediumOrbWeight = 25;
    [SerializeField] private int m_strongOrbWeight = 10;

    private BaseEnemy m_enemy;
    private bool m_hasAttemptedDrop;
    private bool m_isConfigurationValid;

    private void Awake()
    {
        m_enemy = GetComponent<BaseEnemy>();
        m_enemy.Died += HandleEnemyDied;

        bool prefabsAreAssigned = m_weakOrbPrefab != null &&
                                  m_mediumOrbPrefab != null &&
                                  m_strongOrbPrefab != null;
        if (!prefabsAreAssigned)
        {
            Debug.LogError("Associe os três prefabs de cristal no ExperienceDropper.", this);
        }

        bool weightsAreValid = m_noOrbWeight >= 0 && m_weakOrbWeight >= 0 &&
                               m_mediumOrbWeight >= 0 && m_strongOrbWeight >= 0 &&
                               GetTotalWeight() > 0;
        if (!weightsAreValid)
        {
            Debug.LogError("Os pesos de drop devem ser não negativos e somar mais que zero.", this);
        }

        m_isConfigurationValid = prefabsAreAssigned && weightsAreValid;
    }

    private void OnDestroy()
    {
        if (m_enemy != null)
        {
            m_enemy.Died -= HandleEnemyDied;
        }
    }

    private void HandleEnemyDied(BaseCharacter deadCharacter)
    {
        if (m_hasAttemptedDrop || !m_isConfigurationValid || deadCharacter != m_enemy) return;

        // Marcar antes do sorteio impede uma segunda tentativa na mesma morte.
        m_hasAttemptedDrop = true;
        TryDropOrb();
    }

    private void TryDropOrb()
    {
        int totalWeight = GetTotalWeight();
        if (totalWeight <= 0) return;

        int randomWeight = Random.Range(0, totalWeight);

        if (randomWeight < m_noOrbWeight) return;
        randomWeight -= m_noOrbWeight;

        ExperienceOrb selectedPrefab;
        if (randomWeight < m_weakOrbWeight)
        {
            selectedPrefab = m_weakOrbPrefab;
        }
        else
        {
            randomWeight -= m_weakOrbWeight;
            selectedPrefab = randomWeight < m_mediumOrbWeight
                ? m_mediumOrbPrefab
                : m_strongOrbPrefab;
        }

        ExperienceSpawner spawner = FindAnyObjectByType<ExperienceSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("ExperienceDropper não encontrou um ExperienceSpawner para respeitar o limite de cristais.", this);
            return;
        }

        spawner.TrySpawnOrb(selectedPrefab, transform.position);
    }

    private int GetTotalWeight()
    {
        return m_noOrbWeight + m_weakOrbWeight + m_mediumOrbWeight + m_strongOrbWeight;
    }
}
