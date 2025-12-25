using UnityEngine;
using UnityEngine.UI;

public class ChestUI : MonoBehaviour
{
    public static ChestUI Instance { get; private set; }

    private GameObject mainPanel;
    private Transform chestSlotContainer;
    private Transform inventorySlotContainer;
    private Text titleText;
    private Text instructionsText;

    private InventorySlotUI[] chestSlots;
    private InventorySlotUI[] inventorySlots;
    private ChestStorage currentChest;
    private bool isOpen = false;

    private int selectedSlot = 0;
    private bool isSelectingChest = true; // true = chest, false = inventory

    private float slotSize = 50f;
    private float slotSpacing = 4f;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        CreateUI();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.onInventoryChanged += RefreshAll;

        // Ensure closed at start
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
            Debug.Log("[ChestUI] Initialized and closed");
        }

        isOpen = false;
    }

    void OnEnable()
    {
        // Double-check it's closed when enabled
        if (!isOpen && mainPanel != null)
        {
            mainPanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.onInventoryChanged -= RefreshAll;
    }

    void Update()
    {
        if (!isOpen) return;

        // Close with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        // Switch between chest and inventory with Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isSelectingChest = !isSelectingChest;
            selectedSlot = 0;
            UpdateSelection();
            return;
        }

        // Get current inventory
        Inventory currentInv = isSelectingChest ?
            (currentChest != null ? currentChest.chestInventory : null) :
            (InventoryManager.Instance != null ? InventoryManager.Instance.playerInventory : null);

        if (currentInv == null) return;

        int maxSlots = currentInv.TotalSlots;
        int columns = currentInv.width;

        // Navigate with arrow keys
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedSlot -= columns;
            if (selectedSlot < 0) selectedSlot += maxSlots;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedSlot += columns;
            if (selectedSlot >= maxSlots) selectedSlot -= maxSlots;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedSlot--;
            if (selectedSlot < 0) selectedSlot = maxSlots - 1;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedSlot++;
            if (selectedSlot >= maxSlots) selectedSlot = 0;
            UpdateSelection();
        }

        // Transfer item with Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            TransferSelectedItem();
        }
    }

    void CreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("ChestCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Main panel - dark background covering screen
        mainPanel = new GameObject("ChestUI");
        mainPanel.transform.SetParent(canvas.transform, false);

        Image darkBg = mainPanel.AddComponent<Image>();
        darkBg.color = new Color(0, 0, 0, 0.85f);

        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.sizeDelta = Vector2.zero;

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mainPanel.transform, false);
        titleText = titleObj.AddComponent<Text>();
        titleText.text = "CHEST STORAGE";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (titleText.font == null) titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 28;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(800, 40);

        // Instructions
        GameObject instrObj = new GameObject("Instructions");
        instrObj.transform.SetParent(mainPanel.transform, false);
        instructionsText = instrObj.AddComponent<Text>();
        instructionsText.text = "TAB=Switch | ARROWS=Navigate | ENTER=Transfer Item | ESC=Close";
        instructionsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (instructionsText.font == null) instructionsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        instructionsText.fontSize = 14;
        instructionsText.color = new Color(0.7f, 0.7f, 0.7f);
        instructionsText.alignment = TextAnchor.MiddleCenter;
        RectTransform instrRect = instrObj.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0.5f, 1);
        instrRect.anchorMax = new Vector2(0.5f, 1);
        instrRect.pivot = new Vector2(0.5f, 1);
        instrRect.anchoredPosition = new Vector2(0, -60);
        instrRect.sizeDelta = new Vector2(800, 25);

        // Container for both panels
        GameObject container = new GameObject("Container");
        container.transform.SetParent(mainPanel.transform, false);
        HorizontalLayoutGroup hLayout = container.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 20;
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = false;
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(1200, 550);

        // Chest panel (left)
        GameObject chestPanel = CreatePanel(container.transform, "CHEST (20x9)", new Vector2(750, 550), new Color(0.15f, 0.12f, 0.1f, 0.95f));
        chestSlotContainer = chestPanel.transform.Find("SlotContainer");

        // Inventory panel (right)
        GameObject invPanel = CreatePanel(container.transform, "YOUR INVENTORY (5x9)", new Vector2(400, 550), new Color(0.1f, 0.1f, 0.15f, 0.95f));
        inventorySlotContainer = invPanel.transform.Find("SlotContainer");
    }

    GameObject CreatePanel(Transform parent, string title, Vector2 size, Color bgColor)
    {
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(parent, false);

        Image bg = panel.AddComponent<Image>();
        bg.color = bgColor;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = size;

        // Title
        GameObject titleObj = new GameObject("PanelTitle");
        titleObj.transform.SetParent(panel.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = title;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (titleText.font == null) titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 20;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(-20, 35);

        // Slot container
        GameObject container = new GameObject("SlotContainer");
        container.transform.SetParent(panel.transform, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0, -10);
        containerRect.sizeDelta = new Vector2(size.x - 20, size.y - 80);

        return panel;
    }

    public void Open(ChestStorage chest)
    {
        currentChest = chest;
        isOpen = true;

        if (mainPanel != null)
            mainPanel.SetActive(true);

        CreateChestSlots();
        CreateInventorySlots();

        selectedSlot = 0;
        isSelectingChest = true;

        RefreshAll();
        UpdateSelection();
    }

    public void Close()
    {
        isOpen = false;
        currentChest = null;

        if (mainPanel != null)
            mainPanel.SetActive(false);

        ClearSlots();
    }

    void CreateChestSlots()
    {
        if (currentChest == null || chestSlotContainer == null) return;

        ClearContainer(chestSlotContainer);

        Inventory inv = currentChest.chestInventory;
        chestSlots = new InventorySlotUI[inv.TotalSlots];

        for (int y = 0; y < inv.height; y++)
        {
            for (int x = 0; x < inv.width; x++)
            {
                int index = y * inv.width + x;

                GameObject slotObj = CreateSlot();
                slotObj.transform.SetParent(chestSlotContainer, false);

                RectTransform rect = slotObj.GetComponent<RectTransform>();
                float startX = -(inv.width - 1) * (slotSize + slotSpacing) / 2f;
                float startY = (inv.height - 1) * (slotSize + slotSpacing) / 2f;
                rect.anchoredPosition = new Vector2(
                    startX + x * (slotSize + slotSpacing),
                    startY - y * (slotSize + slotSpacing)
                );

                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                slotUI.Setup(inv, index);
                chestSlots[index] = slotUI;
            }
        }
    }

    void CreateInventorySlots()
    {
        if (InventoryManager.Instance == null || inventorySlotContainer == null) return;

        ClearContainer(inventorySlotContainer);

        Inventory inv = InventoryManager.Instance.playerInventory;
        inventorySlots = new InventorySlotUI[inv.TotalSlots];

        float startX = -(inv.width - 1) * (slotSize + slotSpacing) / 2f;
        float startY = (inv.height - 1) * (slotSize + slotSpacing) / 2f;

        for (int y = 0; y < inv.height; y++)
        {
            for (int x = 0; x < inv.width; x++)
            {
                int index = y * inv.width + x;

                GameObject slotObj = CreateSlot();
                slotObj.transform.SetParent(inventorySlotContainer, false);

                RectTransform rect = slotObj.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(
                    startX + x * (slotSize + slotSpacing),
                    startY - y * (slotSize + slotSpacing)
                );

                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                slotUI.Setup(inv, index);
                inventorySlots[index] = slotUI;
            }
        }
    }

    GameObject CreateSlot()
    {
        GameObject slotObj = new GameObject("Slot");

        RectTransform rect = slotObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(slotSize, slotSize);

        Image bg = slotObj.AddComponent<Image>();
        bg.color = new Color(0.25f, 0.25f, 0.25f, 0.9f);

        InventorySlotUI slotUI = slotObj.AddComponent<InventorySlotUI>();
        slotUI.backgroundImage = bg;

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.raycastTarget = false;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-6, -6);
        iconRect.anchoredPosition = Vector2.zero;
        slotUI.iconImage = iconImg;

        GameObject amountObj = new GameObject("Amount");
        amountObj.transform.SetParent(slotObj.transform, false);
        Text amountText = amountObj.AddComponent<Text>();
        amountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (amountText.font == null) amountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        amountText.fontSize = 12;
        amountText.color = Color.white;
        amountText.alignment = TextAnchor.LowerRight;
        amountText.raycastTarget = false;
        amountObj.AddComponent<Outline>().effectColor = Color.black;
        RectTransform amountRect = amountObj.GetComponent<RectTransform>();
        amountRect.anchorMin = Vector2.zero;
        amountRect.anchorMax = Vector2.one;
        amountRect.sizeDelta = new Vector2(-3, -3);
        amountRect.anchoredPosition = Vector2.zero;
        slotUI.amountText = amountText;

        return slotObj;
    }

    void UpdateSelection()
    {
        // Clear all highlights from chest
        if (chestSlots != null)
        {
            for (int i = 0; i < chestSlots.Length; i++)
            {
                if (chestSlots[i] != null && chestSlots[i].backgroundImage != null)
                {
                    chestSlots[i].backgroundImage.color = new Color(0.25f, 0.25f, 0.25f, 0.9f);
                    Outline outline = chestSlots[i].GetComponent<Outline>();
                    if (outline != null) Destroy(outline);
                }
            }
        }

        // Clear all highlights from inventory
        if (inventorySlots != null)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] != null && inventorySlots[i].backgroundImage != null)
                {
                    inventorySlots[i].backgroundImage.color = new Color(0.25f, 0.25f, 0.25f, 0.9f);
                    Outline outline = inventorySlots[i].GetComponent<Outline>();
                    if (outline != null) Destroy(outline);
                }
            }
        }

        // Highlight selected slot
        InventorySlotUI[] currentSlots = isSelectingChest ? chestSlots : inventorySlots;

        if (currentSlots != null && selectedSlot >= 0 && selectedSlot < currentSlots.Length)
        {
            InventorySlotUI slot = currentSlots[selectedSlot];
            if (slot != null && slot.backgroundImage != null)
            {
                slot.backgroundImage.color = new Color(0.5f, 0.5f, 0.3f, 1f);

                Outline outline = slot.GetComponent<Outline>();
                if (outline == null) outline = slot.gameObject.AddComponent<Outline>();
                outline.effectColor = Color.yellow;
                outline.effectDistance = new Vector2(3, -3);
            }
        }

        // Update title to show which side is selected
        if (titleText != null)
        {
            if (isSelectingChest)
                titleText.text = "CHEST STORAGE >>> [SELECTING CHEST]";
            else
                titleText.text = "[SELECTING INVENTORY] <<< CHEST STORAGE";
        }
    }

    void TransferSelectedItem()
    {
        Inventory sourceInv = isSelectingChest ?
            (currentChest != null ? currentChest.chestInventory : null) :
            (InventoryManager.Instance != null ? InventoryManager.Instance.playerInventory : null);

        Inventory destInv = isSelectingChest ?
            (InventoryManager.Instance != null ? InventoryManager.Instance.playerInventory : null) :
            (currentChest != null ? currentChest.chestInventory : null);

        if (sourceInv == null || destInv == null) return;

        ItemStack sourceStack = sourceInv.GetSlot(selectedSlot);
        if (sourceStack == null || sourceStack.IsEmpty()) return;

        // Try to add to destination
        int remaining = destInv.AddItem(sourceStack.item, sourceStack.amount);

        // Remove from source
        int transferred = sourceStack.amount - remaining;
        if (transferred > 0)
        {
            sourceInv.RemoveItem(sourceStack.item, transferred);

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.NotifyChange();

            RefreshAll();
            UpdateSelection();
        }
    }

    void ClearContainer(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    void ClearSlots()
    {
        ClearContainer(chestSlotContainer);
        ClearContainer(inventorySlotContainer);
        chestSlots = null;
        inventorySlots = null;
    }

    void RefreshAll()
    {
        if (chestSlots != null)
        {
            foreach (var slot in chestSlots)
                if (slot != null) slot.Refresh();
        }

        if (inventorySlots != null)
        {
            foreach (var slot in inventorySlots)
                if (slot != null) slot.Refresh();
        }
    }

    public bool IsOpen() { return isOpen; }
}