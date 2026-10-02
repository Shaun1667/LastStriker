
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyRepair : MonoBehaviour
{
    public float healRadius = 6f;
    public int healPerTick = 5;
    public float tickInterval = 1f;

    void Start()
    {
        StartCoroutine(HealLoop());
    }

    IEnumerator HealLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(tickInterval);
            Collider[] hits = Physics.OverlapSphere(transform.position, healRadius);
            EnemyHealth self = GetComponent<EnemyHealth>();
            foreach (Collider c in hits)
            {
                EnemyHealth eh = c.GetComponent<EnemyHealth>();
                if (eh != null && eh != self)
                {
                    eh.Heal(healPerTick);
                }
            }
        }
    }
}
