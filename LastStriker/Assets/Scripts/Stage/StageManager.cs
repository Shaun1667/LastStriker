
using UnityEngine;
using System;

[Serializable]
public class StageArea
{
    public string areaName;
    // Camera waypoints travelled to reach this area's stop (last point = the stop).
    public Transform[] cameraPath;
    // Children of this transform are the area's spawn points (child order = spawn index).
    public Transform spawnGroup;
}

// Flow: travel to area -> on arrival spawn its wave -> on clear travel to next area -> ... -> boss.
public class StageManager : MonoBehaviour
{
    public WaveManager waveManager;
    public GameManager gameManager;
    public EnemySpawner spawner;
    public AutoMoveController cameraMover;
    public StageArea[] areas;
    public Transform[] bossPath;
    public BossController boss;

    void Start()
    {
        if (waveManager != null) waveManager.OnWaveCleared += HandleWaveCleared;
        GoToArea(0);
    }

    void GoToArea(int index)
    {
        StageArea area = areas[index];
        if (cameraMover != null && area.cameraPath != null && area.cameraPath.Length > 0)
        {
            cameraMover.MoveAlongPath(area.cameraPath, () => BeginAreaWave(index));
        }
        else
        {
            BeginAreaWave(index);
        }
    }

    void BeginAreaWave(int index)
    {
        StageArea area = areas[index];
        if (spawner != null && area.spawnGroup != null)
        {
            Transform[] points = new Transform[area.spawnGroup.childCount];
            for (int i = 0; i < points.Length; i++) points[i] = area.spawnGroup.GetChild(i);
            spawner.spawnLanes = points;
        }
        if (gameManager != null) gameManager.SetStatusMessage(area.areaName);
        waveManager.StartWave(index);
    }

    void HandleWaveCleared(int index)
    {
        int next = index + 1;
        if (next < areas.Length && next < waveManager.WaveCount)
        {
            if (gameManager != null) gameManager.SetStatusMessage("AREA CLEAR - MOVE");
            GoToArea(next);
        }
        else
        {
            GoToBoss();
        }
    }

    void GoToBoss()
    {
        if (gameManager != null) gameManager.SetStatusMessage("BOSS INCOMING...");
        if (cameraMover != null && bossPath != null && bossPath.Length > 0)
        {
            cameraMover.MoveAlongPath(bossPath, ActivateBoss);
        }
        else
        {
            ActivateBoss();
        }
    }

    void ActivateBoss()
    {
        if (boss != null)
        {
            boss.OnBossDefeated += HandleBossDefeated;
            boss.Activate();
        }
    }

    void HandleBossDefeated()
    {
        if (gameManager != null) gameManager.ShowFinalResult("STAGE CLEAR!");
    }
}
