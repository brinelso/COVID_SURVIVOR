using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// A derrota não encerra o aplicativo; permite uma partida nova ou voltar ao menu.
public class GameOverUI : MonoBehaviour
{
    private void Awake()
    {
        Time.timeScale = 1f;
        Canvas canvas = SceneUIFactory.CreateCanvas("DefeatCanvas");
        SceneUIFactory.EnsureMenuAudioListener(canvas.transform);
        CovidAudioManager.PlayMusic(MusicTrack.Defeat);
        SceneUIFactory.FillScreen(canvas.transform, SceneUIFactory.Background);
        SceneUIFactory.CreateVirusDecoration(canvas.transform);

        SceneUIFactory.CreateText("Header", canvas.transform, "FIM DE JOGO",
            92, FontStyle.Bold, SceneUIFactory.White, new Vector2(0.38f, 0.5f),
            new Vector2(0f, 250f), new Vector2(920f, 130f));
        SceneUIFactory.CreateText("Score", canvas.transform,
            $"PONTUAÇÃO: {GameRunState.LastScore:N0}",
            52, FontStyle.Bold, SceneUIFactory.Cyan, new Vector2(0.38f, 0.5f),
            new Vector2(0f, 120f), new Vector2(900f, 82f));
        SceneUIFactory.CreateText("Details", canvas.transform,
            $"Tempo: {SceneUIFactory.FormatTime(GameRunState.LastSeconds)}     " +
            $"Eliminações: {GameRunState.LastEliminations}     Onda: {GameRunState.LastWave}",
            27, FontStyle.Normal, SceneUIFactory.White, new Vector2(0.38f, 0.5f),
            new Vector2(0f, 40f), new Vector2(900f, 65f));
        SceneUIFactory.CreateButton("RetryButton", canvas.transform, "REINICIAR",
            new Vector2(0.38f, 0.5f), new Vector2(0f, -165f), Restart);
        SceneUIFactory.CreateButton("MenuButton", canvas.transform, "MENU PRINCIPAL",
            new Vector2(0.38f, 0.5f), new Vector2(0f, -270f), BackToMenu, false);
    }

    private void Restart()
    {
        GameRunState.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    private void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
