
using UnityEngine;
using System;

public class BossPartHealth : MonoBehaviour
{
    public int maxHP = 150;
    public int scoreValue = 200;
    public bool damageBlocked = false;
    public Behaviour attackBehavior;
    [Tooltip("Spawn a small explosion when this part is destroyed.")]
    public bool explodeOnDeplete = true;

    public int CurrentHP { get; private set; }
    public event Action<BossPartHealth> OnDepleted;

    Renderer[] rends;
    Collider col;
    TintGlow glow;

    void Awake()
    {
        CurrentHP = maxHP;
        // The visual may be the part itself (primitive) or a child model.
        rends = GetComponentsInChildren<Renderer>(true);
        col = GetComponent<Collider>();
        glow = GetComponentInChildren<TintGlow>(true);
    }

    void SetVisible(bool on)
    {
        if (rends == null) return;
        foreach (Renderer r in rends) if (r != null) r.enabled = on;
    }

    public void ResetPart()
    {
        CurrentHP = maxHP;
        SetVisible(true);
        if (col != null) col.enabled = true;
        if (attackBehavior != null) attackBehavior.enabled = true;
    }

    public void TakeDamage(int amount)
    {
        if (CurrentHP <= 0) return;
        if (damageBlocked) return;
        CurrentHP -= amount;
        if (glow != null) glow.Flash();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("hit");
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Deplete();
        }
    }

    void Deplete()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(scoreValue);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("death");
        if (explodeOnDeplete)
        {
            Vector3 at = col != null ? col.bounds.center : transform.position;
            GameObject fx = new GameObject("PartExplosionFX");
            fx.transform.position = at;
            Explosion ex = fx.AddComponent<Explosion>();
            ex.maxScale = 3.5f;
        }
        SetVisible(false);
        if (col != null) col.enabled = false;
        if (attackBehavior != null) attackBehavior.enabled = false;
        OnDepleted?.Invoke(this);
    }
}
