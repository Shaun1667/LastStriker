using UnityEngine;

public class EnemyGrenadier : EnemyBase
{
    public GameObject grenadePrefab;
    public int grenadeDamage = 40;

    protected override void Start()
    {
        telegraphColor = new Color(1f, 0.55f, 0f);
        base.Start();
    }

    protected override void PerformAttack()
    {
        if (player == null || grenadePrefab == null) return;
        GameObject g = Instantiate(grenadePrefab, transform.position + Vector3.up, Quaternion.identity);
        Grenade grenade = g.GetComponent<Grenade>();
        if (grenade != null)
        {
            grenade.damage = grenadeDamage;
            grenade.Launch(player);
        }
    }
}
