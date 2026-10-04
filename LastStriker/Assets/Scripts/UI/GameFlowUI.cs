using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

// Runtime-built overlay menus (title / pause / result). Created by GameManager, so no scene setup is needed.
public class GameFlowUI : MonoBehaviour
{
    class MenuButton
    {
        public string id;
        public GameObject panel;
        public RectTransform rect;
        public Image bg;
    }

    static readonly Color ButtonNormal = new Color(0.10f, 0.10f, 0.12f, 0.92f);
    static readonly Color ButtonHover = new Color(0.95f, 0.45f, 0.10f, 0.95f);

    RectTransform root;
    Font font;
    GameObject titlePanel, pausePanel, resultPanel;
    Text titlePrompt, resultTitle, resultScore, resultHi, resultRecord;
    readonly List<MenuButton> buttons = new List<MenuButton>();

    public static GameFlowUI Create(Canvas canvas)
    {
        GameObject go = new GameObject("GameFlowUI", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        GameFlowUI ui = go.AddComponent<GameFlowUI>();
        ui.Build();

        // Keep the crosshair above the menus so it doubles as the mouse pointer.
        CrosshairUI crosshair = FindAnyObjectByType<CrosshairUI>();
        if (crosshair != null && crosshair.transform.parent == canvas.transform)
            go.transform.SetSiblingIndex(crosshair.transform.GetSiblingIndex());
        return ui;
    }

    void Build()
    {
        root = (RectTransform)transform;
        Stretch(root);
        font = LoadFont();

        titlePanel = MakePanel("TitlePanel", 0.55f);
        Text gameTitle = MakeText(titlePanel.transform, "GameTitle", "LAST STRIKER", 150, new Color(1f, 0.55f, 0.15f), new Vector2(0f, 170f), new Vector2(1700f, 220f));
        gameTitle.fontStyle = FontStyle.Bold;
        titlePrompt = MakeText(titlePanel.transform, "Prompt", "화면을 클릭하여 시작", 60, Color.white, new Vector2(0f, -120f), new Vector2(1400f, 120f));

        pausePanel = MakePanel("PausePanel", 0.7f);
        Text paused = MakeText(pausePanel.transform, "Paused", "PAUSED", 120, Color.white, new Vector2(0f, 250f), new Vector2(1200f, 180f));
        paused.fontStyle = FontStyle.Bold;
        MakeButton(pausePanel, "resume", "계속하기", new Vector2(0f, 40f), new Vector2(560f, 120f));
        MakeButton(pausePanel, "quit", "종료하기", new Vector2(0f, -120f), new Vector2(560f, 120f));

        resultPanel = MakePanel("ResultPanel", 0.8f);
        resultTitle = MakeText(resultPanel.transform, "ResultTitle", "GAME OVER", 130, Color.white, new Vector2(0f, 300f), new Vector2(1600f, 200f));
        resultTitle.fontStyle = FontStyle.Bold;
        resultScore = MakeText(resultPanel.transform, "ResultScore", "SCORE 0", 90, Color.white, new Vector2(0f, 130f), new Vector2(1600f, 140f));
        resultHi = MakeText(resultPanel.transform, "ResultHi", "HI SCORE 0", 52, new Color(0.85f, 0.85f, 0.85f), new Vector2(0f, 30f), new Vector2(1600f, 90f));
        resultRecord = MakeText(resultPanel.transform, "ResultRecord", "NEW RECORD!", 56, new Color(1f, 0.85f, 0.2f), new Vector2(0f, -45f), new Vector2(1600f, 90f));
        MakeButton(resultPanel, "title", "타이틀로", new Vector2(-300f, -210f), new Vector2(520f, 120f));
        MakeButton(resultPanel, "quit", "게임 종료", new Vector2(300f, -210f), new Vector2(520f, 120f));
    }

    void Update()
    {
        if (titlePanel != null && titlePanel.activeSelf && titlePrompt != null)
        {
            Color c = titlePrompt.color;
            c.a = 0.4f + 0.6f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3.5f));
            titlePrompt.color = c;
        }

        Vector2 pos = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(-1f, -1f);
        for (int i = 0; i < buttons.Count; i++)
        {
            MenuButton b = buttons[i];
            if (!b.panel.activeInHierarchy) continue;
            bool hover = RectTransformUtility.RectangleContainsScreenPoint(b.rect, pos, null);
            b.bg.color = hover ? ButtonHover : ButtonNormal;
        }
    }

    // Returns the id of the visible button under the screen position, or null.
    public string HitButton(Vector2 screenPos)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            MenuButton b = buttons[i];
            if (!b.panel.activeInHierarchy) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(b.rect, screenPos, null)) return b.id;
        }
        return null;
    }

    public void HideAll()
    {
        titlePanel.SetActive(false);
        pausePanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    public void ShowTitle()
    {
        HideAll();
        titlePanel.SetActive(true);
    }

    public void ShowPause()
    {
        HideAll();
        pausePanel.SetActive(true);
    }

    public void ShowResult(string title, int score, int hiScore, bool newRecord)
    {
        HideAll();
        resultTitle.text = title;
        resultTitle.color = title.Contains("CLEAR") ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.25f, 0.2f);
        resultScore.text = "SCORE  " + score.ToString("N0");
        resultHi.text = "HI SCORE  " + hiScore.ToString("N0");
        resultRecord.gameObject.SetActive(newRecord);
        resultPanel.SetActive(true);
    }

    static Font LoadFont()
    {
        Font f = null;
        try
        {
            f = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "맑은 고딕", "Apple SD Gothic Neo", "NanumGothic", "Noto Sans CJK KR", "Arial" }, 48);
        }
        catch (System.Exception) { }
        if (f == null) f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return f;
    }

    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    GameObject MakePanel(string name, float dim)
    {
        GameObject p = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        p.transform.SetParent(root, false);
        Stretch((RectTransform)p.transform);
        Image img = p.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, dim);
        img.raycastTarget = false;
        p.SetActive(false);
        return p;
    }

    Text MakeText(Transform parent, string name, string str, int size, Color color, Vector2 pos, Vector2 box)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        Text t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        t.text = str;
        Shadow sh = go.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.8f);
        sh.effectDistance = new Vector2(3f, -3f);
        return t;
    }

    void MakeButton(GameObject panel, string id, string label, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject("Button_" + id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(panel.transform, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image bg = go.GetComponent<Image>();
        bg.color = ButtonNormal;
        bg.raycastTarget = false;

        Text t = MakeText(go.transform, "Label", label, 52, Color.white, Vector2.zero, size);
        t.fontStyle = FontStyle.Bold;

        buttons.Add(new MenuButton { id = id, panel = panel, rect = rt, bg = bg });
    }
}
