
using UnityEngine;
using System.Collections.Generic;

// Persistent audio hub (DontDestroyOnLoad): looping BGM, one-shot SFX, looping footsteps and a
// reload sound that can be stopped as soon as the reload is interrupted.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    public AudioClip bgmClip;
    public AudioClip gunshotClip;
    public AudioClip grenadeExplosionClip;
    public AudioClip robotExplodeClip;
    public AudioClip footstepsClip;
    public AudioClip reloadClip;

    [Header("Volume")]
    [Range(0f, 1f)] public float bgmVolume = 0.4f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float gunshotVolume = 0.6f;
    [Range(0f, 1f)] public float footstepsVolume = 0.5f;
    [Range(0f, 1f)] public float reloadVolume = 0.9f;

    public AudioSource sfxSource;
    public Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    public Dictionary<string, float> clipVolumes = new Dictionary<string, float>();

    AudioSource bgmSource;
    AudioSource footstepSource;
    AudioSource reloadSource;
    AutoMoveController mover;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        bgmSource = CreateSource(true);
        footstepSource = CreateSource(true);
        reloadSource = CreateSource(false);

        GenerateClips();

        if (bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
        if (footstepsClip != null)
        {
            footstepSource.clip = footstepsClip;
            footstepSource.volume = footstepsVolume;
        }
    }

    AudioSource CreateSource(bool loop)
    {
        AudioSource s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = loop;
        s.spatialBlend = 0f;
        return s;
    }

    void GenerateClips()
    {
        clips["shoot"] = gunshotClip != null ? gunshotClip : GenerateTone("sfx_shoot", 0.05f, 1200f, 700f, 0.3f, false);
        clipVolumes["shoot"] = gunshotClip != null ? gunshotVolume : 1f;
        clips["hit"] = GenerateTone("sfx_hit", 0.09f, 300f, 120f, 0.45f, false);
        clips["death"] = robotExplodeClip != null ? robotExplodeClip : GenerateTone("sfx_death", 0.35f, 500f, 90f, 0.5f, false);
        clips["explosion"] = grenadeExplosionClip != null ? grenadeExplosionClip : GenerateTone("sfx_explosion", 0.4f, 220f, 60f, 0.65f, true);
        clips["hiscore"] = GenerateTone("sfx_record", 0.5f, 600f, 1200f, 0.35f, false);
    }

    AudioClip GenerateTone(string name, float duration, float startFreq, float endFreq, float volume, bool noisy)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * sampleRate));
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, progress);
            float envelope = Mathf.Pow(1f - progress, 2f);
            float sample = Mathf.Sin(2f * Mathf.PI * freq * t);
            if (noisy)
            {
                sample = Mathf.Lerp(sample, Random.Range(-1f, 1f), 0.5f);
            }
            data[i] = sample * envelope * volume;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    void Update()
    {
        UpdateFootsteps();
    }

    // Footsteps play only while the rail camera is actually travelling and the game is running.
    void UpdateFootsteps()
    {
        if (footstepSource == null || footstepsClip == null) return;
        if (mover == null) mover = Object.FindAnyObjectByType<AutoMoveController>();
        bool walking = mover != null && mover.IsMoving && GameManager.IsPlaying;
        if (walking && !footstepSource.isPlaying) footstepSource.Play();
        else if (!walking && footstepSource.isPlaying) footstepSource.Stop();
    }

    public void PlaySfx(string id)
    {
        if (sfxSource == null) return;
        AudioClip clip;
        if (clips.TryGetValue(id, out clip) && clip != null)
        {
            float v;
            if (!clipVolumes.TryGetValue(id, out v)) v = 1f;
            sfxSource.PlayOneShot(clip, v * sfxVolume);
        }
    }

    // Starts the reload sound, stretched so it lasts exactly as long as the reload.
    public void StartReloadSound(float reloadDuration)
    {
        if (reloadSource == null || reloadClip == null) return;
        reloadSource.clip = reloadClip;
        reloadSource.volume = reloadVolume;
        reloadSource.pitch = reloadDuration > 0.05f ? Mathf.Clamp(reloadClip.length / reloadDuration, 0.5f, 2f) : 1f;
        reloadSource.Play();
    }

    public void StopReloadSound()
    {
        if (reloadSource != null && reloadSource.isPlaying) reloadSource.Stop();
    }
}
