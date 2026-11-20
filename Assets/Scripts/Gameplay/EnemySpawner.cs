using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Virus m_virusPrefab = null;
    [SerializeField] private Player m_player;
    [SerializeField] private WaveManager m_waveManager;
    [SerializeField] private float m_spawnInterval = 3f;
    [SerializeField] private float m_spawnDistance = 4f;

    [Header("Multiplicador de spawn por onda")]
    [SerializeField, Min(0.1f)] private float m_baseSpawnMultiplier = 1f;
    [SerializeField, Min(0f)] private float m_spawnMultiplierIncreasePerWave = 0.5f;
    [SerializeField, Min(0.1f)] private float m_maximumSpawnMultiplier = 2f;

    private void Start()
    {
        if (m_player == null)
        {
            m_player = FindAnyObjectByType<Player>();
        }

        if (m_waveManager == null)
        {
            m_waveManager = GetComponent<WaveManager>();
        }

        if (m_virusPrefab == null)
        {
            Debug.LogError("O prefab do vírus precisa ser associado no Inspector.", this);
            return;
        }

        if (m_player == null)
        {
            Debug.LogError("O Player não foi encontrado na cena.", this);
            return;
        }

        if (m_spawnInterval <= 0f)
        {
            Debug.LogError("O intervalo de spawn precisa ser maior que zero.", this);
            return;
        }

        if (m_spawnDistance <= 0f)
        {
            Debug.LogError("A distância de spawn precisa ser maior que zero.", this);
            return;
        }

        if (m_baseSpawnMultiplier <= 0f || m_maximumSpawnMultiplier <= 0f)
        {
            Debug.LogError("Os multiplicadores de spawn precisam ser maiores que zero.", this);
            return;
        }

        m_spawnMultiplierIncreasePerWave = Mathf.Max(0f, m_spawnMultiplierIncreasePerWave);

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        // Cria um vírus imediatamente e repete após cada intervalo.
        while (true)
        {
            SpawnVirus();
            // Recalcula enquanto espera para refletir imediatamente uma mudança de onda.
            float elapsedTime = 0f;
            int waveAtStartOfWait = m_waveManager != null ? m_waveManager.CurrentWave : 1;
            while (elapsedTime < GetCurrentSpawnInterval())
            {
                int currentWave = m_waveManager != null ? m_waveManager.CurrentWave : 1;
                if (currentWave != waveAtStartOfWait)
                {
                    // O intervalo completo da nova onda começa no momento da transição.
                    waveAtStartOfWait = currentWave;
                    elapsedTime = 0f;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }

    private float GetCurrentSpawnInterval()
    {
        int currentWave = m_waveManager != null ? m_waveManager.CurrentWave : 1;
        float multiplier = m_baseSpawnMultiplier + (currentWave - 1) * m_spawnMultiplierIncreasePerWave;
        multiplier = Mathf.Min(multiplier, m_maximumSpawnMultiplier);
        return m_spawnInterval / multiplier;
    }

    private void SpawnVirus()
    {
        // Sorteia uma direção e posiciona o vírus à distância configurada.
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f)
            * m_spawnDistance;
        Vector3 spawnPosition = m_player.transform.position + offset;

        Virus virus = Instantiate(m_virusPrefab, spawnPosition, Quaternion.identity);
        if (m_waveManager != null)
        {
            m_waveManager.RegisterEnemy(virus);
            virus.Initialize(m_waveManager.GetVirusHealthForCurrentWave());
        }
        else
        {
            virus.Initialize();
        }
    }
}
