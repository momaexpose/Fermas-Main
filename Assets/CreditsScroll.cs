using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Auto-scrolling credits - text comes from bottom, scrolls up
/// </summary>
public class CreditsScroller : MonoBehaviour
{
    [Header("Credits Text - Edit This!")]
    [TextArea(20, 50)]
    public string creditsText = @"YOUR GAME NAME


Created By
Your Name Here


Programming
Your Name


Art & Design
Artist Name


Music
Composer Name


Sound Effects
Sound Designer


Special Thanks
Person 1
Person 2
Person 3


Made with Unity


Thank you for playing!";

    [Header("Settings")]
    public float scrollSpeed = 50f;
    public float waitAtEnd = 2f;
    public string nextScene = "Intro";
    public int fontSize = 40;

    // Internal
    private RectTransform textRect;
    private float screenHeight;
    private float textHeight;
    private bool done = false;

    void Start()
    {
        screenHeight = 1080f; // Reference height

        // Canvas
        GameObject canvasObj = new GameObject("CreditsCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Black background (full screen)
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.black;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Credits text
        GameObject textObj = new GameObject("CreditsText");
        textObj.transform.SetParent(canvasObj.transform, false);

        Text textUI = textObj.AddComponent<Text>();
        textUI.text = creditsText;
        textUI.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textUI.fontSize = fontSize;
        textUI.color = Color.white;
        textUI.alignment = TextAnchor.UpperCenter;
        textUI.horizontalOverflow = HorizontalWrapMode.Wrap;
        textUI.verticalOverflow = VerticalWrapMode.Overflow;
        textUI.lineSpacing = 1.5f;

        textRect = textObj.GetComponent<RectTransform>();

        // Anchor to center of screen
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);

        // Set width
        textRect.sizeDelta = new Vector2(1400, 0);

        // Calculate text height
        Canvas.ForceUpdateCanvases();
        textHeight = textUI.preferredHeight;
        textRect.sizeDelta = new Vector2(1400, textHeight);

        // Start position: text TOP is at BOTTOM of screen
        // Center of text needs to be at: -(screenHeight/2) - (textHeight/2)
        float startY = -(screenHeight / 2f) - (textHeight / 2f);
        textRect.anchoredPosition = new Vector2(0, startY);

        Debug.Log("[Credits] Text height: " + textHeight + ", Start Y: " + startY);
    }

    void Update()
    {
        if (done) return;

        // Scroll UP
        textRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        // Check if done: text BOTTOM is above TOP of screen
        // Text bottom Y = textRect.y - textHeight/2
        // Screen top = screenHeight/2
        float textBottomY = textRect.anchoredPosition.y - (textHeight / 2f);

        if (textBottomY > screenHeight / 2f)
        {
            done = true;
            Debug.Log("[Credits] Finished scrolling");
            StartCoroutine(Finish());
        }

        // Skip with Space or Escape
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            done = true;
            Debug.Log("[Credits] Skipped");
            StartCoroutine(Finish());
        }
    }

    IEnumerator Finish()
    {
        yield return new WaitForSeconds(waitAtEnd);
        Debug.Log("[Credits] Loading: " + nextScene);
        SceneManager.LoadScene(nextScene);
    }
}