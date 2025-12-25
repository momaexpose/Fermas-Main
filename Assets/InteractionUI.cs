using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject uiPanel;
    public Image iconImage;
    public Text promptText;

    [Header("Default Settings")]
    public string defaultPrompt = "E to Interact";
    public Sprite defaultIcon;
    public Color textColor = Color.white;
    public float iconSize = 60f;
    public float fontSize = 24f;
    public float fadeSpeed = 10f;

    private CanvasGroup canvasGroup;
    private bool shouldShow = false;
    private Sprite currentIcon;
    private float hideUntilTime = 0f;

    void Start()
    {
        if (uiPanel == null)
            CreateUI();

        // Create simple default icon if none assigned
        if (defaultIcon == null)
            defaultIcon = CreateSimpleHandIcon();

        currentIcon = null;

        // Start completely hidden
        ForceHideAndClear();

        // Stay hidden for a moment when scene loads
        hideUntilTime = Time.time + 2f;
    }

    Sprite CreateSimpleHandIcon()
    {
        // Create a simple white square as default icon
        Texture2D tex = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];

        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % 32;
            int y = i / 32;

            // Draw a simple hand shape
            if ((x > 10 && x < 22) && (y > 8 && y < 28))
                pixels[i] = Color.white;
            else if ((x > 14 && x < 18) && (y > 5 && y < 10))
                pixels[i] = Color.white;
            else
                pixels[i] = Color.clear;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

    void ForceHideAndClear()
    {
        shouldShow = false;

        if (canvasGroup != null)
            canvasGroup.alpha = 0;

        if (uiPanel != null)
            uiPanel.SetActive(false);

        // Clear the icon sprite completely
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        // Clear text
        if (promptText != null)
            promptText.text = "";
    }

    void CreateUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("InteractionCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Below fade canvas (999)
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        uiPanel = new GameObject("InteractionUI");
        uiPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = uiPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, -50);
        panelRect.sizeDelta = new Vector2(300, 120);

        canvasGroup = uiPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        VerticalLayoutGroup layout = uiPanel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 10;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        // Icon - start disabled
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(uiPanel.transform, false);
        iconImage = iconObj.AddComponent<Image>();
        iconImage.color = textColor;
        iconImage.enabled = false;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);

        // Text - start empty
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(uiPanel.transform, false);
        promptText = textObj.AddComponent<Text>();
        promptText.text = "";
        promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        promptText.fontSize = (int)fontSize;
        promptText.color = textColor;
        promptText.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(300, 40);

        // Outline for readability
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        // Start with panel inactive
        uiPanel.SetActive(false);
    }

    void Update()
    {
        if (canvasGroup == null) return;

        // Force hide during scene transitions
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning())
        {
            ForceHideAndClear();
            hideUntilTime = Time.time + 1.5f;
            return;
        }

        // Stay hidden until hideUntilTime passes (prevents flash on scene load)
        if (Time.time < hideUntilTime)
        {
            if (uiPanel != null && uiPanel.activeSelf)
                ForceHideAndClear();
            return;
        }

        if (shouldShow)
        {
            // Make sure panel is active when showing
            if (uiPanel != null && !uiPanel.activeSelf)
                uiPanel.SetActive(true);

            if (iconImage != null && !iconImage.enabled)
                iconImage.enabled = true;

            float targetAlpha = 1f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
        else
        {
            float targetAlpha = 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

            // When fully faded out, disable panel and clear icon
            if (canvasGroup.alpha < 0.01f)
            {
                ForceHideAndClear();
            }
        }
    }

    /// <summary>
    /// Show the interaction prompt
    /// </summary>
    public void Show()
    {
        // Don't show if we're still in the hide period
        if (Time.time < hideUntilTime) return;

        // Don't show during transitions
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning()) return;

        shouldShow = true;

        // Activate panel
        if (uiPanel != null)
            uiPanel.SetActive(true);

        // Apply current icon, or default if none set
        if (iconImage != null)
        {
            Sprite iconToUse = currentIcon != null ? currentIcon : defaultIcon;

            if (iconToUse != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = iconToUse;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// Hide the interaction prompt
    /// </summary>
    public void Hide()
    {
        shouldShow = false;
        currentIcon = null;
    }

    /// <summary>
    /// Set the prompt text
    /// </summary>
    public void SetPrompt(string text)
    {
        // Don't set if in hide period or transitioning
        if (Time.time < hideUntilTime) return;
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning()) return;

        if (promptText != null)
            promptText.text = text;
    }

    /// <summary>
    /// Set the icon sprite
    /// </summary>
    public void SetIcon(Sprite icon)
    {
        // Don't set if in hide period or transitioning
        if (Time.time < hideUntilTime) return;
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning()) return;

        currentIcon = icon;

        if (iconImage != null)
            iconImage.sprite = currentIcon;
    }

    /// <summary>
    /// Set both prompt and icon at once
    /// </summary>
    public void SetPromptAndIcon(string text, Sprite icon)
    {
        // Don't set if in hide period or transitioning
        if (Time.time < hideUntilTime) return;
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning()) return;

        SetPrompt(text);
        SetIcon(icon);
    }

    public bool IsVisible() { return shouldShow; }
}