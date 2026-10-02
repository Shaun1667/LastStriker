
using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 60;
    public int scoreValue = 100;
    public bool canHeadshot = true;
    public bool hasArmor = false;
    public float armorDamageMultiplier = 0.5f;
    public GameObject deathEffectPrefab;

    public bool damageBlocked = false;
    public float damageReductionBuff = 0f;

    public int CurrentHP { get; private set; }
    public event Action<EnemyHealth> OnDied;
    // Fired when the enemy takes damage but survives (used for hit reactions).
    public event Action<EnemyHealth, int> OnDamaged;

    void Awake()
    {
        CurrentHP = maxHP;
    }

    public void TakeDamage(int amount, bool isHeadshot)
    {
        TakeDamageAdvanced(amount, isHeadshot, false);
    }

    public void TakeDamageAdvanced(int amount, bool isHeadshot, bool isWeakPoint)
    {
        if (CurrentHP <= 0) return;
        if (damageBlocked) return;

        int finalAmount = amount;
        bool headshotApplied = canHeadshot && isHeadshot;
        if (headshotApplied)
        {
            finalAmount *= 2;
        }
        else if (hasArmor && !isWeakPoint)
        {
            finalAmount = Mathf.RoundToInt(finalAmount * armorDamageMultiplier);
        }

        if (damageReductionBuff > 0f)
        {
            finalAmount = Mathf.RoundToInt(finalAmount * (1f - damageReductionBuff));
        }

        if (finalAmount < 0) finalAmount = 0;

        CurrentHP -= finalAmount;
        if (CurrentHP <= 0)
        {
            Die(headshotApplied);
        }
        else if (finalAmount > 0)
        {
            OnDamaged?.Invoke(this, finalAmount);
        }
    }

    public void Heal(int amount)
    {
        if (CurrentHP <= 0) return;
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
    }

    void Die(bool wasHeadshot)
    {
        int score = scoreValue;
        if (wasHeadshot) score += 300;
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(score);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("death");

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        OnDied?.Invoke(this);
        Destroy(gameObject);
    }
}
