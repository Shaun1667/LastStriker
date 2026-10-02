using UnityEngine;
using UnityEngine.UI;

public class ShieldUI : MonoBehaviour
{
    public GameObject shieldIcon;
    public PlayerShield shield;

    void OnEnable()
    {
        if (shield != null) shield.OnShieldStateChanged += HandleShieldChanged;
    }

    void OnDisable()
    {
        if (shield != null) shield.OnShieldStateChanged -= HandleShieldChanged;
    }

    void HandleShieldChanged(bool active)
    {
        if (shieldIcon != null) shieldIcon.SetActive(active);
    }
}
