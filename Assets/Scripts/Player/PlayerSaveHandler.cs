using UnityEngine;

public class PlayerSaveHandler : MonoBehaviour
{
    void Start()
    {
        if (SaveManager.Instance != null &&
            SaveManager.Instance.currentSlot != -1)
        {
            SaveManager.Instance.ApplyLoadedData();
        }
    }
}
