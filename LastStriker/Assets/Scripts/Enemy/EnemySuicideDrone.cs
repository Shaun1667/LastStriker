
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))]
public class EnemySuicideDrone : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float explodeRange = 2f;
    public int damage = 45;
    public float warningDuration = 0.8f;
    public Renderer bodyRenderer;
    public GameObject explosionPrefab;

    Transform player;
    Color originalColor;
    bool warning;
    EnemyHealth health;

    void Start()
    {
        health = GetComponent<EnemyHealth>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (bodyRenderer != null) originalColor = bodyRenderer.material.color;
    }

    void Update()
    {
        if (player == null || health.CurrentHP <= 0) return;

        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist > explodeRange)
        {
            Vector3 dir = toPlayer.normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            if (warning)
            {
                warning = false;
                if (bodyRenderer != null) bodyRenderer.material.color = originalColor;
            }
        }
        else if (!warning)
        {
            warning = true;
            StartCoroutine(ExplodeSequence());
        }
    }

    IEnumerator ExplodeSequence()
    {
        if (bodyRenderer != null) bodyRenderer.material.color = new Color(1f, 0.55f, 0f);
        yield return new WaitForSeconds(warningDuration);

        if (health.CurrentHP <= 0) yield break;

        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage, true);
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
