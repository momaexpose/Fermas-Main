using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player inventory UI. 5x9 grid = 45 slots.
/// Press Tab to open/close.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventoryPanel;
    public Transform slotContainer;
    public Text moneyText;

    [Header("Slot Prefab")]
    public GameObject slotPrefab;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Tab;
    public float slotSize = 60f;
    public float slotSpacing = 5f;

    private InventorySlotUI[] slotUIs;
    private bool isOpen = false;

    void Start()
    {
        // Create UI if needed
        if (inventoryPanel == null)
        {
            CreateUI();
        }

        // Create slots
        CreateSlots();

        // Subscribe to changes
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged += RefreshSlots;
            InventoryManager.Instance.onMoneyChanged += RefreshMoney;
        }

        // Start closed
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        RefreshMoney();
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged -= RefreshSlots;
            InventoryManager.Instance.onMoneyChanged -= RefreshMoney;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            Close();
        }
    }

    void CreateUI()
    {
        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("InventoryCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Main panel
        inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(canvas.transform, false);

        Image bg = inventoryPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        RectTransform panelRect = inventoryPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.5f);
        panelRect.anchorMax = new Vector2(0, 0.5f);
        panelRect.pivot = new Vector2(0, 0.5f);
        panelRect.anchoredPosition = new Vector2(20, 0);

        // Calculate size based on 5x9 grid
        float width = 5 * slotSize + 6 * slotSpacing + 20;
        float height = 9 * slotSize + 10 * slotSpacing + 80;
        panelRect.sizeDelta = new Vector2(width, height);

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(inventoryPanel.transform, false);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "INVENTORY";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 20;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(0, 30);

        // Money text
        GameObject moneyObj = new GameObject("MoneyText");
        moneyObj.transform.SetParent(inventoryPanel.transform, false);

        moneyText = moneyObj.AddComponent<Text>();
        moneyText.text = "$0";
        moneyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        moneyText.fontSize = 18;
        moneyText.color = Color.green;
        moneyText.alignment = TextAnchor.MiddleCenter;

        RectTransform moneyRect = moneyObj.GetComponent<RectTransform>();
        moneyRect.anchorMin = new Vector2(0, 0);
        moneyRect.anchorMax = new Vector2(1, 0);
        moneyRect.pivot = new Vector2(0.5f, 0);
        moneyRect.anchoredPosition = new Vector2(0, 10);
        moneyRect.sizeDelta = new Vector2(0, 30);

        // Slot container
        GameObject containerObj = new GameObject("SlotContainer");
        containerObj.transform.SetParent(inventoryPanel.transform, false);

        slotContainer = containerObj.transform;

        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0, 0);
        containerRect.sizeDelta = new Vector2(width - 20, height - 80);
    }

    void CreateSlots()
    {
        if (InventoryManager.Instance == null) return;

        Inventory inv = InventoryManager.Instance.playerInventory;
        if (inv == null) return;

        slotUIs = new InventorySlotUI[inv.TotalSlots];

        float startX = -(inv.width - 1) * (slotSize + slotSpacing) / 2f;
        float startY = (inv.height - 1) * (slotSize + slotSpacing) / 2f;

        for (int y = 0; y < inv.height; y++)
        {
            for (int x = 0; x < inv.width; x++)
            {
                int index = y * inv.width + x;

                GameObject slotObj = CreateSlot();
                slotObj.transform.SetParent(slotContainer, false);

                RectTransform rect = slotObj.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(
                    startX + x * (slotSize + slotSpacing),
                    startY - y * (slotSize + slotSpacing)
                );

                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                slotUI.Setup(inv, index);
                slotUIs[index] = slotUI;
            }
        }
    }

    GameObject CreateSlot()
    {
        if (slotPrefab != null)
        {
            return Instantiate(slotPrefab);
        }

        // Create slot from scratch
        GameObject slotObj = new GameObject("Slot");

        RectTransform rect = slotObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(slotSize, slotSize);

        // Background
        Image bg = slotObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        InventorySlotUI slotUI = slotObj.AddComponent<InventorySlotUI>();
        slotUI.backgroundImage = bg;

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);

        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.raycastTarget = false;

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-8, -8);
        iconRect.anchoredPosition = Vector2.zero;

        slotUI.iconImage = iconImg;

        // Amount text
        GameObject amountObj = new GameObject("Amount");
        amountObj.transform.SetParent(slotObj.transform, false);

        Text amountText = amountObj.AddComponent<Text>();
        amountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        amountText.fontSize = 14;
        amountText.color = Color.white;
        amountText.alignment = TextAnchor.LowerRight;
        amountText.raycastTarget = false;

        Outline outline = amountObj.AddComponent<Outline>();
        outline.effectColor = Color.black;

        RectTransform amountRect = amountObj.GetComponent<RectTransform>();
        amountRect.anchorMin = Vector2.zero;
        amountRect.anchorMax = Vector2.one;
        amountRect.sizeDelta = new Vector2(-4, -4);
        amountRect.anchoredPosition = Vector2.zero;

        slotUI.amountText = amountText;

        return slotObj;
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        isOpen = true;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        RefreshSlots();
        RefreshMoney();

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        isOpen = false;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        // Hide cursor if no other UI is open
        if (!ChestStorage.IsAnyChestOpen())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void RefreshSlots()
    {
        if (slotUIs == null) return;

        foreach (var slot in slotUIs)
        {
            if (slot != null)
            {
                slot.Refresh();
            }
        }
    }

    void RefreshMoney()
    {
        if (moneyText != null && InventoryManager.Instance != null)
        {
            moneyText.text = "$" + InventoryManager.Instance.money;
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}