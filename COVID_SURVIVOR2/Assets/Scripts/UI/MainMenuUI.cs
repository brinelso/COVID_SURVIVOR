using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controla a primeira cena. O jogo em si permanece em SampleScene.
public class MainMenuUI : MonoBehaviour
{
    private void Awake()
    {
        Time.timeScale = 1f;
        Canvas canvas = SceneUIFactory.CreateCanvas("MainMenuCanvas");
        SceneUIFactory.EnsureMenuAudioListener(canvas.transform);
        CovidAudioManager.PlayMusic(MusicTrack.Menu);
        SceneUIFactory.FillScreen(canvas.transform, SceneUIFactory.Background);
        SceneUIFactory.CreateVirusDecoration(canvas.transform);
        SceneUIFactory.CreateText("Eyebrow", canvas.transform, "SOBREVIVA À INFECÇÃO",
            25, FontStyle.Bold, SceneUIFactory.Cyan, new Vector2(0.38f, 0.5f),
            new Vector2(0f, 245f), new Vector2(900f, 50f));
        SceneUIFactory.CreateText("Title", canvas.transform, "COVID\nSURVIVOR",
            103, FontStyle.Bold, SceneUIFactory.White, new Vector2(0.38f, 0.5f),
            new Vector2(0f, 90f), new Vector2(900f, 265f));
        SceneUIFactory.CreateText("Instructions", canvas.transform,
            "Mova-se com WASD ou setas. Colete XP, evolua sua magia e resista o máximo possível.",
            26, FontStyle.Normal, SceneUIFactory.White, new Vector2(0.38f, 0.5f),
            new Vector2(0f, -75f), new Vector2(830f, 110f));
        SceneUIFactory.CreateButton("PlayButton", canvas.transform, "JOGAR",
            new Vector2(0.38f, 0.5f), new Vector2(0f, -240f), StartGame);
        SceneUIFactory.CreateButton("QuitButton", canvas.transform, "SAIR",
            new Vector2(0.38f, 0.5f), new Vector2(0f, -345f), QuitGame, false);
    }

    private void StartGame()
    {
        GameRunState.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
