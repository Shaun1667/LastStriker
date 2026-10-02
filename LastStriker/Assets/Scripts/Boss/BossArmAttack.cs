
using UnityEngine;
using System.Collections;

public class BossArmAttack : MonoBehaviour
{
    public float minInterval = 2f;
    public float maxInterval = 3.5f;
    public float telegraphDuration = 0.6f;
    public int damage = 15;
    public bool unblockable = false;
    public Renderer bodyRenderer;
    public Color telegraphColor = new Color(1f, 0.15f, 0.15f);

    Color originalColor;
    Coroutine loop;

    void OnEnable()
    {
        if (bodyRenderer != null) originalColor = bodyRenderer.material.color;
        loop = StartCoroutine(AttackLoop());
    }

    void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
        if (bodyRenderer != null) bodyRenderer.material.color = originalColor;
    }

    IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            if (bodyRenderer != null) bodyRenderer.material.color = telegraphColor;
            yield return new WaitForSeconds(telegraphDuration);
            if (bodyRenderer != null) bodyRenderer.material.color = originalColor;

            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                PlayerHealth ph = p.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage, unblockable);
            }
        }
    }
}
