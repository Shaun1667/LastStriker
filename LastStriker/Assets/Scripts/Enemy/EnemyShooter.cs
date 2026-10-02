using UnityEngine;

public class EnemyShooter : EnemyBase
{
    public int damage = 15;

    protected override void Start()
    {
        telegraphColor = new Color(1f, 0.15f, 0.15f);
        base.Start();
    }

    protected override void PerformAttack()
    {
        if (player == null) return;
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamage(damage, false);
    }
}
