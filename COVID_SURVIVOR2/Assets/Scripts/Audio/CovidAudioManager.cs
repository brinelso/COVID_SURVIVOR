using System.Collections.Generic;
using UnityEngine;

// Identificadores claros para não espalhar caminhos de áudio pelo código.
public enum MusicTrack { None, Menu, Game, Defeat }
public enum SoundCue { Click, Shoot, ElementCast, EnemyHit, EnemyDeath, PlayerHit, Pickup, LevelUp, Defeat }

// Um objeto persistente controla músicas em loop e efeitos 2D nas três cenas.
public class CovidAudioManager : MonoBehaviour
{
    private static CovidAudioManager s_instance;

    [SerializeField, Range(0f, 1f)] private float m_musicVolume = 0.27f;
    [SerializeField, Range(0f, 1f)] private float m_effectVolume = 0.54f;

    private AudioSource m_musicSource;
    private AudioSource m_effectSource;
    private MusicTrack m_currentMusic = MusicTrack.None;
    private readonly Dictionary<string, AudioClip> m_clipCache = new Dictionary<string, AudioClip>();
    private readonly Dictionary<SoundCue, float> m_nextCueTime = new Dictionary<SoundCue, float>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticInstance() { s_instance = null; }

    private static CovidAudioManager Instance
    {
        get
        {
            if (s_instance != null) return s_instance;
            s_instance = FindFirstObjectByType<CovidAudioManager>();
            if (s_instance != null) return s_instance;
            GameObject manager = new GameObject("CovidAudioManager");
            return manager.AddComponent<CovidAudioManager>();
        }
    }

    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;
        DontDestroyOnLoad(gameObject);

        m_musicSource = gameObject.AddComponent<AudioSource>();
        m_musicSource.playOnAwake = false;
        m_musicSource.loop = true;
        m_musicSource.spatialBlend = 0f;
        m_musicSource.volume = m_musicVolume;

        m_effectSource = gameObject.AddComponent<AudioSource>();
        m_effectSource.playOnAwake = false;
        m_effectSource.spatialBlend = 0f;
        m_effectSource.volume = m_effectVolume;
    }

    public static void PlayMusic(MusicTrack track) { Instance.SetMusic(track); }
    public static void Play(SoundCue cue) { Instance.PlayEffect(cue); }

    private AudioClip GetClip(string relativePath)
    {
        if (m_clipCache.TryGetValue(relativePath, out AudioClip clip)) return clip;
        clip = Resources.Load<AudioClip>("Audio/" + relativePath);
        if (clip == null) Debug.LogError("Áudio não encontrado em Resources/Audio/" + relativePath);
        m_clipCache[relativePath] = clip;
        return clip;
    }

    private void SetMusic(MusicTrack track)
    {
        if (track == m_currentMusic && m_musicSource.isPlaying) return;
        m_musicSource.Stop();
        m_currentMusic = track;
        if (track == MusicTrack.None) return;

        string name = track == MusicTrack.Menu ? "menu_loop"
            : track == MusicTrack.Game ? "game_loop" : "defeat_loop";
        m_musicSource.clip = GetClip("Music/" + name);
        if (m_musicSource.clip != null) m_musicSource.Play();
    }

    private void PlayEffect(SoundCue cue)
    {
        float now = Time.unscaledTime;
        // Limite por tipo de evento, NAO por projétil. Mantem o combate audivel
        // sem formar uma parede de sons nos niveis mais altos.
        float cooldown = cue == SoundCue.Shoot ? 0.32f
            : cue == SoundCue.ElementCast ? 0.9f
            : cue == SoundCue.EnemyHit ? 0.30f
            : cue == SoundCue.EnemyDeath ? 0.30f
            : cue == SoundCue.Pickup ? 0.16f
            : cue == SoundCue.PlayerHit ? 0.28f
            : 0f;
        if (m_nextCueTime.TryGetValue(cue, out float nextTime) && now < nextTime) return;
        m_nextCueTime[cue] = now + cooldown;

        string clipName;
        switch (cue)
        {
            case SoundCue.Click: clipName = "ui_click"; break;
            case SoundCue.Shoot: clipName = "shoot"; break;
            case SoundCue.ElementCast: clipName = "element_cast"; break;
            case SoundCue.EnemyHit: clipName = "enemy_hit"; break;
            case SoundCue.EnemyDeath: clipName = "enemy_death"; break;
            case SoundCue.PlayerHit: clipName = "player_hit"; break;
            case SoundCue.Pickup: clipName = "pickup"; break;
            case SoundCue.LevelUp: clipName = "level_up"; break;
            default: clipName = "defeat"; break;
        }
        AudioClip clip = GetClip("SFX/" + clipName);
        // Os sons frequentes ficam mais baixos do que os avisos importantes.
        float volumeScale = cue == SoundCue.Shoot ? 0.70f
            : cue == SoundCue.ElementCast ? 0.55f
            : cue == SoundCue.EnemyHit ? 0.26f
            : cue == SoundCue.EnemyDeath ? 0.43f
            : cue == SoundCue.Pickup ? 0.42f
            : cue == SoundCue.Click ? 0.65f
            : 1f;
        if (clip != null) m_effectSource.PlayOneShot(clip, volumeScale);
    }
}
