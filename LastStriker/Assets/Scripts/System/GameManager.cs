
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerHealth playerHealth;
    public EnemySpawner spawner;
    public Text statusText;

    public int continueScorePenalty = 1000;
    public float continueCountdown = 5f;

    bool isGameOver;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (playerHealth != null) playerHealth.OnPlayerDied += HandlePlayerDied;
    }

    public void SetStatusMessage(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    public void ShowFinalResult(string baseMessage)
    {
        bool isRecord = false;
        int hiScore = 0;
        if (ScoreManager.Instance != null)
        {
            isRecord = ScoreManager.Instance.CheckAndSaveHiScore();
            hiScore = ScoreManager.Instance.HiScore;
        }

        string msg = baseMessage;
        if (isRecord)
        {
            msg += "  NEW RECORD!";
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("hiscore");
        }
        msg = msg + "  (HI " + hiScore.ToString("N0") + ")";
        SetStatusMessage(msg);
    }

    void HandlePlayerDied()
    {
        if (isGameOver) return;
        isGameOver = true;
        StartCoroutine(ContinueSequence());
    }

    IEnumerator ContinueSequence()
    {
        Time.timeScale = 0f;
        float remaining = continueCountdown;
        bool clicked = false;

        while (remaining > 0f)
        {
            SetStatusMessage("GAME OVER - Continue? (" + Mathf.CeilToInt(remaining) + ")");
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                clicked = true;
                break;
            }
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;

        if (clicked)
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(-continueScorePenalty);
            if (playerHealth != null) playerHealth.FullRestore();
            SetStatusMessage("");
            isGameOver = false;
        }
        else
        {
            ShowFinalResult("GAME OVER");
        }
    }
}
