using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent singleton (DontDestroyOnLoad) that owns all audio playback.
///
/// MUSIC: one clip for the main menu, and 3 clips for the game (index 0 = Small,
/// 1 = Normal, 2 = Large — same order as PlayerScale.ScaleStage). Crossfades
/// automatically between them.
///
/// SFX: everything is driven by the SFXType enum. Assign clips per type in
/// "Sfx Library" in the Inspector. Each entry has its own "Enabled" checkbox —
/// uncheck it to silence that one sound everywhere without touching code.
///
/// One-shot sounds: PlaySFX(SFXType.Jump, transform.position)
/// Continuous/looping sounds: StartLoopSFX(myAudioSource, SFXType.Move) / StopLoopSFX(myAudioSource)
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SFXEntry
    {
        public SFXType type;
        public bool enabled = true;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 1.5f)] public float minPitch = 0.95f;
        [Range(0.5f, 1.5f)] public float maxPitch = 1.05f;
    }

    [Header("Music")]
    public AudioClip mainMenuMusic;
    [Tooltip("Index 0 = Small, 1 = Normal, 2 = Large — matches PlayerScale.ScaleStage.")]
    public AudioClip[] gameMusicByScale = new AudioClip[3];
    public float musicCrossfadeDuration = 1.5f;

    [Header("SFX Library")]
    public List<SFXEntry> sfxLibrary = new List<SFXEntry>();

    [Header("One-Shot Pool")]
    public int oneShotPoolSize = 12;

    [Header("Saved Volume (0-1)")]
    [Range(0f, 1f)] public float defaultMusicVolume = 1f;
    [Range(0f, 1f)] public float defaultSFXVolume = 1f;

    private const string MusicVolKey = "Audio_MusicVolume";
    private const string SFXVolKey = "Audio_SFXVolume";

    private float musicVolume;
    private float sfxVolume;

    private Dictionary<SFXType, SFXEntry> sfxLookup;
    private AudioSource[] oneShotPool;
    private int poolIndex;

    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;
    private AudioClip currentMusicClip;
    private Coroutine crossfadeRoutine;

    private readonly List<AudioSource> loopSources = new List<AudioSource>();
    private readonly Dictionary<AudioSource, SFXType> loopSourceTypes = new Dictionary<AudioSource, SFXType>();

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            if (activeMusicSource != null) activeMusicSource.volume = musicVolume;
            PlayerPrefs.SetFloat(MusicVolKey, musicVolume);
        }
    }

    public float SFXVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SFXVolKey, sfxVolume);
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLookup();
        BuildMusicSources();
        BuildOneShotPool();

        musicVolume = PlayerPrefs.HasKey(MusicVolKey) ? PlayerPrefs.GetFloat(MusicVolKey) : defaultMusicVolume;
        sfxVolume = PlayerPrefs.HasKey(SFXVolKey) ? PlayerPrefs.GetFloat(SFXVolKey) : defaultSFXVolume;
        if (activeMusicSource != null) activeMusicSource.volume = musicVolume;
    }

    void Reset()
    {
        // Convenience: pre-populate one row per SFXType so you just drop clips in.
        sfxLibrary.Clear();
        foreach (SFXType type in System.Enum.GetValues(typeof(SFXType)))
        {
            sfxLibrary.Add(new SFXEntry { type = type, enabled = true, clips = new AudioClip[0] });
        }
    }

    void Update()
    {
        // Keep any active looping sources in sync with live volume/enabled changes.
        for (int i = loopSources.Count - 1; i >= 0; i--)
        {
            AudioSource src = loopSources[i];
            if (src == null)
            {
                loopSources.RemoveAt(i);
                continue;
            }

            SFXType type = loopSourceTypes[src];
            SFXEntry entry = GetEntry(type);
            if (entry == null || !entry.enabled)
            {
                if (src.isPlaying) src.Stop();
                continue;
            }

            src.volume = entry.volume * sfxVolume;
        }
    }

    void BuildLookup()
    {
        sfxLookup = new Dictionary<SFXType, SFXEntry>();
        foreach (SFXEntry entry in sfxLibrary)
        {
            if (entry == null) continue;
            sfxLookup[entry.type] = entry;
        }
    }

    void BuildMusicSources()
    {
        musicSourceA = CreateSource("MusicSourceA", loop: true, spatialBlend: 0f);
        musicSourceB = CreateSource("MusicSourceB", loop: true, spatialBlend: 0f);
        activeMusicSource = musicSourceA;
        inactiveMusicSource = musicSourceB;
    }

    void BuildOneShotPool()
    {
        oneShotPoolSize = Mathf.Max(1, oneShotPoolSize);
        oneShotPool = new AudioSource[oneShotPoolSize];
        for (int i = 0; i < oneShotPoolSize; i++)
        {
            oneShotPool[i] = CreateSource($"SFXOneShot_{i}", loop: false, spatialBlend: 0f);
        }
    }

    AudioSource CreateSource(string name, bool loop, float spatialBlend)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = loop;
        src.spatialBlend = spatialBlend;
        return src;
    }

    SFXEntry GetEntry(SFXType type)
    {
        if (sfxLookup == null) BuildLookup();
        sfxLookup.TryGetValue(type, out SFXEntry entry);
        return entry;
    }

    AudioClip PickClip(SFXEntry entry)
    {
        if (entry.clips == null || entry.clips.Length == 0) return null;
        return entry.clips[Random.Range(0, entry.clips.Length)];
    }

    // ---------------- One-shot SFX ----------------

    /// <summary>Plays a one-shot sound. Pass overrideClip to use a specific clip instead of a random one from the library entry.</summary>
    public void PlaySFX(SFXType type, Vector3? position = null, AudioClip overrideClip = null)
    {
        SFXEntry entry = GetEntry(type);
        if (entry == null || !entry.enabled) return;

        AudioClip clip = overrideClip != null ? overrideClip : PickClip(entry);
        if (clip == null) return;

        AudioSource src = oneShotPool[poolIndex];
        poolIndex = (poolIndex + 1) % oneShotPool.Length;

        if (position.HasValue) src.transform.position = position.Value;
        src.pitch = Random.Range(entry.minPitch, entry.maxPitch);
        src.PlayOneShot(clip, entry.volume * sfxVolume);
    }

    public bool IsSFXTypeEnabled(SFXType type)
    {
        SFXEntry entry = GetEntry(type);
        return entry != null && entry.enabled;
    }

    // ---------------- Looping SFX ----------------

    /// <summary>Starts (or keeps playing) a looping sound on the given AudioSource. Safe to call every frame.</summary>
    public void StartLoopSFX(AudioSource source, SFXType type)
    {
        if (source == null) return;

        SFXEntry entry = GetEntry(type);
        if (entry == null || !entry.enabled)
        {
            StopLoopSFX(source);
            return;
        }

        AudioClip clip = PickClip(entry);
        if (clip == null)
        {
            StopLoopSFX(source);
            return;
        }

        if (source.clip != clip) source.clip = clip;
        source.loop = true;
        source.volume = entry.volume * sfxVolume;

        if (!source.isPlaying) source.Play();

        if (!loopSourceTypes.ContainsKey(source))
        {
            loopSourceTypes[source] = type;
            loopSources.Add(source);
        }
        else
        {
            loopSourceTypes[source] = type;
        }
    }

    /// <summary>Stops a looping sound previously started with StartLoopSFX. Safe to call every frame / even if never started.</summary>
    public void StopLoopSFX(AudioSource source)
    {
        if (source == null) return;
        if (source.isPlaying) source.Stop();
        loopSourceTypes.Remove(source);
        loopSources.Remove(source);
    }

    // ---------------- Music ----------------

    public void PlayMainMenuMusic()
    {
        CrossfadeTo(mainMenuMusic);
    }

    /// <summary>stageIndex should match PlayerScale.ScaleStage: 0 = Small, 1 = Normal, 2 = Large.</summary>
    public void PlayGameMusicForStage(int stageIndex)
    {
        if (gameMusicByScale == null || stageIndex < 0 || stageIndex >= gameMusicByScale.Length) return;
        CrossfadeTo(gameMusicByScale[stageIndex]);
    }

    void CrossfadeTo(AudioClip clip)
    {
        if (clip == null || clip == currentMusicClip) return;
        currentMusicClip = clip;

        if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
        crossfadeRoutine = StartCoroutine(CrossfadeRoutine(clip));
    }

    IEnumerator CrossfadeRoutine(AudioClip newClip)
    {
        AudioSource from = activeMusicSource;
        AudioSource to = inactiveMusicSource;

        to.clip = newClip;
        to.volume = 0f;
        to.Play();

        float t = 0f;
        float duration = Mathf.Max(0.01f, musicCrossfadeDuration);
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // keeps fading correctly even if the game is paused
            float f = t / duration;
            to.volume = Mathf.Lerp(0f, musicVolume, f);
            from.volume = Mathf.Lerp(musicVolume, 0f, f);
            yield return null;
        }

        from.Stop();
        to.volume = musicVolume;

        activeMusicSource = to;
        inactiveMusicSource = from;
        crossfadeRoutine = null;
    }
}
