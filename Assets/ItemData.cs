using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "Item";
    public string itemID;
    public Sprite icon;
    public int maxStack = 9;
    public int baseValue = 100;

    [TextArea]
    public string description;
}