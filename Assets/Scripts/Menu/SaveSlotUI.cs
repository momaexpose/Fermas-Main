using UnityEngine;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex;
    public TextMeshProUGUI slotText;

    void OnEnable()
    {
        if (SaveManager.Instance.SlotExists(slotIndex))
        {
            var data = SaveManager.Instance.LoadSlot(slotIndex);
            slotText.text = data.saveName;
        }
        else
        {
            slotText.text = "Empty Slot";
        }
    }
}
