
using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))]
public abstract class EnemyBase : MonoBehaviour
{
    public float minAttackInterval = 2.5f;
    public float maxAttackInterval = 4f;
    public float telegraphDuration = 0.6f;
    public Color telegraphColor = Color.red;
    public Renderer bodyRenderer;
    public float attackSpeedMultiplier = 1f;

    // Fired when the attack warning starts (argument: warning duration) and when the attack lands.
    public event Action<float> OnTelegraph;
    public event Action OnAttack;

    protected Transform player;
    Color originalColor;
    Coroutine loop;

    protected virtual void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (bodyRenderer != null) originalColor = bodyRenderer.material.color;
        loop = StartCoroutine(AttackLoop());
    }

    void OnDestroy()
    {
        if (loop != null) StopCoroutine(loop);
    }

    IEnumerator AttackLoop()
    {
        while (true)
        {
            float wait = UnityEngine.Random.Range(minAttackInterval, maxAttackInterval) / Mathf.Max(0.01f, attackSpeedMultiplier);
            yield return new WaitForSeconds(wait);

            OnTelegraph?.Invoke(telegraphDuration);
            if (bodyRenderer != null) bodyRenderer.material.color = telegraphColor;
            yield return new WaitForSeconds(telegraphDuration);
            if (bodyRenderer != null) bodyRenderer.material.color = originalColor;

            OnAttack?.Invoke();
            PerformAttack();
        }
    }

    protected abstract void PerformAttack();
}
