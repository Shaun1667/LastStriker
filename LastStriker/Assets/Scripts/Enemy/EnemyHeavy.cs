
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHeavy : MonoBehaviour
{
    public float moveSpeed = 1.2f;
    public float preferredRange = 8f;
    public int normalDamage = 15;
    public int explosiveDamage = 40;
    public float minAttackInterval = 3f;
    public float maxAttackInterval = 5f;
    public float telegraphDuration = 0.7f;
    public Renderer bodyRenderer;
    public GameObject bombPrefab;

    Transform player;
    Color originalColor;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (bodyRenderer != null) originalColor = bodyRenderer.material.color;
        StartCoroutine(AttackLoop());
    }

    void Update()
    {
        if (player == null) return;
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;
        if (dist > preferredRange)
        {
            Vector3 dir = toPlayer.normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            if (dir.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    IEnumerator AttackLoop()
    {
        while (true)
        {
            float wait = Random.Range(minAttackInterval, maxAttackInterval);
            yield return new WaitForSeconds(wait);

            bool explosive = Random.value > 0.5f;
            Color telegraph = explosive ? new Color(1f, 0.55f, 0f) : new Color(1f, 0.15f, 0.15f);
            if (bodyRenderer != null) bodyRenderer.material.color = telegraph;
            yield return new WaitForSeconds(telegraphDuration);
            if (bodyRenderer != null) bodyRenderer.material.color = originalColor;

            if (player == null) continue;

            if (explosive)
            {
                if (bombPrefab != null)
                {
                    GameObject g = Instantiate(bombPrefab, transform.position + Vector3.up, Quaternion.identity);
                    Grenade bomb = g.GetComponent<Grenade>();
                    if (bomb != null)
                    {
                        bomb.damage = explosiveDamage;
                        bomb.Launch(player);
                    }
                }
            }
            else
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(normalDamage, false);
            }
        }
    }
}
