using UnityEngine;

/// <summary>
/// Tracks player money - uses GameData for persistence
/// </summary>
public class PlayerMoney : MonoBehaviour
{
    public static PlayerMoney Instance;

    public static int Money
    {
        get { return GameData.GetMoney(); }
        private set { GameData.SetMoney(value); }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure GameData exists
        if (GameData.Instance == null)
        {
            GameObject obj = new GameObject("GameData");
            obj.AddComponent<GameData>();
        }

        Debug.Log("[PlayerMoney] Initialized with $" + Money);
    }

    public static void Add(int amount)
    {
        GameData.SetMoney(GameData.GetMoney() + amount);
        Debug.Log("[PlayerMoney] Added $" + amount + " (Total: $" + Money + ")");

        if (MoneyUI.Instance != null)
            MoneyUI.Instance.Refresh();
    }

    public static void Remove(int amount)
    {
        int current = GameData.GetMoney();
        current -= amount;
        if (current < 0) current = 0;
        GameData.SetMoney(current);
        Debug.Log("[PlayerMoney] Removed $" + amount + " (Total: $" + Money + ")");

        if (MoneyUI.Instance != null)
            MoneyUI.Instance.Refresh();
    }

    public static bool CanAfford(int amount)
    {
        return Money >= amount;
    }

    public static void Set(int amount)
    {
        GameData.SetMoney(amount);
        Debug.Log("[PlayerMoney] Set to $" + Money);

        if (MoneyUI.Instance != null)
            MoneyUI.Instance.Refresh();
    }
}