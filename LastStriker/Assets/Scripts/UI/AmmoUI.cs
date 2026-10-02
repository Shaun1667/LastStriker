using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    public Text ammoText;
    public PlayerGun gun;
    [Tooltip("Optional: shows reload progress while the shield is held.")]
    public PlayerShield shield;

    int current;
    int max;
    bool wasReloading;

    void OnEnable()
    {
        if (gun != null) gun.OnAmmoChanged += HandleAmmoChanged;
    }

    void OnDisable()
    {
        if (gun != null) gun.OnAmmoChanged -= HandleAmmoChanged;
    }

    void HandleAmmoChanged(int c, int m)
    {
        current = c;
        max = m;
        Refresh();
    }

    void Update()
    {
        bool reloading = shield != null && shield.ReloadProgress > 0f;
        if (reloading || wasReloading) Refresh();
        wasReloading = reloading;
    }

    void Refresh()
    {
        if (ammoText == null) return;
        float p = shield != null ? shield.ReloadProgress : 0f;
        if (p > 0f && current < max)
            ammoText.text = "RELOADING " + Mathf.RoundToInt(p * 100f) + "%";
        else
            ammoText.text = "AMMO " + current + " / " + max;
    }
}
