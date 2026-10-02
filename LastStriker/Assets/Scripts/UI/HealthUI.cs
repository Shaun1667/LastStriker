using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Text hpText;
    public PlayerHealth health;

    void OnEnable()
    {
        if (health != null) health.OnHealthChanged += HandleHealthChanged;
    }

    void OnDisable()
    {
        if (health != null) health.OnHealthChanged -= HandleHealthChanged;
    }

    void HandleHealthChanged(int current, int max)
    {
        if (hpText != null) hpText.text = "HP " + current + " / " + max;
    }
}
