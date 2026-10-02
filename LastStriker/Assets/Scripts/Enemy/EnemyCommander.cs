
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyCommander : MonoBehaviour
{
    public float buffRadius = 8f;
    public float damageReduction = 0.2f;
    public float attackSpeedBoost = 1.3f;
    public float tickInterval = 1f;

    void Start()
    {
        StartCoroutine(BuffLoop());
    }

    IEnumerator BuffLoop()
    {
        while (true)
        {
            EnemyHealth self = GetComponent<EnemyHealth>();
            Collider[] hits = Physics.OverlapSphere(transform.position, buffRadius);
            foreach (Collider c in hits)
            {
                EnemyHealth eh = c.GetComponent<EnemyHealth>();
                if (eh != null && eh != self)
                {
                    eh.damageReductionBuff = damageReduction;
                }
                EnemyBase eb = c.GetComponent<EnemyBase>();
                if (eb != null)
                {
                    eb.attackSpeedMultiplier = attackSpeedBoost;
                }
            }
            yield return new WaitForSeconds(tickInterval);
        }
    }
}
