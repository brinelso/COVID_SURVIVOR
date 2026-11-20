using System.Collections;
using UnityEngine;

public class ExperienceSpawner : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Player m_player;
    [SerializeField] private ExperienceOrb m_weakOrbPrefab;
    [SerializeField] private ExperienceOrb m_mediumOrbPrefab;
    [SerializeField] private ExperienceOrb m_strongOrbPrefab;

    [Header("Posição e frequência")]
    [SerializeField] private float m_minimumSpawnDistance = 3f;
    [SerializeField] private float m_maximumSpawnDistance = 8f;
    [SerializeField] private float m_minimumOrbSpacing = 1.5f;
    [SerializeField] private float m_spawnInterval = 1.5f;
    [SerializeField] private int m_maximumOrbs = 20;
    [SerializeField] private int m_positionAttempts = 20;

    [Header("Pesos dos tipos (teste temporário)")]
    [SerializeField] private int m_weakOrbWeight = 45;
    [SerializeField] private int m_mediumOrbWeight = 35;
    [SerializeField] private int m_strongOrbWeight = 20;

    private void Start()
    {
        if (m_player == null)
        {
            m_player = FindAnyObjectByType<Player>();
        }

        if (!HasValidConfiguration()) return;

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        // Espera um quadro para dar tempo de Player.Initialize() definir IsAlive.
        yield return null;

        while (m_player != null && m_player.IsAlive)
        {
            yield return new WaitForSeconds(m_spawnInterval);

            if (m_player == null || !m_player.IsAlive) yield break;
            if (CountActiveOrbs() >= m_maximumOrbs) continue;

            if (TryGetSpawnPosition(out Vector3 spawnPosition))
            {
                ExperienceOrb prefab = ChooseOrbPrefab();
                if (prefab != null)
                {
                    TrySpawnOrb(prefab, spawnPosition);
                }
            }
        }
    }

    public bool TrySpawnOrb(ExperienceOrb prefab, Vector3 spawnPosition)
    {
        if (prefab == null)
        {
            Debug.LogError("Não é possível gerar um cristal sem prefab associado.", this);
            return false;
        }

        // O mesmo limite protege tanto o spawn aleatório quanto os drops dos inimigos.
        if (m_maximumOrbs <= 0 || CountActiveOrbs() >= m_maximumOrbs)
        {
            return false;
        }

        Instantiate(prefab, spawnPosition, Quaternion.identity);
        return true;
    }

    private int CountActiveOrbs()
    {
        // Cristais colocados diretamente na cena também contam para o limite.
        return FindObjectsByType<ExperienceOrb>(FindObjectsInactive.Exclude).Length;
    }

    private ExperienceOrb ChooseOrbPrefab()
    {
        int totalWeight = m_weakOrbWeight + m_mediumOrbWeight + m_strongOrbWeight;
        if (totalWeight <= 0) return null;

        int randomWeight = Random.Range(0, totalWeight);

        if (randomWeight < m_weakOrbWeight) return m_weakOrbPrefab;
        randomWeight -= m_weakOrbWeight;

        if (randomWeight < m_mediumOrbWeight) return m_mediumOrbPrefab;
        return m_strongOrbPrefab;
    }

    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        for (int attempt = 0; attempt < m_positionAttempts; attempt++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(m_minimumSpawnDistance, m_maximumSpawnDistance);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 candidate = (Vector2)m_player.transform.position + direction * distance;

            if (IsTooCloseToAnotherOrb(candidate)) continue;

            spawnPosition = new Vector3(candidate.x, candidate.y, m_player.transform.position.z);
            return true;
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    private bool IsTooCloseToAnotherOrb(Vector2 candidatePosition)
    {
        ExperienceOrb[] activeOrbs = FindObjectsByType<ExperienceOrb>(FindObjectsInactive.Exclude);

        foreach (ExperienceOrb orb in activeOrbs)
        {
            if (Vector2.Distance(candidatePosition, orb.transform.position) < m_minimumOrbSpacing)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasValidConfiguration()
    {
        if (m_player == null)
        {
            Debug.LogError("ExperienceSpawner precisa de um Player na cena.", this);
            return false;
        }

        if (m_weakOrbPrefab == null || m_mediumOrbPrefab == null || m_strongOrbPrefab == null)
        {
            Debug.LogError("Associe os três prefabs de cristal no ExperienceSpawner.", this);
            return false;
        }

        if (m_spawnInterval <= 0f || m_maximumOrbs <= 0 || m_positionAttempts <= 0)
        {
            Debug.LogError("Intervalo, limite de cristais e tentativas devem ser maiores que zero.", this);
            return false;
        }

        if (m_minimumSpawnDistance < 0f || m_maximumSpawnDistance <= m_minimumSpawnDistance || m_minimumOrbSpacing < 0f)
        {
            Debug.LogError("As distâncias devem ser válidas: máxima maior que mínima, distâncias não negativas.", this);
            return false;
        }

        if (m_weakOrbWeight < 0 || m_mediumOrbWeight < 0 || m_strongOrbWeight < 0 ||
            m_weakOrbWeight + m_mediumOrbWeight + m_strongOrbWeight <= 0)
        {
            Debug.LogError("Os pesos devem ser zero ou positivos, e pelo menos um peso precisa ser maior que zero.", this);
            return false;
        }

        return true;
    }
}
