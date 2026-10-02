
using UnityEngine;

public class EnemySniper : EnemyBase
{
    public int damage = 35;

    protected override void Start()
    {
        telegraphColor = new Color(1f, 0.15f, 0.15f);
        telegraphDuration = 1.4f;
        base.Start();
    }

    protected override void PerformAttack()
    {
        if (player == null) return;
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamage(damage, false);
    }
}
