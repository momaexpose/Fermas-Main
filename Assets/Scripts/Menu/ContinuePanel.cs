using UnityEngine;

public class ContinuePanel : MonoBehaviour
{
    public void SelectSlot(int slot)
    {
        if (SaveManager.Instance.SlotExists(slot))
            SaveManager.Instance.LoadGame(slot);
    }
}
