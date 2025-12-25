using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows money in upper right corner - always visible
/// </summary>
public class MoneyUI : MonoBehaviour
{
    public static MoneyUI Instance;

    private Canvas canvas;
    private Text moneyText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateUI();
        Refresh();

        Debug.Log("[MoneyUI] Created");
    }

    void CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("MoneyCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Always on top

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Background panel
        GameObject panel = new GameObject("MoneyPanel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 1);
        panelRect.anchoredPosition = new Vector2(-20, -20);
        panelRect.sizeDelta = new Vector2(200, 50);

        // Money text
        GameObject textObj = new GameObject("MoneyText");
        textObj.transform.SetParent(panel.transform, false);
        moneyText = textObj.AddComponent<Text>();
        moneyText.text = "$0";
        moneyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        moneyText.fontSize = 32;
        moneyText.fontStyle = FontStyle.Bold;
        moneyText.alignment = TextAnchor.MiddleCenter;
        moneyText.color = new Color(0.2f, 1f, 0.2f); // Green

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    public void Refresh()
    {
        if (moneyText != null)
        {
            moneyText.text = "$" + PlayerMoney.Money;
        }
    }
}