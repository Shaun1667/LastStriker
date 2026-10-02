
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource sfxSource;
    public Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    void Awake()
    {
        Instance = this;
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        GenerateClips();
    }

    void GenerateClips()
    {
        clips["shoot"] = GenerateTone("sfx_shoot", 0.05f, 1200f, 700f, 0.3f, false);
        clips["hit"] = GenerateTone("sfx_hit", 0.09f, 300f, 120f, 0.45f, false);
        clips["death"] = GenerateTone("sfx_death", 0.35f, 500f, 90f, 0.5f, false);
        clips["explosion"] = GenerateTone("sfx_explosion", 0.4f, 220f, 60f, 0.65f, true);
        clips["reload"] = GenerateTone("sfx_reload", 0.15f, 400f, 600f, 0.25f, false);
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

    public void PlaySfx(string id)
    {
        if (sfxSource == null) return;
        AudioClip clip;
        if (clips.TryGetValue(id, out clip) && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
