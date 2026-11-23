using System;
using System.Collections.Generic;
using UnityEngine;

// Troca o sprite do SpriteRenderer deste objeto em sequência, como um flipbook.
// Cada clipe é uma pasta dentro de Assets/Resources, com os frames em ordem alfabética
// (frame_00, frame_01, ...). Ex.: Resources/Characters/Doctor/Walk/frame_00.png.
public class SpriteAnimator : MonoBehaviour
{
    // Pasta base dentro de Resources, por exemplo "Characters/Doctor".
    private string m_resourcesFolder = string.Empty;

    private SpriteRenderer m_spriteRenderer;

    // Cache: cada clipe é lido do disco apenas na primeira vez.
    private readonly Dictionary<string, Sprite[]> m_clipCache = new();

    private Sprite[] m_currentFrames;
    private string m_currentClipName = string.Empty;
    private float m_frameDuration;
    private float m_elapsed;
    private bool m_loop;
    private bool m_isPlaying;
    private Action m_onFinished;

    public bool IsPlaying => m_isPlaying;

    // Permite manter o ataque visível enquanto o personagem continua andando.
    public bool IsPlayingClip(string clipName)
    {
        return m_isPlaying && m_currentClipName == clipName;
    }

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        if (m_spriteRenderer == null)
            Debug.LogError("SpriteAnimator precisa de um SpriteRenderer no mesmo objeto.", this);
    }

    // Define a pasta de frames. Deve ser chamado antes do primeiro Play.
    public void Configure(string resourcesFolder)
    {
        m_resourcesFolder = resourcesFolder;
    }

    public bool HasClip(string clipName)
    {
        return LoadClip(clipName).Length > 0;
    }

    // Toca um clipe em loop ou uma vez. Se o mesmo clipe já estiver tocando, não reinicia.
    public void Play(string clipName, float framesPerSecond, bool loop, Action onFinished = null)
    {
        if (m_isPlaying && m_currentClipName == clipName) return;

        Sprite[] frames = LoadClip(clipName);
        if (frames.Length == 0 || framesPerSecond <= 0f) return;

        m_currentClipName = clipName;
        m_currentFrames = frames;
        m_frameDuration = 1f / framesPerSecond;
        m_elapsed = 0f;
        m_loop = loop;
        m_onFinished = onFinished;
        m_isPlaying = true;

        ShowFrame(0);
    }

    // Atalho para animações que tocam uma única vez, como a morte.
    public void PlayOnce(string clipName, float framesPerSecond, Action onFinished = null)
    {
        Play(clipName, framesPerSecond, false, onFinished);
    }

    private void Update()
    {
        if (!m_isPlaying) return;

        m_elapsed += Time.deltaTime;
        int frameIndex = Mathf.FloorToInt(m_elapsed / m_frameDuration);
        int frameCount = m_currentFrames.Length;

        if (frameIndex < frameCount)
        {
            ShowFrame(frameIndex);
            return;
        }

        if (m_loop)
        {
            // Volta ao início sem perder o tempo que sobrou do último quadro.
            m_elapsed %= frameCount * m_frameDuration;
            ShowFrame(Mathf.FloorToInt(m_elapsed / m_frameDuration));
            return;
        }

        // Animação não repetível: mostra o último quadro e avisa quem pediu.
        m_isPlaying = false;
        ShowFrame(frameCount - 1);

        Action callback = m_onFinished;
        m_onFinished = null;
        callback?.Invoke();
    }

    private void ShowFrame(int index)
    {
        if (m_spriteRenderer != null)
            m_spriteRenderer.sprite = m_currentFrames[index];
    }

    private Sprite[] LoadClip(string clipName)
    {
        if (m_clipCache.TryGetValue(clipName, out Sprite[] cached))
            return cached;

        string path = string.IsNullOrEmpty(m_resourcesFolder)
            ? clipName
            : m_resourcesFolder + "/" + clipName;

        Sprite[] frames = Resources.LoadAll<Sprite>(path);

        // Resources não garante ordem: ordenamos pelo nome do arquivo.
        Array.Sort(frames, (a, b) => string.CompareOrdinal(a.name, b.name));

        if (frames.Length == 0)
            Debug.LogWarning($"Nenhum frame encontrado em Resources/{path}.", this);

        m_clipCache[clipName] = frames;
        return frames;
    }
}
