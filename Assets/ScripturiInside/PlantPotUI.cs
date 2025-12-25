using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI for selecting seeds to plant
/// Press F to plant selected seed
/// </summary>
public class PlantPotUI : MonoBehaviour
{
    public static PlantPotUI Instance;
    public static bool IsOpen { get; private set; }

    // UI
    private Canvas canvas;
    private GameObject panel;
    private Text titleText;
    private Text contentText;
    private Text instructionsText;

    // State
    private PlantPot currentPot;
    private SeedChest currentChest;
    private int selectedIndex = 0;
    private int skipFrames = 0;
    private List<int> seedIndices = new List<int>(); // Indices of seeds only

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateUI();
        panel.SetActive(false);
        IsOpen = false;
    }

    void Update()
    {
        if (!IsOpen || currentChest == null || currentPot == null) return;

        // Skip frames after opening
        if (skipFrames > 0)
        {
            skipFrames--;
            return;
        }

        int count = seedIndices.Count;

        // Navigate with UP/DOWN arrows
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

        // F to plant selected seed
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("[PlantPotUI] F pressed!");
            PlantSelected();
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
        GameObject canvasObj = new GameObject("PlantPotCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 201;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Panel background
        panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.1f, 0.05f, 0.95f);

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
        titleText.text = "SELECT SEED TO PLANT";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.green;

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
        instructionsText.text = "↑↓ Navigate | F Plant | E/ESC Close";
        instructionsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionsText.fontSize = 16;
        instructionsText.alignment = TextAnchor.MiddleCenter;
        instructionsText.color = new Color(0.7f, 0.9f, 0.7f);

        RectTransform instrRect = instrObj.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0, 0);
        instrRect.anchorMax = new Vector2(1, 0);
        instrRect.pivot = new Vector2(0.5f, 0);
        instrRect.anchoredPosition = new Vector2(0, 20);
        instrRect.sizeDelta = new Vector2(0, 40);

        Debug.Log("[PlantPotUI] Created");
    }

    public void Open(PlantPot pot, SeedChest chest)
    {
        currentPot = pot;
        currentChest = chest;
        selectedIndex = 0;
        skipFrames = 5;
        IsOpen = true;

        // Build list of seed indices (only seeds, not products)
        BuildSeedList();

        panel.SetActive(true);
        RefreshUI();

        Debug.Log("[PlantPotUI] Opened with " + seedIndices.Count + " seeds");
    }

    public void Close()
    {
        IsOpen = false;
        panel.SetActive(false);
        currentPot = null;
        currentChest = null;
        seedIndices.Clear();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[PlantPotUI] Closed");
    }

    void BuildSeedList()
    {
        seedIndices.Clear();

        if (currentChest == null) return;

        var items = currentChest.storage.items;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].isSeed)
            {
                seedIndices.Add(i);
            }
        }
    }

    void PlantSelected()
    {
        Debug.Log("[PlantPotUI] PlantSelected called");
        Debug.Log("[PlantPotUI] seedIndices.Count: " + seedIndices.Count);
        Debug.Log("[PlantPotUI] selectedIndex: " + selectedIndex);
        Debug.Log("[PlantPotUI] currentPot: " + (currentPot != null ? currentPot.name : "NULL"));
        Debug.Log("[PlantPotUI] currentChest: " + (currentChest != null ? currentChest.name : "NULL"));

        if (seedIndices.Count == 0 || currentPot == null || currentChest == null)
        {
            Debug.Log("[PlantPotUI] Early return - missing data");
            return;
        }
        if (selectedIndex >= seedIndices.Count)
        {
            Debug.Log("[PlantPotUI] Early return - selectedIndex out of range");
            return;
        }

        int itemIndex = seedIndices[selectedIndex];
        var items = currentChest.storage.items;

        Debug.Log("[PlantPotUI] itemIndex: " + itemIndex + ", items.Count: " + items.Count);

        if (itemIndex >= items.Count)
        {
            Debug.Log("[PlantPotUI] Early return - itemIndex out of range");
            return;
        }

        SeedItem seed = items[itemIndex];
        Debug.Log("[PlantPotUI] Seed: " + seed.GetDisplayName());

        // Clone the seed for planting
        SeedItem plantingSeed = new SeedItem(seed.drugId, seed.quality, true, 1);

        // Remove one from storage
        currentChest.storage.RemoveAt(itemIndex, 1);

        // Plant it
        Debug.Log("[PlantPotUI] Calling PlantSeed on pot");
        currentPot.PlantSeed(plantingSeed);

        // Close UI
        Close();
    }

    void RefreshUI()
    {
        if (currentChest == null) return;

        // Rebuild seed list in case it changed
        BuildSeedList();

        var items = currentChest.storage.items;

        // Title
        titleText.text = "SELECT SEED TO PLANT (" + seedIndices.Count + " seeds)";

        // Build content
        string content = "";

        if (seedIndices.Count == 0)
        {
            content = "\n\n<color=#888888>No seeds available!\n\nSend Grasu' to find some.</color>";
        }
        else
        {
            for (int i = 0; i < seedIndices.Count; i++)
            {
                int itemIndex = seedIndices[i];
                if (itemIndex >= items.Count) continue;

                var item = items[itemIndex];
                DrugType drugType = item.GetDrugType();

                string line = "";

                // Selection marker
                if (i == selectedIndex)
                    line += "<color=#00FF00>► </color>";
                else
                    line += "   ";

                // Quality color
                string qualColor = ColorUtility.ToHtmlStringRGB(DrugDatabase.GetQualityColor(item.quality));
                line += "<color=#" + qualColor + ">" + item.quality + "</color> ";

                // Drug name
                string name = drugType != null ? drugType.displayName : item.drugId;
                line += name + " Seeds";

                // Amount
                if (item.amount > 1)
                    line += " x" + item.amount;

                // Grow time
                if (drugType != null)
                {
                    float growMins = drugType.growTime / 60f;
                    line += " <color=#888888>(" + growMins.ToString("F1") + " min)</color>";
                }

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