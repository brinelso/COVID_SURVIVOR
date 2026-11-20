using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Cria e apresenta quatro opções dentro do Canvas existente.
public class UpgradeChoiceUI : MonoBehaviour
{
    private const int ChoiceCount = 4;
    private GameObject m_panel;
    private Text m_titleText;
    private Button[] m_optionButtons;
    private Text[] m_optionTexts;
    private Image[] m_optionImages;
    private GameObject m_debugButtonObject;
    private Button m_debugButton;
    private Text m_debugButtonLabel;
    private Action<UpgradeChoice> m_onOptionSelected;
    private Action m_onDebugLevelUp;

    private void Awake()
    {
        EnsureGraphicRaycaster();
        if (!enabled) return;
        CreatePanel();
        CreateDebugButton();
        EnsureInputSystemEventSystem();
    }

    public void SetDebugLevelUpAction(Action action)
    {
        m_onDebugLevelUp = action;
    }

    public void SetDebugLevelUpAvailable(bool available)
    {
        if (m_debugButton != null)
            m_debugButton.interactable = available;

        if (m_debugButtonLabel != null)
        {
            m_debugButtonLabel.text = available
                ? "DEBUG — Subir nível"
                : "DEBUG — Limite de 3 elementos";
        }
    }

    public void Show(int level, UpgradeChoice[] options, Action<UpgradeChoice> onOptionSelected)
    {
        if (options == null || options.Length != ChoiceCount)
        {
            Debug.LogError("A tela de melhorias precisa receber exatamente quatro opções.", this);
            return;
        }

        m_onOptionSelected = onOptionSelected;
        m_debugButtonObject.SetActive(false);
        m_titleText.text = $"Nível {level} alcançado! Escolha uma melhoria";

        for (int i = 0; i < m_optionButtons.Length; i++)
        {
            UpgradeChoice option = options[i];
            if (option == null)
            {
                Debug.LogError($"A opção {i + 1} está vazia.", this);
                return;
            }

            m_optionTexts[i].text = $"{GetCategoryName(option.Category)}\n\n{option.DisplayName}\n\n{option.Description}";
            m_optionImages[i].color = GetCategoryColor(option);
            m_optionButtons[i].onClick.RemoveAllListeners();
            m_optionButtons[i].onClick.AddListener(() => SelectOption(option));
        }

        m_panel.SetActive(true);
    }

    public void Hide()
    {
        if (m_panel != null) m_panel.SetActive(false);
        if (m_debugButtonObject != null) m_debugButtonObject.SetActive(true);
        m_onOptionSelected = null;
    }

    private void CreateDebugButton()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        m_debugButtonObject = CreateUIObject("DEBUG_LevelUpButton", transform,
            new Vector2(0f, 0f), new Vector2(240f, 64f), new Vector2(140f, 50f));
        Image background = m_debugButtonObject.AddComponent<Image>();
        background.color = new Color(0.62f, 0.12f, 0.08f, 0.96f);
        m_debugButton = m_debugButtonObject.AddComponent<Button>();
        m_debugButton.targetGraphic = background;
        m_debugButton.onClick.AddListener(() => m_onDebugLevelUp?.Invoke());

        m_debugButtonLabel = CreateText("DEBUG_Label", m_debugButtonObject.transform, font, 20,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(230f, 58f));
        m_debugButtonLabel.text = "DEBUG — Subir nível";
        m_debugButtonLabel.color = Color.white;
    }

    private void SelectOption(UpgradeChoice option)
    {
        Action<UpgradeChoice> callback = m_onOptionSelected;
        if (callback == null) return;

        Hide();
        callback(option);
    }

    private void CreatePanel()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        m_panel = CreateUIObject("UpgradeChoicePanel", transform, new Vector2(0.5f, 0.5f), new Vector2(1320f, 500f));
        Image panelImage = m_panel.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.1f, 0.16f, 0.96f);

        m_titleText = CreateText("UpgradeTitle", m_panel.transform, font, 32, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(1240f, 70f));
        m_titleText.color = Color.white;

        m_optionButtons = new Button[ChoiceCount];
        m_optionTexts = new Text[ChoiceCount];
        m_optionImages = new Image[ChoiceCount];
        for (int i = 0; i < ChoiceCount; i++)
        {
            float x = (i - 1.5f) * 300f;
            GameObject buttonObject = CreateUIObject($"UpgradeOption{i + 1}", m_panel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(280f, 320f), new Vector2(x, -22f));
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.48f, 0.34f, 0.12f, 1f);
            m_optionImages[i] = buttonImage;

            Button button = buttonObject.AddComponent<Button>();
            button.interactable = true;
            button.targetGraphic = buttonImage;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.28f, 0.45f, 0.58f, 1f);
            colors.pressedColor = new Color(0.12f, 0.23f, 0.34f, 1f);
            button.colors = colors;
            m_optionButtons[i] = button;

            Text optionText = CreateText("OptionText", buttonObject.transform, font, 21, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(255f, 290f));
            optionText.color = Color.white;
            optionText.raycastTarget = false;
            m_optionTexts[i] = optionText;
        }

        m_panel.SetActive(false);
    }

    private static GameObject CreateUIObject(string objectName, Transform parent, Vector2 anchor,
        Vector2 size, Vector2 anchoredPosition = default)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)uiObject.transform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        return uiObject;
    }

    private static Text CreateText(string objectName, Transform parent, Font font, int size,
        TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 dimensions)
    {
        GameObject textObject = CreateUIObject(objectName, parent, anchorMin, dimensions, position);
        RectTransform rect = (RectTransform)textObject.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static string GetCategoryName(UpgradeCategory category)
    {
        switch (category)
        {
            case UpgradeCategory.SpellUpgrade: return "MELHORIA DA MAGIA";
            case UpgradeCategory.Element: return "ELEMENTO";
            default: return "ATRIBUTO GERAL";
        }
    }

    private static Color GetCategoryColor(UpgradeChoice option)
    {
        if (option.IsElementChoice)
            return Color.Lerp(option.Color, new Color(0.08f, 0.1f, 0.16f, 1f), 0.45f);

        switch (option.Category)
        {
            case UpgradeCategory.SpellUpgrade:
                return new Color(0.12f, 0.32f, 0.52f, 1f);
            default:
                return new Color(0.38f, 0.3f, 0.16f, 1f);
        }
    }

    private static void EnsureInputSystemEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        if (inputModule.actionsAsset == null)
            inputModule.AssignDefaultActions();
    }

    private void EnsureGraphicRaycaster()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("UpgradeChoiceUI precisa estar no Canvas ou em um objeto filho dele.", this);
            enabled = false;
            return;
        }

        if (canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();
    }
}
