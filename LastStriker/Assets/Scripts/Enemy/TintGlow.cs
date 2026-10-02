using UnityEngine;

// Watches the tint that gameplay scripts put on a renderer (material.color for attack warnings)
// and adds a matching emissive glow so the warning reads clearly on textured models.
// Also provides a short hit flash and a "wrecked" darkening.
public class TintGlow : MonoBehaviour
{
    public Renderer source;
    public Renderer[] targets;
    public float glowIntensity = 2.5f;
    public float tintThreshold = 0.08f;
    public Color flashColor = Color.white;
    public float flashIntensity = 1.2f;
    public float flashDuration = 0.07f;

    public bool IsTinted { get; private set; }

    Color baseColor = Color.white;
    bool baseCaptured;
    float flashUntil;
    bool dimmed;
    Color dimColor = Color.gray;
    MaterialPropertyBlock mpb;

    static readonly int EmissiveFactorId = Shader.PropertyToID("emissiveFactor");
    static readonly int EmissiveTexId = Shader.PropertyToID("emissiveTexture");
    static readonly int BaseColorId = Shader.PropertyToID("baseColorFactor");

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        if (targets == null || targets.Length == 0) targets = GetComponentsInChildren<Renderer>();
    }

    public void Flash()
    {
        flashUntil = Time.time + flashDuration;
    }

    public void SetWrecked(Color c)
    {
        dimmed = true;
        dimColor = c;
        source = null;
        IsTinted = false;
    }

    void LateUpdate()
    {
        Color glow = Color.black;
        if (source != null && source.sharedMaterial != null && source.sharedMaterial.HasProperty(BaseColorId))
        {
            Color c = source.sharedMaterial.GetColor(BaseColorId);
            if (!baseCaptured) { baseColor = c; baseCaptured = true; }
            float diff = Mathf.Abs(c.r - baseColor.r) + Mathf.Abs(c.g - baseColor.g) + Mathf.Abs(c.b - baseColor.b);
            IsTinted = diff > tintThreshold;
            if (IsTinted) glow = c * glowIntensity;
        }
        if (Time.time < flashUntil) glow = flashColor * flashIntensity;

        foreach (Renderer r in targets)
        {
            if (r == null) continue;
            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissiveFactorId, glow);
            mpb.SetTexture(EmissiveTexId, Texture2D.whiteTexture);
            if (dimmed) mpb.SetColor(BaseColorId, dimColor);
            r.SetPropertyBlock(mpb);
        }
    }
}
