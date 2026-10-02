
using UnityEngine;
using System.Collections;

public class BossSummoner : MonoBehaviour
{
    public GameObject[] summonPrefabs;
    public Transform[] summonPoints;
    public float minInterval = 8f;
    public float maxInterval = 12f;

    Coroutine loop;

    void OnEnable()
    {
        loop = StartCoroutine(SummonLoop());
    }

    void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
    }

    IEnumerator SummonLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            if (summonPrefabs == null || summonPrefabs.Length == 0) continue;
            if (summonPoints == null || summonPoints.Length == 0) continue;
            GameObject prefab = summonPrefabs[Random.Range(0, summonPrefabs.Length)];
            Transform point = summonPoints[Random.Range(0, summonPoints.Length)];
            Instantiate(prefab, point.position, point.rotation);
        }
    }
}
