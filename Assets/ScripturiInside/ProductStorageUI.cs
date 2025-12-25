using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// UI for viewing products and selling to pirates
/// </summary>
public class ProductStorageUI : MonoBehaviour
{
    public static ProductStorageUI Instance;
    public static bool IsOpen { get; private set; }

    // UI
    private Canvas canvas;
    private GameObject panel;
    private Text titleText;
    private Text contentText;
    private Text instructionsText;

    // State
    private ProductStorage currentStorage;
    private int selectedIndex = 0;
    private int skipFrames = 0;

    // Selling mode
    private bool sellingMode = false;
    private Action<SeedItem, int> sellCallback;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateUI();
        panel.SetActive(false);
    }

    void Update()
    {
        if (!IsOpen || currentStorage == null) return;

        if (skipFrames > 0)
        {
            skipFrames--;
            return;
        }

        int count = currentStorage.storage.items.Count;

        // Navigate
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

        // Sell (only in selling mode)
        if (sellingMode && Input.GetKeyDown(KeyCode.F))
        {
            SellSelected();
        }

        // Delete (only in view mode)
        if (!sellingMode && Input.GetKeyDown(KeyCode.X))
        {
            DeleteSelected();
        }

        // Close
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
        {
            Close();
        }
    }

    void CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("ProductStorageCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 202;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Panel
        panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.08f, 0.05f, 0.95f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(550, 650);
        panelRect.anchoredPosition = Vector2.zero;

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        titleText = titleObj.AddComponent<Text>();
        titleText.text = "PRODUCTS";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 30;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.9f, 0.6f);

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(0, 50);

        // Content
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
        instructionsText.text = "↑↓ Navigate | X Delete | E/ESC Close";
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
    }

    public void Open(ProductStorage storage)
    {
        currentStorage = storage;
        selectedIndex = 0;
        skipFrames = 5;
        IsOpen = true;
        sellingMode = false;
        sellCallback = null;

        panel.SetActive(true);
        instructionsText.text = "↑↓ Navigate | X Delete | E/ESC Close";
        RefreshUI();
    }

    public void OpenForSelling(ProductStorage storage, Action<SeedItem, int> callback)
    {
        currentStorage = storage;
        selectedIndex = 0;
        skipFrames = 5;
        IsOpen = true;
        sellingMode = true;
        sellCallback = callback;

        panel.SetActive(true);
        instructionsText.text = "↑↓ Navigate | F Sell | E/ESC Close";
        titleText.color = Color.green;
        RefreshUI();
    }

    public void Close()
    {
        IsOpen = false;
        panel.SetActive(false);
        currentStorage = null;
        sellingMode = false;
        sellCallback = null;
        titleText.color = new Color(1f, 0.9f, 0.6f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SellSelected()
    {
        if (!sellingMode || currentStorage == null) return;

        var items = currentStorage.storage.items;
        if (selectedIndex >= items.Count) return;

        var item = items[selectedIndex];
        int value = item.GetValue();

        // Remove one
        currentStorage.storage.RemoveAt(selectedIndex, 1);

        // Callback to pirate
        if (sellCallback != null)
        {
            sellCallback(item, value);
        }

        // Adjust selection
        if (selectedIndex >= currentStorage.storage.items.Count)
            selectedIndex = Mathf.Max(0, currentStorage.storage.items.Count - 1);

        RefreshUI();

        Debug.Log("[ProductStorageUI] Sold: " + item.GetDisplayName() + " for $" + value);
    }

    void DeleteSelected()
    {
        if (currentStorage == null) return;

        var items = currentStorage.storage.items;
        if (selectedIndex >= items.Count) return;

        currentStorage.storage.RemoveAt(selectedIndex, 1);

        if (selectedIndex >= currentStorage.storage.items.Count)
            selectedIndex = Mathf.Max(0, currentStorage.storage.items.Count - 1);

        RefreshUI();
    }

    void RefreshUI()
    {
        if (currentStorage == null) return;

        var items = currentStorage.storage.items;

        // Title
        int totalValue = currentStorage.GetTotalValue();
        if (sellingMode)
            titleText.text = "SELL PRODUCTS - Total: $" + totalValue;
        else
            titleText.text = "PRODUCTS (" + items.Count + "/" + currentStorage.storage.maxSlots + ") - Value: $" + totalValue;

        // Build content
        string content = "";

        if (items.Count == 0)
        {
            content = "\n\n<color=#888888>No products.\n\nGrow and harvest plants!</color>";
        }
        else
        {
            // Only show products (not seeds)
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.isSeed) continue; // Skip seeds

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

                // Amount
                if (item.amount > 1)
                    line += " x" + item.amount;

                // Value
                int value = item.GetValue() * item.amount;
                line += " <color=#88FF88>$" + value + "</color>";

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