using UnityEngine;
using TMPro;

public class NewGamePanel : MonoBehaviour
{
    public TMP_InputField nameInput;
    public GameObject confirmDeletePanel;
    int selectedSlot;

    public void SelectSlot(int slot)
    {
        selectedSlot = slot;

        if (SaveManager.Instance.SlotExists(slot))
            confirmDeletePanel.SetActive(true);
        else
            CreateNew();
    }

    public void ConfirmDelete(bool yes)
    {
        confirmDeletePanel.SetActive(false);
        if (yes) CreateNew();
        else FindObjectOfType<MainMenuManager>().OpenMain();
    }

    void CreateNew()
    {
        SaveManager.Instance.StartNewGame(
            selectedSlot,
            nameInput.text
        );
    }
}
