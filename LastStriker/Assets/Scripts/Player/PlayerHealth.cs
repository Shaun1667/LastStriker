
using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 200;
    public int CurrentHP { get; private set; }
    public PlayerShield shield;

    public event Action<int,int> OnHealthChanged;
    public event Action OnPlayerDied;

    void Awake()
    {
        CurrentHP = maxHP;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    public void TakeDamage(int amount, bool isExplosive)
    {
        if (CurrentHP <= 0) return;
        if (shield != null && shield.IsShieldActive && !isExplosive)
        {
            return;
        }
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("hit");
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
        if (CurrentHP <= 0)
        {
            OnPlayerDied?.Invoke();
        }
    }

    public void FullRestore()
    {
        CurrentHP = maxHP;
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }
}
