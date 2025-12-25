using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string saveName;
    public string sceneName;

    public Vector3 playerPosition;
    public Quaternion playerRotation;

    public float playTime;

    // INVENTORY (IMPLEMENT LATER)
    // public InventoryData inventory;

    // HEALTH / STATS (IMPLEMENT LATER)
    // public float health;
}
