
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class WaveDefinition
{
    public string waveName;
    public EnemySpawnEntry[] entries;
    // 0 = use the spawner's default.
    public int maxConcurrent = 0;
}

public class WaveManager : MonoBehaviour
{
    public EnemySpawner spawner;
    public List<WaveDefinition> waves = new List<WaveDefinition>();
    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCleared;
    public event Action OnAllWavesCleared;

    int currentIndex = -1;
    public int CurrentIndex { get { return currentIndex; } }
    public int WaveCount { get { return waves.Count; } }

    void OnEnable()
    {
        if (spawner != null) spawner.OnAllCleared += HandleSpawnerCleared;
    }

    void OnDisable()
    {
        if (spawner != null) spawner.OnAllCleared -= HandleSpawnerCleared;
    }

    public void StartWave(int index)
    {
        if (index < 0 || index >= waves.Count) return;
        currentIndex = index;
        OnWaveStarted?.Invoke(index);
        spawner.StartWave(waves[index].entries, waves[index].maxConcurrent);
    }

    public void StartNextWave()
    {
        if (currentIndex + 1 >= waves.Count)
        {
            OnAllWavesCleared?.Invoke();
            return;
        }
        StartWave(currentIndex + 1);
    }

    void HandleSpawnerCleared()
    {
        OnWaveCleared?.Invoke(currentIndex);
        if (currentIndex >= waves.Count - 1) OnAllWavesCleared?.Invoke();
    }
}
