using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI for seed chest
/// </summary>
public class SeedChestUI : MonoBehaviour
{
    public static SeedChestUI Instance;
    public static bool IsOpen { get; private set; }

    // UI
    private Canvas canvas;
    private GameObject panel;
    private Text titleText;
    private Text contentText;
    private Text instructionsText;

    // State
    private SeedChest currentChest;
    private int selectedIndex = 0;
    private int skipFrames = 0;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateUI();
        panel.SetActive(false);
    }

    void Update()
    {
        if (!IsOpen || currentChest == null) return;

        // Skip frames after opening
        if (skipFrames > 0)
        {
            skipFrames--;
            return;
        }

        int count = currentChest.storage.items.Count;

        // Navigate with UP/DOWN arrows only
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex++;
            if (selectedIndex >= count) selectedIndex = 0;
            RefreshUI();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = Mathf.Max(0, count - 1);
            RefreshUI();
        }

        // Delete
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (count > 0 && selectedIndex < count)
            {
                currentChest.storage.RemoveAt(selectedIndex, 1);
                if (selectedIndex >= currentChest.storage.items.Count)
                    selectedIndex = Mathf.Max(0, currentChest.storage.items.Count - 1);
                RefreshUI();
            }
        }

        // NOTE: Close is handled by SeedChest, not here
    }

    void CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("SeedChestCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Panel background
        panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(500, 600);
        panelRect.anchoredPosition = Vector2.zero;

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        titleText = titleObj.AddComponent<Text>();
        titleText.text = "SEED CHEST";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(0, 50);

        // Content (scrollable text list)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(panel.transform, false);
        contentText = contentObj.AddComponent<Text>();
        contentText.text = "";
        contentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        contentText.fontSize = 18;
        contentText.alignment = TextAnchor.UpperLeft;
        contentText.color = Color.white;
        contentText.supportRichText = true;

        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.offsetMin = new Vector2(20, 80);
        contentRect.offsetMax = new Vector2(-20, -70);

        // Instructions
        GameObject instrObj = new GameObject("Instructions");
        instrObj.transform.SetParent(panel.transform, false);
        instructionsText = instrObj.AddComponent<Text>();
        instructionsText.text = "↑↓ Navigate | X Delete | E Close";
        instructionsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionsText.fontSize = 16;
        instructionsText.alignment = TextAnchor.MiddleCenter;
        instructionsText.color = new Color(0.7f, 0.7f, 0.7f);

        RectTransform instrRect = instrObj.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0, 0);
        instrRect.anchorMax = new Vector2(1, 0);
        instrRect.pivot = new Vector2(0.5f, 0);
        instrRect.anchoredPosition = new Vector2(0, 20);
        instrRect.sizeDelta = new Vector2(0, 40);

        Debug.Log("[SeedChestUI] Created");
    }

    public void Open(SeedChest chest)
    {
        Debug.Log("[SeedChestUI] OPEN CALLED");
        currentChest = chest;
        selectedIndex = 0;
        skipFrames = 5;
        panel.SetActive(true);
        IsOpen = true;
        RefreshUI();
    }

    public void Close()
    {
        Debug.Log("[SeedChestUI] CLOSE CALLED");
        IsOpen = false;
        panel.SetActive(false);
        currentChest = null;
    }

    void RefreshUI()
    {
        if (currentChest == null) return;

        var items = currentChest.storage.items;

        // Title
        titleText.text = "SEED CHEST (" + items.Count + "/" + currentChest.storage.maxSlots + ")";

        // Build content
        string content = "";

        if (items.Count == 0)
        {
            content = "\n\n<color=#888888>Chest is empty.\n\nSend Grasu' to find seeds!</color>";
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                DrugType drugType = item.GetDrugType();

                string line = "";

                // Selection marker
                if (i == selectedIndex)
                    line += "<color=#FFFF00>► </color>";
                else
                    line += "   ";

                // Quality color
                string qualColor = ColorUtility.ToHtmlStringRGB(DrugDatabase.GetQualityColor(item.quality));
                line += "<color=#" + qualColor + ">" + item.quality + "</color> ";

                // Drug name
                string name = drugType != null ? drugType.displayName : item.drugId;
                line += name;

                // Seed tag
                if (item.isSeed)
                    line += " <color=#88FF88>(Seed)</color>";

                // Amount
                if (item.amount > 1)
                    line += " x" + item.amount;

                // Value
                int value = item.GetValue() * item.amount;
                line += " <color=#888888>$" + value + "</color>";

                // Rarity
                if (drugType != null)
                {
                    string rarityColor = ColorUtility.ToHtmlStringRGB(DrugDatabase.GetRarityColor(drugType.rarity));
                    line += " <color=#" + rarityColor + ">[" + drugType.rarity + "]</color>";
                }

                content += line + "\n";
            }
        }

        contentText.text = content;
    }
}