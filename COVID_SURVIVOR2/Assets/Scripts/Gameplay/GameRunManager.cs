using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controla só a sessão atual. A interface é criada ao abrir o jogo.
public class GameRunManager : MonoBehaviour
{
    [SerializeField] private Player m_player;
    [SerializeField] private WaveManager m_waveManager;
    [SerializeField, Min(0f)] private float m_defeatTransitionDelay = 0.85f;

    private float m_survivalSeconds;
    private bool m_gameEnded;
    private long m_finalScore;
    private Text m_timerText;
    private Text m_scoreText;

    private void Start()
    {
        if (m_player == null) m_player = FindAnyObjectByType<Player>();
        if (m_waveManager == null) m_waveManager = FindAnyObjectByType<WaveManager>();
        if (m_player == null || m_waveManager == null)
        {
            Debug.LogError("GameRunManager precisa de Player e WaveManager na cena.", this);
            enabled = false;
            return;
        }

        m_player.Died += HandlePlayerDied;
        CovidAudioManager.PlayMusic(MusicTrack.Game);
        CreateRunHud();
        UpdateHud();
    }

    private void Update()
    {
        if (m_gameEnded) return;
        // DeltaTime não conta os segundos de pausa da seleção de melhorias.
        m_survivalSeconds += Time.deltaTime;
        UpdateHud();
    }

    private long CalculateScore()
    {
        // Regra fácil de apresentar: 100 pontos por vírus + 1 por segundo sobrevivido.
        return (long)m_waveManager.TotalDefeated * 100L +
            Mathf.FloorToInt(m_survivalSeconds);
    }

    private void CreateRunHud()
    {
        Canvas canvas = SceneUIFactory.CreateCanvas("RunStatsCanvas", 30);
        SceneUIFactory.CreateImage("StatsPanel", canvas.transform,
            SceneUIFactory.Panel, new Vector2(0.5f, 1f),
            new Vector2(0f, -60f), new Vector2(460f, 106f));
        m_timerText = SceneUIFactory.CreateText("TimerText", canvas.transform, "TEMPO 00:00",
            30, FontStyle.Bold, SceneUIFactory.Cyan, new Vector2(0.5f, 1f),
            new Vector2(0f, -35f), new Vector2(440f, 38f));
        m_scoreText = SceneUIFactory.CreateText("ScoreText", canvas.transform, "PONTOS 0",
            26, FontStyle.Bold, SceneUIFactory.White, new Vector2(0.5f, 1f),
            new Vector2(0f, -79f), new Vector2(440f, 35f));
    }

    private void UpdateHud()
    {
        if (m_timerText != null)
            m_timerText.text = "TEMPO  " + SceneUIFactory.FormatTime(m_survivalSeconds);
        if (m_scoreText != null)
            m_scoreText.text = $"PONTOS  {(m_gameEnded ? m_finalScore : CalculateScore()):N0}";
    }

    private void HandlePlayerDied(BaseCharacter deadCharacter)
    {
        if (m_gameEnded || deadCharacter != m_player) return;
        m_gameEnded = true;
        m_finalScore = CalculateScore();
        GameRunState.Save(m_finalScore, m_waveManager.TotalDefeated,
            m_waveManager.CurrentWave, m_survivalSeconds);
        CovidAudioManager.Play(SoundCue.Defeat);

        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            // Desativar um componente não para automaticamente corrotinas já iniciadas.
            spawner.StopAllCoroutines();
            spawner.enabled = false;
        }
        UpdateHud();
        StartCoroutine(OpenDefeatAfterAnimation());
    }

    private IEnumerator OpenDefeatAfterAnimation()
    {
        // Espera em tempo real para manter o prazo previsível mesmo após uma pausa.
        yield return new WaitForSecondsRealtime(m_defeatTransitionDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    private void OnDestroy()
    {
        if (m_player != null)
            m_player.Died -= HandlePlayerDied;
    }
}
