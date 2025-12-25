using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI that shows hand icon and "E to open" when looking at door.
/// Auto-creates UI if not set up manually.
/// </summary>
public class DoorInteractionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject uiPanel;
    public Image handIcon;
    public Text promptText;

    [Header("Settings")]
    public string promptMessage = "E to Open";
    public Color iconColor = Color.white;
    public float iconSize = 60f;
    public float fontSize = 24f;

    [Header("Animation")]
    public float fadeSpeed = 10f;

    private CanvasGroup canvasGroup;
    private bool shouldShow = false;

    void Start()
    {
        // Create UI if not assigned
        if (uiPanel == null)
        {
            CreateUI();
        }

        // Start hidden
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
    }

    void CreateUI()
    {
        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("InteractionCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create panel
        uiPanel = new GameObject("DoorInteractionUI");
        uiPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = uiPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, -50); // Slightly below center
        panelRect.sizeDelta = new Vector2(200, 100);

        canvasGroup = uiPanel.AddComponent<CanvasGroup>();

        // Create vertical layout
        VerticalLayoutGroup layout = uiPanel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 10;
        layout.childControlWidth = false;
        layout.childControlHeight = false;





      

        // Create text
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(uiPanel.transform, false);

        promptText = textObj.AddComponent<Text>();
        promptText.text = promptMessage;
        promptText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        promptText.fontSize = (int)fontSize;
        promptText.color = iconColor;
        promptText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(200, 40);

        // Add outline for visibility
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
    }

    
   

    void Update()
    {
        if (canvasGroup == null) return;

        // Smooth fade
        float targetAlpha = shouldShow ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Show the interaction UI
    /// </summary>
    public void Show()
    {
        shouldShow = true;
    }

    /// <summary>
    /// Hide the interaction UI
    /// </summary>
    public void Hide()
    {
        shouldShow = false;
    }

    /// <summary>
    /// Check if UI is currently visible
    /// </summary>
    public bool IsVisible()
    {
        return shouldShow;
    }

    /// <summary>
    /// Set custom prompt text
    /// </summary>
    public void SetPrompt(string text)
    {
        promptMessage = text;
        if (promptText != null)
        {
            promptText.text = text;
        }
    }
}