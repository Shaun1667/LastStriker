using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public enum GameState { Title, Playing, Paused, Continue, Ending, Result }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Defaults to Playing so scenes without a GameManager keep working; Awake switches it to Title.
    public static GameState State { get; private set; } = GameState.Playing;
    public static bool IsPlaying { get { return State == GameState.Playing; } }

    // False on title / pause / result screens, and while the click that closed a menu is still held.
    public static bool CanFire
    {
        get
        {
            if (State != GameState.Playing) return false;
            if (fireLatch && Mouse.current != null && Mouse.current.leftButton.isPressed) return false;
            return true;
        }
    }

    public PlayerHealth playerHealth;
    public EnemySpawner spawner;
    public Text statusText;

    public int continueScorePenalty = 1000;
    public float continueCountdown = 5f;
    [Tooltip("Seconds to wait after the boss dies before the result window appears.")]
    public float stageClearResultDelay = 2.5f;

    static bool fireLatch;
    bool isGameOver;
    GameFlowUI ui;
    Coroutine resultRoutine;

    void Awake()
    {
        Instance = this;
        State = GameState.Title;
        fireLatch = false;
        Time.timeScale = 0f;
        AudioListener.pause = false;

        Canvas canvas = statusText != null ? statusText.canvas : FindAnyObjectByType<Canvas>();
        if (canvas != null) ui = GameFlowUI.Create(canvas.rootCanvas);
    }

    void Start()
    {
        if (playerHealth != null) playerHealth.OnPlayerDied += HandlePlayerDied;
        if (ui != null) ui.ShowTitle();
        else StartGame();
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnPlayerDied -= HandlePlayerDied;
        if (Instance == this)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;
        bool click = mouse != null && mouse.leftButton.wasPressedThisFrame;
        bool esc = keyboard != null && keyboard.escapeKey.wasPressedThisFrame;

        if (fireLatch && (mouse == null || !mouse.leftButton.isPressed)) fireLatch = false;

        switch (State)
        {
            case GameState.Title:
                if (click) StartGame();
                break;

            case GameState.Playing:
                if (esc) Pause();
                break;

            case GameState.Paused:
                if (esc)
                {
                    Resume();
                }
                else if (click && ui != null)
                {
                    string id = ui.HitButton(mouse.position.ReadValue());
                    if (id == "resume") Resume();
                    else if (id == "quit") QuitGame();
                }
                break;

            case GameState.Result:
                if (click && ui != null)
                {
                    string id = ui.HitButton(mouse.position.ReadValue());
                    if (id == "title") ReturnToTitle();
                    else if (id == "quit") QuitGame();
                }
                break;
        }
    }

    void StartGame()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        fireLatch = true; // the start click must not fire a shot
        if (ui != null) ui.HideAll();
    }

    void Pause()
    {
        State = GameState.Paused;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (ui != null) ui.ShowPause();
    }

    void Resume()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        fireLatch = true;
        if (ui != null) ui.HideAll();
    }

    void ReturnToTitle()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void QuitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetStatusMessage(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    public void ShowFinalResult(string baseMessage, float delay = 0f)
    {
        if (State == GameState.Result || resultRoutine != null) return;
        resultRoutine = StartCoroutine(FinalResultRoutine(baseMessage, delay));
    }

    IEnumerator FinalResultRoutine(string baseMessage, float delay)
    {
        if (State == GameState.Paused)
        {
            // Finished on the same frame the pause menu opened: let time run so the delay can elapse.
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (ui != null) ui.HideAll();
        }
        State = GameState.Ending;

        if (delay > 0f) yield return new WaitForSeconds(delay);

        bool isRecord = false;
        int score = 0;
        int hiScore = 0;
        if (ScoreManager.Instance != null)
        {
            score = ScoreManager.Instance.Score;
            isRecord = ScoreManager.Instance.CheckAndSaveHiScore();
            hiScore = ScoreManager.Instance.HiScore;
        }
        if (isRecord && AudioManager.Instance != null) AudioManager.Instance.PlaySfx("hiscore");

        SetStatusMessage("");
        State = GameState.Result;
        Time.timeScale = 0f;
        if (ui != null) ui.ShowResult(baseMessage, score, hiScore, isRecord);
        resultRoutine = null;
    }

    void HandlePlayerDied()
    {
        if (isGameOver || State != GameState.Playing) return;
        isGameOver = true;
        StartCoroutine(ContinueSequence());
    }

    IEnumerator ContinueSequence()
    {
        State = GameState.Continue;
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

        if (clicked)
        {
            Time.timeScale = 1f;
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(-continueScorePenalty);
            if (playerHealth != null) playerHealth.FullRestore();
            SetStatusMessage("");
            fireLatch = true;
            State = GameState.Playing;
            isGameOver = false;
        }
        else
        {
            ShowFinalResult("GAME OVER");
        }
    }
}
