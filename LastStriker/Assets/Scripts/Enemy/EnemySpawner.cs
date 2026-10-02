
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    public int count = 1;
    // Indices into the current area's spawn points. Empty = any point.
    public int[] spawnPointIndices;
}

public class EnemySpawner : MonoBehaviour
{
    public Transform[] spawnLanes;
    public int maxConcurrent = 2;
    public float spawnCheckInterval = 0.5f;
    public float occupiedRadius = 1.2f;

    struct PendingSpawn
    {
        public GameObject prefab;
        public int[] indices;
    }

    List<EnemyHealth> alive = new List<EnemyHealth>();
    Queue<PendingSpawn> pending = new Queue<PendingSpawn>();
    public event Action OnAllCleared;
    bool spawning;
    int activeMaxConcurrent;

    public void StartWave(EnemySpawnEntry[] entries)
    {
        StartWave(entries, 0);
    }

    public void StartWave(EnemySpawnEntry[] entries, int maxConcurrentOverride)
    {
        pending.Clear();
        alive.Clear();
        activeMaxConcurrent = maxConcurrentOverride > 0 ? maxConcurrentOverride : maxConcurrent;
        foreach (EnemySpawnEntry entry in entries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                PendingSpawn p = new PendingSpawn();
                p.prefab = entry.prefab;
                p.indices = entry.spawnPointIndices;
                pending.Enqueue(p);
            }
        }
        if (!spawning)
        {
            spawning = true;
            StartCoroutine(SpawnLoop());
        }
    }

    IEnumerator SpawnLoop()
    {
        while (pending.Count > 0 || alive.Count > 0)
        {
            alive.RemoveAll(e => e == null);

            if (pending.Count > 0 && alive.Count < activeMaxConcurrent)
            {
                PendingSpawn next = pending.Peek();
                Transform lane = PickLane(next.indices);
                if (lane != null)
                {
                    pending.Dequeue();
                    GameObject instance = Instantiate(next.prefab, lane.position, lane.rotation);
                    EnemyHealth health = instance.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        alive.Add(health);
                        health.OnDied += HandleEnemyDied;
                    }
                }
            }

            yield return new WaitForSeconds(spawnCheckInterval);
        }

        spawning = false;
        OnAllCleared?.Invoke();
    }

    Transform PickLane(int[] indices)
    {
        if (spawnLanes == null || spawnLanes.Length == 0) return null;

        List<Transform> candidates = new List<Transform>();
        if (indices != null && indices.Length > 0)
        {
            foreach (int idx in indices)
            {
                if (idx >= 0 && idx < spawnLanes.Length && spawnLanes[idx] != null) candidates.Add(spawnLanes[idx]);
            }
        }
        if (candidates.Count == 0)
        {
            foreach (Transform t in spawnLanes) if (t != null) candidates.Add(t);
        }

        List<Transform> free = new List<Transform>();
        foreach (Transform t in candidates)
        {
            if (!IsOccupied(t.position)) free.Add(t);
        }

        // If every allowed point is occupied, wait instead of stacking enemies on top of each other.
        if (free.Count == 0) return null;
        return free[UnityEngine.Random.Range(0, free.Count)];
    }

    bool IsOccupied(Vector3 pos)
    {
        foreach (EnemyHealth e in alive)
        {
            if (e == null) continue;
            if (Vector3.Distance(e.transform.position, pos) < occupiedRadius) return true;
        }
        return false;
    }

    void HandleEnemyDied(EnemyHealth e)
    {
        alive.Remove(e);
    }
}
