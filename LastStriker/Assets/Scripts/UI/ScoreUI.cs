
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    public Text scoreText;
    public ScoreManager scoreManager;

    void OnEnable()
    {
        if (scoreManager != null) scoreManager.OnScoreChanged += HandleScoreChanged;
    }

    void OnDisable()
    {
        if (scoreManager != null) scoreManager.OnScoreChanged -= HandleScoreChanged;
    }

    void Start()
    {
        if (scoreManager != null) HandleScoreChanged(scoreManager.Score);
    }

    void HandleScoreChanged(int score)
    {
        if (scoreText == null) return;
        int hi = scoreManager != null ? scoreManager.HiScore : 0;
        scoreText.text = "SCORE " + score.ToString("N0") + "   HI " + hi.ToString("N0");
    }
}
