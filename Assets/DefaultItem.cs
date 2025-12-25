using UnityEngine;

public class DefaultItemDatabase : MonoBehaviour
{
    public ItemData[] items;

    void Awake()
    {
        CreateDefaultItems();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.allItems = items;
    }

    void CreateDefaultItems()
    {
        items = new ItemData[10];

        items[0] = CreateItem("Cannabis", "cannabis", 50, new Color(0.2f, 0.7f, 0.2f));
        items[1] = CreateItem("Premium Cannabis", "cannabis_premium", 120, new Color(0.4f, 0.9f, 0.3f));
        items[2] = CreateItem("Opium", "opium", 150, new Color(0.6f, 0.4f, 0.2f));
        items[3] = CreateItem("Cocaine", "cocaine", 300, new Color(0.95f, 0.95f, 0.95f));
        items[4] = CreateItem("Mushrooms", "mushrooms", 80, new Color(0.7f, 0.5f, 0.3f));
        items[5] = CreateItem("Peyote", "peyote", 200, new Color(0.4f, 0.6f, 0.4f));
        items[6] = CreateItem("Pills", "pills", 100, new Color(0.8f, 0.3f, 0.3f));
        items[7] = CreateItem("LSA Seeds", "lsa", 60, new Color(0.5f, 0.4f, 0.3f));
        items[8] = CreateItem("Salvia", "salvia", 180, new Color(0.3f, 0.5f, 0.3f));
        items[9] = CreateItem("San Pedro", "sanpedro", 220, new Color(0.5f, 0.7f, 0.5f));
    }

    ItemData CreateItem(string name, string id, int value, Color color)
    {
        ItemData item = ScriptableObject.CreateInstance<ItemData>();
        item.itemName = name;
        item.itemID = id;
        item.maxStack = 9;
        item.baseValue = value;
        item.icon = CreateColoredIcon(color);
        return item;
    }

    Sprite CreateColoredIcon(Color color)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);

        Color[] pixels = new Color[size * size];
        Color border = color * 0.6f;
        border.a = 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - size / 2f) / (size / 2f);
                float dy = Mathf.Abs(y - size / 2f) / (size / 2f);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > 0.9f)
                    pixels[y * size + x] = Color.clear;
                else if (dist > 0.75f)
                    pixels[y * size + x] = border;
                else
                {
                    float t = 1f - (dist / 0.75f) * 0.3f;
                    Color c = color * t;
                    c.a = 1f;
                    pixels[y * size + x] = c;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // Press G to give test items
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItemByID("cannabis", 9);
                InventoryManager.Instance.AddItemByID("cocaine", 5);
                InventoryManager.Instance.AddItemByID("mushrooms", 7);
                InventoryManager.Instance.AddItemByID("opium", 3);
                InventoryManager.Instance.AddMoney(500);
                Debug.Log("Test items added! Press Tab to see inventory.");
            }
        }
    }
}