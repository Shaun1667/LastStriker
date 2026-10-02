
using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyShield : MonoBehaviour
{
    public float guardDuration = 3f;
    public float exposeDuration = 1f;
    public int counterDamage = 15;
    public Renderer bodyRenderer;
    public Color exposedColor = new Color(1f, 0.15f, 0.15f);

    // true = guarding (damage blocked), false = exposed.
    public event Action<bool> OnGuardChanged;

    EnemyHealth health;
    Transform player;
    Color originalColor;

    void Start()
    {
        health = GetComponent<EnemyHealth>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (bodyRenderer != null) originalColor = bodyRenderer.material.color;
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        while (true)
        {
            health.damageBlocked = true;
            if (bodyRenderer != null) bodyRenderer.material.color = originalColor;
            OnGuardChanged?.Invoke(true);
            yield return new WaitForSeconds(guardDuration);

            health.damageBlocked = false;
            if (bodyRenderer != null) bodyRenderer.material.color = exposedColor;
            OnGuardChanged?.Invoke(false);
            if (player != null)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(counterDamage, false);
            }
            yield return new WaitForSeconds(exposeDuration);
        }
    }
}
