
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerGun : MonoBehaviour
{
    public int magazineSize = 30;
    public int damage = 20;
    public float fireInterval = 0.1f;
    public float range = 100f;

    [Header("Reload")]
    [Tooltip("Seconds the shield (Space) must be held before the magazine is refilled. Lower = faster reload.")]
    [Min(0.1f)]
    public float reloadTime = 2f;

    public PlayerAim aim;
    public PlayerShield shield;
    public RailCameraController cameraController;

    public int CurrentAmmo { get; private set; }
    public event Action<int,int> OnAmmoChanged;
    public event Action OnFired;

    float fireTimer;

    void Awake()
    {
        CurrentAmmo = magazineSize;
    }

    void Start()
    {
        OnAmmoChanged?.Invoke(CurrentAmmo, magazineSize);
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;
        bool wantsFire = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool canFire = shield == null || !shield.IsShieldActive;

        if (wantsFire && canFire && CurrentAmmo > 0 && fireTimer <= 0f)
        {
            fireTimer = fireInterval;
            Fire();
        }
    }

    void Fire()
    {
        CurrentAmmo--;
        OnAmmoChanged?.Invoke(CurrentAmmo, magazineSize);
        OnFired?.Invoke();
        if (cameraController != null) cameraController.Shake(0.05f, 0.03f);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("shoot");

        if (aim == null) return;
        Ray ray = aim.GetAimRay();
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            Grenade grenade = hit.collider.GetComponent<Grenade>();
            if (grenade != null)
            {
                grenade.Shoot();
                return;
            }

            BossPartHitbox bossPart = hit.collider.GetComponent<BossPartHitbox>();
            if (bossPart != null && bossPart.part != null)
            {
                bossPart.part.TakeDamage(damage);
                return;
            }

            EnemyHeadHitbox head = hit.collider.GetComponent<EnemyHeadHitbox>();
            if (head != null && head.enemy != null)
            {
                head.enemy.TakeDamageAdvanced(damage, true, false);
                return;
            }

            EnemyWeakPointHitbox weak = hit.collider.GetComponent<EnemyWeakPointHitbox>();
            if (weak != null && weak.enemy != null)
            {
                weak.enemy.TakeDamageAdvanced(damage, false, true);
                return;
            }

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamageAdvanced(damage, false, false);
            }
        }
    }

    public void Reload()
    {
        CurrentAmmo = magazineSize;
        OnAmmoChanged?.Invoke(CurrentAmmo, magazineSize);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("reload");
    }
}
