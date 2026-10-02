
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyCharger : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float meleeRange = 2.5f;
    public int damage = 25;
    public float attackCooldown = 1.5f;
    public float telegraphDuration = 0.5f;
    public Renderer bodyRenderer;

    Transform player;
    Color originalColor;
    float cooldownTimer;
    bool attacking;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (bodyRenderer != null) originalColor = bodyRenderer.material.color;
    }

    void Update()
    {
        if (player == null || attacking) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        if (dist > meleeRange)
        {
            Vector3 dir = toPlayer.normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            if (dir.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = attackCooldown;
                StartCoroutine(AttackRoutine());
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        attacking = true;
        if (bodyRenderer != null) bodyRenderer.material.color = new Color(1f, 0.15f, 0.15f);
        yield return new WaitForSeconds(telegraphDuration);
        if (bodyRenderer != null) bodyRenderer.material.color = originalColor;

        if (player != null)
        {
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.magnitude <= meleeRange + 0.5f)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage, false);
            }
        }
        attacking = false;
    }
}
