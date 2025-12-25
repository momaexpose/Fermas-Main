using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button startGameButton;
    public Button continueButton;
    public Button optionsButton;
    public Button quitButton;

    [Header("Title")]
    public string gameTitle = "DRUG BOAT";
    public string subtitle = "A Survival Trading Game";

    private GameObject mainPanel;

    void Start()
    {
        CreateUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ensure IntroManager exists
        if (IntroManager.Instance == null)
        {
            GameObject introObj = new GameObject("IntroManager");
            introObj.AddComponent<IntroManager>();
        }

        // Ensure SceneTransitionManager exists
        if (SceneTransitionManager.Instance == null)
        {
            GameObject transitionObj = new GameObject("SceneTransitionManager");
            transitionObj.AddComponent<SceneTransitionManager>();
        }
    }

    void Update()
    {
        // Quick start with Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnStartGame();
        }

        // Quit with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnQuit();
        }
    }

    void CreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MainMenuCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Dark background
        mainPanel = new GameObject("MainMenuPanel");
        mainPanel.transform.SetParent(canvas.transform, false);

        Image bg = mainPanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.08f, 1f);

        RectTransform panelRect = mainPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Title
        CreateText(mainPanel.transform, gameTitle, 72, new Vector2(0, 150), Color.white, FontStyle.Bold);

        // Subtitle
        CreateText(mainPanel.transform, subtitle, 24, new Vector2(0, 80), new Color(0.6f, 0.6f, 0.6f), FontStyle.Italic);

        // Buttons
        startGameButton = CreateMenuButton(mainPanel.transform, "START NEW GAME", new Vector2(0, -20));
        startGameButton.onClick.AddListener(OnStartGame);

        continueButton = CreateMenuButton(mainPanel.transform, "CONTINUE", new Vector2(0, -90));
        continueButton.onClick.AddListener(OnContinue);
        continueButton.interactable = false; // Disabled until save system exists

        optionsButton = CreateMenuButton(mainPanel.transform, "OPTIONS", new Vector2(0, -160));
        optionsButton.onClick.AddListener(OnOptions);
        optionsButton.interactable = false; // Disabled until options menu exists

        quitButton = CreateMenuButton(mainPanel.transform, "QUIT", new Vector2(0, -230));
        quitButton.onClick.AddListener(OnQuit);

        // Controls hint
        CreateText(mainPanel.transform, "Press ENTER to start | ESC to quit", 16, new Vector2(0, -300), new Color(0.4f, 0.4f, 0.4f), FontStyle.Normal);

        // Version
        CreateText(mainPanel.transform, "v0.1 - Development Build", 14, new Vector2(0, -350), new Color(0.3f, 0.3f, 0.3f), FontStyle.Normal);
    }

    Text CreateText(Transform parent, string text, int fontSize, Vector2 position, Color color, FontStyle style)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);

        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = fontSize;
        t.color = color;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(800, 80);

        return t;
    }

    Button CreateMenuButton(Transform parent, string label, Vector2 position)
    {
        GameObject obj = new GameObject(label + "Button");
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        // Button colors
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.35f, 1f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.2f, 1f);
        colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        btn.colors = colors;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(350, 55);
        rect.anchoredPosition = position;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = label;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = 22;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return btn;
    }

    void OnStartGame()
    {
        Debug.Log("[MainMenu] Starting new game...");

        if (IntroManager.Instance != null)
        {
            IntroManager.Instance.StartNewGame();
        }
        else
        {
            Debug.LogError("[MainMenu] IntroManager not found!");
        }
    }

    void OnContinue()
    {
        Debug.Log("[MainMenu] Continue - Not implemented yet");
    }

    void OnOptions()
    {
        Debug.Log("[MainMenu] Options - Not implemented yet");
    }

    void OnQuit()
    {
        Debug.Log("[MainMenu] Quitting...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}