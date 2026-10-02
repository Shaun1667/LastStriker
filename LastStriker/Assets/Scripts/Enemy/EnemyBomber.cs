
using UnityEngine;

public class EnemyBomber : EnemyBase
{
    public GameObject bombPrefab;
    public int bombDamage = 30;

    protected override void Start()
    {
        telegraphColor = new Color(1f, 0.55f, 0f);
        base.Start();
    }

    protected override void PerformAttack()
    {
        if (player == null || bombPrefab == null) return;
        GameObject g = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Grenade bomb = g.GetComponent<Grenade>();
        if (bomb != null)
        {
            bomb.damage = bombDamage;
            bomb.Launch(player);
        }
    }
}
