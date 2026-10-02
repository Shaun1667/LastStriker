
using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public int Score { get; private set; }
    public int HiScore { get; private set; }
    public event Action<int> OnScoreChanged;

    const string HiScoreKey = "LastStriker_HiScore";

    void Awake()
    {
        Instance = this;
        HiScore = PlayerPrefs.GetInt(HiScoreKey, 0);
    }

    public void AddScore(int amount)
    {
        Score = Mathf.Max(0, Score + amount);
        OnScoreChanged?.Invoke(Score);
    }

    public bool CheckAndSaveHiScore()
    {
        if (Score > HiScore)
        {
            HiScore = Score;
            PlayerPrefs.SetInt(HiScoreKey, HiScore);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }
}
