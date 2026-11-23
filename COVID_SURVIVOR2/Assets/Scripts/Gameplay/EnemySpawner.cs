using System;
using System.Collections;
using UnityEngine;

// Seleciona as variantes por onda sem criar um segundo sistema de inimigos.
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Virus m_virusPrefab;
    [SerializeField] private Virus m_runnerPrefab;
    [SerializeField] private Virus m_tankPrefab;
    [SerializeField] private Virus m_miniBossPrefab;
    [SerializeField] private Virus m_bossPrefab;
    [SerializeField] private Player m_player;
    [SerializeField] private WaveManager m_waveManager;
    [SerializeField] private float m_spawnInterval = 3f;
    [SerializeField] private float m_spawnDistance = 4f;

    [Header("Multiplicador de spawn por onda")]
    [SerializeField, Min(0.1f)] private float m_baseSpawnMultiplier = 1f;
    [SerializeField, Min(0f)] private float m_spawnMultiplierIncreasePerWave = 0.5f;
    [SerializeField, Min(0.1f)] private float m_maximumSpawnMultiplier = 2f;

    [Header("Variedades e chefes")]
    [SerializeField, Min(1)] private int m_runnerStartingWave = 2;
    [SerializeField, Min(1)] private int m_tankStartingWave = 4;
    [SerializeField, Min(2)] private int m_miniBossEveryWaves = 5;
    [SerializeField, Min(2)] private int m_bossEveryWaves = 10;
    [SerializeField, Range(0f, 1f)] private float m_runnerBaseChance = 0.28f;
    [SerializeField, Range(0f, 1f)] private float m_tankBaseChance = 0.13f;

    private int m_lastSpecialSpawnWave;

    private void Start()
    {
        if (m_player == null)
            m_player = FindAnyObjectByType<Player>();
        if (m_waveManager == null)
            m_waveManager = GetComponent<WaveManager>();

        if (m_virusPrefab == null || m_player == null || m_spawnInterval <= 0f ||
            m_spawnDistance <= 0f || m_baseSpawnMultiplier <= 0f ||
            m_maximumSpawnMultiplier <= 0f)
        {
            Debug.LogError("Configure o vírus comum, Player e intervalos de spawn.", this);
            return;
        }

        m_spawnMultiplierIncreasePerWave = Mathf.Max(0f, m_spawnMultiplierIncreasePerWave);
        if (m_waveManager != null)
            m_waveManager.WaveChanged += HandleWaveChanged;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (m_player != null && m_player.IsAlive)
        {
            SpawnEnemy(ChooseRegularEnemy(), m_spawnDistance);
            float elapsedTime = 0f;
            int startWave = m_waveManager != null ? m_waveManager.CurrentWave : 1;
            while (elapsedTime < GetCurrentSpawnInterval())
            {
                int currentWave = m_waveManager != null ? m_waveManager.CurrentWave : 1;
                if (currentWave != startWave)
                {
                    startWave = currentWave;
                    elapsedTime = 0f;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }

    private Virus ChooseRegularEnemy()
    {
        int wave = m_waveManager != null ? m_waveManager.CurrentWave : 1;
        // O tanque é menos frequente e só aparece depois dos primeiros upgrades.
        float tankChance = Mathf.Min(0.30f,
            m_tankBaseChance + Mathf.Max(0, wave - m_tankStartingWave) * 0.012f);
        if (wave >= m_tankStartingWave && m_tankPrefab != null &&
            UnityEngine.Random.value < tankChance)
            return m_tankPrefab;

        float runnerChance = Mathf.Min(0.50f,
            m_runnerBaseChance + Mathf.Max(0, wave - m_runnerStartingWave) * 0.012f);
        if (wave >= m_runnerStartingWave && m_runnerPrefab != null &&
            UnityEngine.Random.value < runnerChance)
            return m_runnerPrefab;

        return m_virusPrefab;
    }

    private void HandleWaveChanged(int currentWave, int defeated, int required)
    {
        // WaveChanged ocorre também a cada eliminação. Só gera um especial
        // quando uma NOVA onda elegível começa; nunca um por morte.
        if (currentWave <= 1 || currentWave == m_lastSpecialSpawnWave ||
            m_player == null || !m_player.IsAlive)
            return;

        if (m_bossEveryWaves > 1 && currentWave % m_bossEveryWaves == 0 &&
            m_bossPrefab != null)
        {
            m_lastSpecialSpawnWave = currentWave;
            SpawnEnemy(m_bossPrefab, m_spawnDistance * 1.65f);
        }
        else if (m_miniBossEveryWaves > 1 &&
                 currentWave % m_miniBossEveryWaves == 0 &&
                 m_miniBossPrefab != null)
        {
            m_lastSpecialSpawnWave = currentWave;
            SpawnEnemy(m_miniBossPrefab, m_spawnDistance * 1.45f);
        }
    }

    private float GetCurrentSpawnInterval()
    {
        int wave = m_waveManager != null ? m_waveManager.CurrentWave : 1;
        float multiplier = m_baseSpawnMultiplier +
            (wave - 1) * m_spawnMultiplierIncreasePerWave;
        multiplier = Mathf.Min(multiplier, m_maximumSpawnMultiplier);
        return m_spawnInterval / multiplier;
    }

    private void SpawnEnemy(Virus prefab, float distance)
    {
        if (prefab == null || m_player == null || !m_player.IsAlive)
            return;

        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;
        Virus virus = Instantiate(prefab, m_player.transform.position + offset, Quaternion.identity);

        if (m_waveManager != null)
        {
            m_waveManager.RegisterEnemy(virus);
            double hp = m_waveManager.GetVirusHealthForCurrentWave() *
                        (double)virus.HealthMultiplier;
            virus.Initialize((int)Math.Min(int.MaxValue, Math.Max(1d, Math.Ceiling(hp))));
        }
        else
            virus.Initialize();
    }

    private void OnDestroy()
    {
        if (m_waveManager != null)
            m_waveManager.WaveChanged -= HandleWaveChanged;
    }
}
