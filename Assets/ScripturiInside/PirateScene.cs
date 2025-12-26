using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Put this in Pirate.unity scene
/// Handles dialogue then opens trade window
/// </summary>
public class PirateScene : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea(3, 10)]
    public string[] dialogueLines = new string[]
    {
        "Ahoy there, landlubber!",
        "We've sailed far to find your... special goods.",
        "Show us what you've got, and we'll pay handsomely.",
        "Let's do business!"
    };

    [Header("Settings")]
    public float textSpeed = 0.03f;
    public float timeBetweenLines = 1f;
    public string returnScene = "MainGame";

    [Header("What Pirates Buy (leave empty for all)")]
    public List<string> acceptedDrugIds = new List<string>();
    public List<DrugCategory> acceptedCategories = new List<DrugCategory>();

    // UI
    private Canvas canvas;
    private GameObject dialoguePanel;
    private Text dialogueText;
    private Text continueText;

    private GameObject tradePanel;
    private Text tradeTitleText;
    private Text tradeContentText;
    private Text tradeInstructionsText;

    // State
    private int currentLine = 0;
    private bool dialogueFinished = false;
    private bool trading = false;
    private int selectedIndex = 0;
    private List<int> sellableIndices = new List<int>();
    private int skipFrames = 0;

    void Start()
    {
        // Ensure money system exists
        if (PlayerMoney.Instance == null)
        {
            GameObject obj = new GameObject("PlayerMoney");
            obj.AddComponent<PlayerMoney>();
        }

        if (MoneyUI.Instance == null)
        {
            GameObject obj = new GameObject("MoneyUI");
            obj.AddComponent<MoneyUI>();
        }

        CreateUI();
        StartCoroutine(RunDialogue());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (trading)
        {
            HandleTrading();
        }
    }

    void CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("PirateCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // ===== DIALOGUE PANEL =====
        dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(canvasObj.transform, false);
        Image dialogueBg = dialoguePanel.AddComponent<Image>();
        dialogueBg.color = new Color(0.1f, 0.05f, 0f, 0.95f);

        RectTransform dialogueRect = dialoguePanel.GetComponent<RectTransform>();
        dialogueRect.anchorMin = new Vector2(0.1f, 0.1f);
        dialogueRect.anchorMax = new Vector2(0.9f, 0.35f);
        dialogueRect.offsetMin = Vector2.zero;
        dialogueRect.offsetMax = Vector2.zero;

        // Dialogue text
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(dialoguePanel.transform, false);
        dialogueText = textObj.AddComponent<Text>();
        dialogueText.text = "";
        dialogueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dialogueText.fontSize = 28;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.2f);
        textRect.anchorMax = new Vector2(0.95f, 0.9f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Continue text
        GameObject continueObj = new GameObject("ContinueText");
        continueObj.transform.SetParent(dialoguePanel.transform, false);
        continueText = continueObj.AddComponent<Text>();
        continueText.text = "Press SPACE to continue...";
        continueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        continueText.fontSize = 18;
        continueText.color = new Color(0.7f, 0.7f, 0.7f);
        continueText.alignment = TextAnchor.MiddleCenter;

        RectTransform continueRect = continueObj.GetComponent<RectTransform>();
        continueRect.anchorMin = new Vector2(0, 0);
        continueRect.anchorMax = new Vector2(1, 0.2f);
        continueRect.offsetMin = Vector2.zero;
        continueRect.offsetMax = Vector2.zero;

        // ===== TRADE PANEL =====
        tradePanel = new GameObject("TradePanel");
        tradePanel.transform.SetParent(canvasObj.transform, false);
        Image tradeBg = tradePanel.AddComponent<Image>();
        tradeBg.color = new Color(0.05f, 0.1f, 0.05f, 0.95f);

        RectTransform tradeRect = tradePanel.GetComponent<RectTransform>();
        tradeRect.anchorMin = new Vector2(0.5f, 0.5f);
        tradeRect.anchorMax = new Vector2(0.5f, 0.5f);
        tradeRect.pivot = new Vector2(0.5f, 0.5f);
        tradeRect.sizeDelta = new Vector2(600, 700);

        // Trade title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(tradePanel.transform, false);
        tradeTitleText = titleObj.AddComponent<Text>();
        tradeTitleText.text = "PIRATE TRADE";
        tradeTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tradeTitleText.fontSize = 32;
        tradeTitleText.fontStyle = FontStyle.Bold;
        tradeTitleText.color = new Color(1f, 0.8f, 0.2f);
        tradeTitleText.alignment = TextAnchor.MiddleCenter;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(0, 60);

        // Trade content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(tradePanel.transform, false);
        tradeContentText = contentObj.AddComponent<Text>();
        tradeContentText.text = "";
        tradeContentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tradeContentText.fontSize = 20;
        tradeContentText.color = Color.white;
        tradeContentText.alignment = TextAnchor.UpperLeft;
        tradeContentText.supportRichText = true;

        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = new Vector2(20, 80);
        contentRect.offsetMax = new Vector2(-20, -80);

        // Trade instructions
        GameObject instrObj = new GameObject("Instructions");
        instrObj.transform.SetParent(tradePanel.transform, false);
        tradeInstructionsText = instrObj.AddComponent<Text>();
        tradeInstructionsText.text = "↑↓ Select | F Sell | ESC Leave";
        tradeInstructionsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tradeInstructionsText.fontSize = 18;
        tradeInstructionsText.color = new Color(0.7f, 0.7f, 0.7f);
        tradeInstructionsText.alignment = TextAnchor.MiddleCenter;

        RectTransform instrRect = instrObj.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0, 0);
        instrRect.anchorMax = new Vector2(1, 0);
        instrRect.pivot = new Vector2(0.5f, 0);
        instrRect.anchoredPosition = new Vector2(0, 20);
        instrRect.sizeDelta = new Vector2(0, 50);

        // Start with only dialogue visible
        dialoguePanel.SetActive(true);
        tradePanel.SetActive(false);
    }

    IEnumerator RunDialogue()
    {
        for (int i = 0; i < dialogueLines.Length; i++)
        {
            currentLine = i;
            continueText.gameObject.SetActive(false);

            // Type out text
            dialogueText.text = "";
            foreach (char c in dialogueLines[i])
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }

            // Show continue prompt
            continueText.gameObject.SetActive(true);

            // Wait for space
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }

        // Dialogue done, open trade
        dialogueFinished = true;
        dialoguePanel.SetActive(false);
        OpenTrade();
    }

    void OpenTrade()
    {
        trading = true;
        skipFrames = 5;
        tradePanel.SetActive(true);

        BuildSellableList();
        selectedIndex = 0;
        RefreshTradeUI();

        Debug.Log("[PirateScene] Trade opened with " + sellableIndices.Count + " items");
    }

    void BuildSellableList()
    {
        sellableIndices.Clear();

        ProductStorage storage = FindFirstObjectByType<ProductStorage>();
        if (storage == null)
        {
            Debug.LogWarning("[PirateScene] No ProductStorage found!");
            return;
        }

        var items = storage.storage.items;

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];

            // Skip seeds, only products
            if (item.isSeed) continue;

            // Check if pirates accept this
            if (PiratesAccept(item))
            {
                sellableIndices.Add(i);
            }
        }
    }

    bool PiratesAccept(SeedItem item)
    {
        // If no restrictions, accept all
        if (acceptedDrugIds.Count == 0 && acceptedCategories.Count == 0)
            return true;

        // Check specific drug IDs
        if (acceptedDrugIds.Contains(item.drugId))
            return true;

        // Check categories
        DrugType drugType = item.GetDrugType();
        if (drugType != null && acceptedCategories.Contains(drugType.category))
            return true;

        return false;
    }

    void HandleTrading()
    {
        if (skipFrames > 0)
        {
            skipFrames--;
            return;
        }

        int count = sellableIndices.Count;

        // Navigate
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex++;
            if (selectedIndex >= count) selectedIndex = 0;
            RefreshTradeUI();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = Mathf.Max(0, count - 1);
            RefreshTradeUI();
        }

        // Sell
        if (Input.GetKeyDown(KeyCode.F))
        {
            SellSelected();
        }

        // Leave
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LeavePirates();
        }
    }

    void SellSelected()
    {
        if (sellableIndices.Count == 0) return;
        if (selectedIndex >= sellableIndices.Count) return;

        ProductStorage storage = FindFirstObjectByType<ProductStorage>();
        if (storage == null) return;

        int itemIndex = sellableIndices[selectedIndex];
        var items = storage.storage.items;

        if (itemIndex >= items.Count) return;

        SeedItem item = items[itemIndex];
        int price = item.GetValue();

        // Add money
        PlayerMoney.Add(price);

        // Remove item
        storage.storage.RemoveAt(itemIndex, 1);

        Debug.Log("[PirateScene] Sold " + item.GetDisplayName() + " for $" + price);

        // Rebuild list
        BuildSellableList();
        if (selectedIndex >= sellableIndices.Count)
            selectedIndex = Mathf.Max(0, sellableIndices.Count - 1);

        RefreshTradeUI();
    }

    void RefreshTradeUI()
    {
        ProductStorage storage = FindFirstObjectByType<ProductStorage>();

        tradeTitleText.text = "PIRATE TRADE - Your Money: $" + PlayerMoney.Money;

        string content = "";

        if (sellableIndices.Count == 0)
        {
            content = "\n\n<color=#888888>Nothing to sell!\n\nGrow some plants first.</color>";
        }
        else
        {
            var items = storage.storage.items;

            for (int i = 0; i < sellableIndices.Count; i++)
            {
                int itemIndex = sellableIndices[i];
                if (itemIndex >= items.Count) continue;

                var item = items[itemIndex];
                DrugType drugType = item.GetDrugType();

                string line = "";

                // Selection
                if (i == selectedIndex)
                    line += "<color=#FFFF00>► </color>";
                else
                    line += "   ";

                // Quality
                string qualColor = ColorUtility.ToHtmlStringRGB(DrugDatabase.GetQualityColor(item.quality));
                line += "<color=#" + qualColor + ">" + item.quality + "</color> ";

                // Name
                string name = drugType != null ? drugType.displayName : item.drugId;
                line += name;

                // Amount
                if (item.amount > 1)
                    line += " x" + item.amount;

                // Price
                int price = item.GetValue();
                line += " <color=#00FF00>$" + price + "</color>";

                content += line + "\n";
            }
        }

        tradeContentText.text = content;
    }

    void LeavePirates()
    {
        trading = false;

        // Pirates leave
        PirateManager.PiratesLeave();

        // Return to main game - use GameData's tracked scene
        string sceneToLoad = GameData.GetMainScene();
        if (string.IsNullOrEmpty(sceneToLoad))
            sceneToLoad = returnScene;

        Debug.Log("[PirateScene] Leaving, going to " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}