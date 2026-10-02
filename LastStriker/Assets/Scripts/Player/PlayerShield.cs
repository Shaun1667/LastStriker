using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerShield : MonoBehaviour
{
    public bool IsShieldActive { get; private set; }
    [Tooltip("Used only when no PlayerGun is assigned. The reload time is set on PlayerGun > Reload Time.")]
    [HideInInspector]
    public float reloadHoldDuration = 2f;
    public PlayerGun gun;

    public event Action<bool> OnShieldStateChanged;

    // 0..1 while the shield is held and the reload has not completed yet; 0 otherwise.
    public float ReloadProgress
    {
        get
        {
            if (!IsShieldActive || reloadTriggeredThisHold) return 0f;
            return Mathf.Clamp01(holdTimer / Mathf.Max(0.01f, ReloadTime));
        }
    }

    float ReloadTime => gun != null ? gun.reloadTime : reloadHoldDuration;

    float holdTimer;
    bool reloadTriggeredThisHold;

    void Update()
    {
        bool held = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        if (held && !IsShieldActive)
        {
            IsShieldActive = true;
            holdTimer = 0f;
            reloadTriggeredThisHold = false;
            OnShieldStateChanged?.Invoke(true);
        }
        else if (!held && IsShieldActive)
        {
            IsShieldActive = false;
            OnShieldStateChanged?.Invoke(false);
        }

        if (IsShieldActive)
        {
            holdTimer += Time.deltaTime;
            if (!reloadTriggeredThisHold && holdTimer >= ReloadTime)
            {
                reloadTriggeredThisHold = true;
                if (gun != null) gun.Reload();
            }
        }
    }
}
