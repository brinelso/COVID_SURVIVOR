using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Centraliza a criação da interface sem exigir pacotes ou prefabs novos.
public static class SceneUIFactory
{
    public static readonly Color Background = new Color(0.055f, 0.075f, 0.12f, 1f);
    public static readonly Color Panel = new Color(0.10f, 0.14f, 0.22f, 0.96f);
    public static readonly Color Cyan = new Color(0.38f, 0.91f, 0.94f, 1f);
    public static readonly Color White = new Color(0.94f, 0.97f, 1f, 1f);

    public static Canvas CreateCanvas(string name, int sortingOrder = 100)
    {
        GameObject canvasObject = new GameObject(name, typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();
        return canvas;
    }

    public static RectTransform CreateRect(string name, Transform parent,
        Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    public static Image CreateImage(string name, Transform parent, Color color,
        Vector2 anchor, Vector2 position, Vector2 size)
    {
        RectTransform rect = CreateRect(name, parent, anchor, position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static void FillScreen(Transform parent, Color color)
    {
        Image image = CreateImage("FullScreenBackground", parent, color,
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
    }

    public static Text CreateText(string name, Transform parent, string content, int fontSize,
        FontStyle fontStyle, Color color, Vector2 anchor, Vector2 position,
        Vector2 size, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        RectTransform rect = CreateRect(name, parent, anchor, position, size);
        Text label = rect.gameObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = content;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.color = color;
        label.alignment = alignment;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        return label;
    }

    public static Button CreateButton(string name, Transform parent, string title,
        Vector2 anchor, Vector2 position, Action onClicked, bool primary = true)
    {
        Color background = primary ? Cyan : Panel;
        Image image = CreateImage(name, parent, background, anchor, position, new Vector2(400f, 84f));
        image.raycastTarget = true;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.82f, 0.94f, 1f, 1f);
        colors.pressedColor = new Color(0.59f, 0.73f, 0.82f, 1f);
        button.colors = colors;
        button.onClick.AddListener(() =>
        {
            CovidAudioManager.Play(SoundCue.Click);
            onClicked?.Invoke();
        });
        CreateText("Label", button.transform, title, 31, FontStyle.Bold,
            primary ? Background : White, new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(390f, 78f));
        return button;
    }

    public static void CreateVirusDecoration(Transform parent)
    {
        Sprite virus = Resources.Load<Sprite>("Characters/Virus/Walk/frame_00");
        if (virus == null) return;
        Image icon = CreateImage("VirusDecoration", parent,
            new Color(1f, 1f, 1f, 0.33f), new Vector2(0.83f, 0.52f),
            Vector2.zero, new Vector2(580f, 580f));
        icon.sprite = virus;
        icon.preserveAspect = true;
    }

    public static void EnsureEventSystem()
    {
        EventSystem current = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
        if (current == null)
        {
            GameObject objectForInput = new GameObject("EventSystem", typeof(EventSystem));
            current = objectForInput.GetComponent<EventSystem>();
        }
        InputSystemUIInputModule inputModule = current.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
            inputModule = current.gameObject.AddComponent<InputSystemUIInputModule>();
        // Sem ações padrão, os botões não recebem cliques no novo menu.
        if (inputModule.actionsAsset == null)
            inputModule.AssignDefaultActions();
    }

    // Os menus só têm Canvas; sem AudioListener não se ouve nenhum som.
    // Na cena de jogo a câmera principal já possui AudioListener próprio.
    public static void EnsureMenuAudioListener(Transform owner)
    {
        if (UnityEngine.Object.FindFirstObjectByType<AudioListener>() == null)
            owner.gameObject.AddComponent<AudioListener>();
    }

    public static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.FloorToInt(Mathf.Max(0f, seconds));
        int hours = totalSeconds / 3600;
        int minutes = totalSeconds % 3600 / 60;
        int remainingSeconds = totalSeconds % 60;
        return hours > 0
            ? $"{hours:00}:{minutes:00}:{remainingSeconds:00}"
            : $"{minutes:00}:{remainingSeconds:00}";
    }
}
