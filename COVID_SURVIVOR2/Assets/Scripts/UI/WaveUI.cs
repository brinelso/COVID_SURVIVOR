using UnityEngine;
using UnityEngine.UI;

// Mostra a onda, o progresso de eliminações e a barra correspondente no HUD.
public class WaveUI : MonoBehaviour
{
    [SerializeField] private WaveManager m_waveManager;

    private Text m_waveText;
    private Text m_killsText;
    private Image m_progressFill;
    private RectTransform m_progressFillRect;
    private RectTransform m_progressTrackRect;
    private GameObject m_panelObject;

    private void Awake()
    {
        CreateWavePanel();
    }

    private void OnEnable()
    {
        // Garante que o painel exista antes de receber a primeira atualização.
        CreateWavePanel();

        if (m_waveManager == null)
            m_waveManager = FindAnyObjectByType<WaveManager>();

        if (m_waveManager == null)
        {
            Debug.LogError("WaveUI precisa de um WaveManager na cena.", this);
            return;
        }

        m_waveManager.WaveChanged += UpdateWaveDisplay;
        UpdateWaveDisplay(m_waveManager.CurrentWave, m_waveManager.DefeatedThisWave,
            m_waveManager.KillsRequiredPerWave);
    }

    private void OnDisable()
    {
        if (m_waveManager != null)
            m_waveManager.WaveChanged -= UpdateWaveDisplay;
    }

    private void CreateWavePanel()
    {
        // Awake e OnEnable podem chamar este método; nunca crie dois painéis.
        if (m_panelObject != null) return;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        m_panelObject = new GameObject("WavePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        m_panelObject.transform.SetParent(transform, false);

        RectTransform panelRect = m_panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.one;
        panelRect.anchorMax = Vector2.one;
        panelRect.pivot = Vector2.one;
        panelRect.anchoredPosition = new Vector2(-24f, -24f);
        panelRect.sizeDelta = new Vector2(380f, 112f);

        Image panelImage = m_panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.04f, 0.07f, 0.12f, 0.88f);
        panelImage.raycastTarget = false;

        m_waveText = CreateText("WaveTitle", m_panelObject.transform, font, 30, FontStyle.Bold,
            new Vector2(20f, -12f), new Vector2(340f, 38f), TextAnchor.MiddleLeft);
        m_killsText = CreateText("WaveKills", m_panelObject.transform, font, 22, FontStyle.Normal,
            new Vector2(20f, -52f), new Vector2(340f, 30f), TextAnchor.MiddleLeft);

        GameObject trackObject = new GameObject("KillsProgressTrack", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        trackObject.transform.SetParent(m_panelObject.transform, false);
        RectTransform trackRect = trackObject.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0f, 1f);
        trackRect.anchorMax = new Vector2(0f, 1f);
        trackRect.pivot = new Vector2(0f, 1f);
        trackRect.anchoredPosition = new Vector2(20f, -88f);
        trackRect.sizeDelta = new Vector2(340f, 12f);
        m_progressTrackRect = trackRect;

        Image trackImage = trackObject.GetComponent<Image>();
        trackImage.color = new Color(0.13f, 0.17f, 0.23f, 1f);
        trackImage.raycastTarget = false;

        GameObject fillObject = new GameObject("KillsProgressFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillObject.transform.SetParent(trackObject.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.zero;
        fillRect.pivot = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(0f, 12f);

        m_progressFill = fillObject.GetComponent<Image>();
        m_progressFill.type = Image.Type.Simple;
        m_progressFill.color = new Color(0.18f, 0.78f, 0.36f, 1f);
        m_progressFill.raycastTarget = false;
        m_progressFillRect = fillRect;
    }

    private Text CreateText(string objectName, Transform parent, Font font, int fontSize,
        FontStyle style, Vector2 position, Vector2 size, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private void UpdateWaveDisplay(int wave, int defeated, int required)
    {
        if (m_waveText == null || m_killsText == null || m_progressFill == null) return;

        m_waveText.text = $"ONDA {wave}";
        m_killsText.text = $"Eliminações: {defeated} / {required}";
        float progress = required > 0 ? Mathf.Clamp01((float)defeated / required) : 0f;
        // Ajustar a largura do retângulo deixa o preenchimento previsível sem depender de sprite.
        m_progressFillRect.sizeDelta = new Vector2(m_progressTrackRect.rect.width * progress,
            m_progressTrackRect.rect.height);
    }
}
