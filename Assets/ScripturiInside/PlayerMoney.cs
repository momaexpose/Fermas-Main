using UnityEngine;

/// <summary>
/// Tracks player money - persists across scenes
/// </summary>
public class PlayerMoney : MonoBehaviour
{
    public static PlayerMoney Instance;
    public static int Money { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[PlayerMoney] Initialized with $" + Money);
    }

    public static void Add(int amount)
    {
        Money += amount;
        Debug.Log("[PlayerMoney] Added $" + amount + " (Total: $" + Money + ")");

        if (MoneyUI.Instance != null)
            MoneyUI.Instance.Refresh();
    }

    public static void Remove(int amount)
    {
        Money -= amount;
        if (Money < 0) Money = 0;
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
        Money = amount;
        Debug.Log("[PlayerMoney] Set to $" + Money);

        if (MoneyUI.Instance != null)
            MoneyUI.Instance.Refresh();
    }
}