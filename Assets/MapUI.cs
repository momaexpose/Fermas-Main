using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapUI : MonoBehaviour
{
    public static MapUI Instance { get; private set; }

    private GameObject mainPanel;
    private GameObject homesPanel;
    private GameObject locationsPanel;
    private Button homesTabButton;
    private Button locationsTabButton;
    private Text currentLocationText;
    private Text selectedDestinationText;
    private Text wantedDrugsText;
    private Transform homesContent;
    private Transform locationsContent;

    private Color activeTabColor = new Color(0.3f, 0.5f, 0.3f);
    private Color inactiveTabColor = new Color(0.2f, 0.2f, 0.2f);

    private bool isOpen = false;
    private List<GameObject> spawnedButtons = new List<GameObject>();
    private Canvas canvas;
    private int selectedIndex = 0;
    private bool isOnHomesTab = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateUI();
        mainPanel.SetActive(false);
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

        // Switch tabs with Left/Right or Tab
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOnHomesTab)
                ShowLocationsTab();
            else
                ShowHomesTab();
            return;
        }

        // Navigate with Up/Down arrows
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = spawnedButtons.Count - 1;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex++;
            if (selectedIndex >= spawnedButtons.Count) selectedIndex = 0;
            UpdateSelection();
        }

        // Select with Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SelectCurrent();
        }
    }

    void CreateUI()
    {
        // Find or create canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Main panel - dark background
        mainPanel = CreatePanel("MapUI", canvas.transform, new Vector2(700, 550), Vector2.zero, new Color(0.08f, 0.08f, 0.12f, 0.98f));

        // Title
        CreateLabel(mainPanel.transform, "NAVIGATION MAP", 28, new Vector2(0, 240), Color.white);

        // Current location
        currentLocationText = CreateLabel(mainPanel.transform, "Current: Black Sea", 18, new Vector2(0, 200), Color.cyan);

        // Selected destination
        selectedDestinationText = CreateLabel(mainPanel.transform, "Destination: None", 18, new Vector2(0, 175), Color.yellow);

        // Wanted drugs
        wantedDrugsText = CreateLabel(mainPanel.transform, "Pirates want: cannabis, opium", 14, new Vector2(0, 150), Color.gray);

        // Tab buttons
        homesTabButton = CreateTabButton(mainPanel.transform, "HOMES", new Vector2(-100, 110), activeTabColor);
        homesTabButton.onClick.AddListener(ShowHomesTab);

        locationsTabButton = CreateTabButton(mainPanel.transform, "LOCATIONS", new Vector2(100, 110), inactiveTabColor);
        locationsTabButton.onClick.AddListener(ShowLocationsTab);

        // Homes panel with scroll
        homesPanel = CreatePanel("HomesPanel", mainPanel.transform, new Vector2(650, 300), new Vector2(0, -60), new Color(0.12f, 0.12f, 0.18f, 1f));
        homesContent = CreateScrollContent(homesPanel);

        // Locations panel with scroll
        locationsPanel = CreatePanel("LocationsPanel", mainPanel.transform, new Vector2(650, 300), new Vector2(0, -60), new Color(0.12f, 0.12f, 0.18f, 1f));
        locationsContent = CreateScrollContent(locationsPanel);
        locationsPanel.SetActive(false);

        // Close button
        Button closeBtn = CreateCloseButton(mainPanel.transform);
        closeBtn.onClick.AddListener(Close);

        // Instructions
        CreateLabel(mainPanel.transform, "Controls: ARROWS=Navigate | TAB=Switch Tabs | ENTER=Select | ESC=Close", 13, new Vector2(0, -245), new Color(0.5f, 0.5f, 0.5f));
    }

    GameObject CreatePanel(string name, Transform parent, Vector2 size, Vector2 position, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        Image img = panel.AddComponent<Image>();
        img.color = color;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        return panel;
    }

    Transform CreateScrollContent(GameObject panel)
    {
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(panel.transform, false);
        Image viewImg = viewport.AddComponent<Image>();
        viewImg.color = Color.clear;
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        RectTransform viewRect = viewport.GetComponent<RectTransform>();
        viewRect.anchorMin = Vector2.zero;
        viewRect.anchorMax = Vector2.one;
        viewRect.offsetMin = new Vector2(10, 10);
        viewRect.offsetMax = new Vector2(-10, -10);

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);

        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ScrollRect
        ScrollRect scroll = panel.AddComponent<ScrollRect>();
        scroll.content = contentRect;
        scroll.viewport = viewRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 30f;

        return content.transform;
    }

    Text CreateLabel(Transform parent, string text, int fontSize, Vector2 position, Color color)
    {
        GameObject obj = new GameObject("Label");
        obj.transform.SetParent(parent, false);

        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(650, 30);
        rect.anchoredPosition = position;

        return t;
    }

    Button CreateTabButton(Transform parent, string label, Vector2 position, Color color)
    {
        GameObject obj = new GameObject(label + "Tab");
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = color;

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(180, 40);
        rect.anchoredPosition = position;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = label;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = 18;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return btn;
    }

    Button CreateCloseButton(Transform parent)
    {
        GameObject obj = new GameObject("CloseButton");
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.6f, 0.2f, 0.2f);

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(40, 40);
        rect.anchoredPosition = new Vector2(320, 240);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = "X";
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = 24;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return btn;
    }

    GameObject CreateListButton(Transform parent, string text, Color bgColor, System.Action onClick)
    {
        GameObject obj = new GameObject("ListButton");
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        if (onClick != null)
            btn.onClick.AddListener(() => onClick());

        LayoutElement layoutElem = obj.AddComponent<LayoutElement>();
        layoutElem.minHeight = 60;
        layoutElem.preferredHeight = 60;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = 16;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);

        return obj;
    }

    public void Open()
    {
        if (mainPanel == null) return;

        isOpen = true;
        mainPanel.SetActive(true);

        RefreshInfo();
        ShowHomesTab();

        selectedIndex = 0;
        UpdateSelection();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        isOpen = false;

        if (mainPanel != null)
            mainPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ShowHomesTab()
    {
        homesPanel.SetActive(true);
        locationsPanel.SetActive(false);

        homesTabButton.GetComponent<Image>().color = activeTabColor;
        locationsTabButton.GetComponent<Image>().color = inactiveTabColor;

        isOnHomesTab = true;
        selectedIndex = 0;

        PopulateHomes();
        UpdateSelection();
    }

    void ShowLocationsTab()
    {
        homesPanel.SetActive(false);
        locationsPanel.SetActive(true);

        homesTabButton.GetComponent<Image>().color = inactiveTabColor;
        locationsTabButton.GetComponent<Image>().color = activeTabColor;

        isOnHomesTab = false;
        selectedIndex = 0;

        PopulateLocations();
        UpdateSelection();
    }

    void ClearButtons()
    {
        foreach (var btn in spawnedButtons)
        {
            if (btn != null) Destroy(btn);
        }
        spawnedButtons.Clear();
    }

    void PopulateHomes()
    {
        ClearButtons();

        if (TravelManager.Instance == null)
        {
            Debug.LogError("TravelManager not found!");
            return;
        }

        for (int i = 0; i < TravelManager.Instance.homes.Count; i++)
        {
            var home = TravelManager.Instance.homes[i];
            int index = i;

            string displayText = home.homeName;
            Color btnColor;
            System.Action clickAction;

            if (home.owned)
            {
                displayText += "\n<size=12>Click to select as destination</size>";
                btnColor = new Color(0.2f, 0.4f, 0.2f);
                clickAction = () => SelectHome(index);
            }
            else
            {
                displayText += "\n<size=12>Price: $" + home.purchasePrice + " - Click to buy</size>";
                btnColor = new Color(0.4f, 0.2f, 0.2f);
                clickAction = () => TryBuyHome(index);
            }

            GameObject btn = CreateListButton(homesContent, displayText, btnColor, clickAction);
            spawnedButtons.Add(btn);
        }
    }

    void PopulateLocations()
    {
        ClearButtons();

        if (TravelManager.Instance == null)
        {
            Debug.LogError("TravelManager not found!");
            return;
        }

        // Random location button
        GameObject randomBtn = CreateListButton(locationsContent, "TRAVEL TO RANDOM LOCATION", new Color(0.4f, 0.3f, 0.5f), () => SelectRandomLocation());
        spawnedButtons.Add(randomBtn);

        // All locations
        for (int i = 0; i < TravelManager.Instance.locations.Count; i++)
        {
            var loc = TravelManager.Instance.locations[i];
            int index = i;
            bool isCurrent = (i == TravelManager.Instance.currentLocationIndex);

            string displayText = loc.locationName;
            if (isCurrent)
                displayText += " (YOU ARE HERE)";
            displayText += "\n<size=12>" + loc.description + " | Price x" + loc.priceMultiplier.ToString("F1") + "</size>";
            displayText += "\n<size=11>Pirates want: " + string.Join(", ", loc.wantedDrugs) + "</size>";

            Color btnColor = isCurrent ? new Color(0.3f, 0.3f, 0.5f) : new Color(0.25f, 0.25f, 0.3f);

            System.Action clickAction = null;
            if (!isCurrent)
                clickAction = () => SelectLocation(index);

            GameObject btn = CreateListButton(locationsContent, displayText, btnColor, clickAction);

            // Disable button if current location
            if (isCurrent)
                btn.GetComponent<Button>().interactable = false;

            // Make location buttons taller
            btn.GetComponent<LayoutElement>().minHeight = 80;
            btn.GetComponent<LayoutElement>().preferredHeight = 80;

            spawnedButtons.Add(btn);
        }
    }

    void SelectHome(int index)
    {
        if (TravelManager.Instance != null)
        {
            TravelManager.Instance.SelectHome(index);
            RefreshInfo();
            Close();
        }
    }

    void SelectLocation(int index)
    {
        if (TravelManager.Instance != null)
        {
            TravelManager.Instance.SelectLocation(index);
            RefreshInfo();
            Close();
        }
    }

    void SelectRandomLocation()
    {
        if (TravelManager.Instance != null)
        {
            TravelManager.Instance.SelectRandomLocation();
            RefreshInfo();
            Close();
        }
    }

    void TryBuyHome(int index)
    {
        if (TravelManager.Instance != null)
        {
            if (TravelManager.Instance.BuyHome(index))
            {
                RefreshInfo();
                PopulateHomes();
            }
            else
            {
                Debug.Log("Not enough money to buy home!");
            }
        }
    }

    void RefreshInfo()
    {
        if (TravelManager.Instance == null) return;

        if (currentLocationText != null)
            currentLocationText.text = "Current Location: " + TravelManager.Instance.currentLocationName;

        if (selectedDestinationText != null)
        {
            if (TravelManager.Instance.hasSelectedDestination)
                selectedDestinationText.text = "Destination: " + TravelManager.Instance.GetSelectedDestinationName() + " (Use LEVER to travel)";
            else
                selectedDestinationText.text = "Destination: None selected";
        }

        if (wantedDrugsText != null)
        {
            string[] wanted = TravelManager.Instance.GetWantedDrugsAtCurrentLocation();
            wantedDrugsText.text = "Pirates want here: " + string.Join(", ", wanted);
        }
    }

    void UpdateSelection()
    {
        // Clear all highlights
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            Image img = spawnedButtons[i].GetComponent<Image>();
            if (img != null)
            {
                Color baseColor = img.color;
                img.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            }
        }

        // Highlight selected
        if (selectedIndex >= 0 && selectedIndex < spawnedButtons.Count)
        {
            Image img = spawnedButtons[selectedIndex].GetComponent<Image>();
            if (img != null)
            {
                Color highlightColor = img.color;
                img.color = new Color(highlightColor.r + 0.2f, highlightColor.g + 0.2f, highlightColor.b + 0.2f, 1f);
            }

            // Add outline
            Outline outline = spawnedButtons[selectedIndex].GetComponent<Outline>();
            if (outline == null)
                outline = spawnedButtons[selectedIndex].AddComponent<Outline>();
            outline.effectColor = Color.yellow;
            outline.effectDistance = new Vector2(3, -3);
        }

        // Remove outline from non-selected
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (i != selectedIndex)
            {
                Outline outline = spawnedButtons[i].GetComponent<Outline>();
                if (outline != null)
                    Destroy(outline);
            }
        }
    }

    void SelectCurrent()
    {
        if (selectedIndex < 0 || selectedIndex >= spawnedButtons.Count) return;

        Button btn = spawnedButtons[selectedIndex].GetComponent<Button>();
        if (btn != null && btn.interactable)
        {
            btn.onClick.Invoke();
        }
    }

    public bool IsOpen() { return isOpen; }
}