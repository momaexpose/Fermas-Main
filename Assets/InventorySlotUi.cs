using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image iconImage;
    public Text amountText;
    public Image backgroundImage;

    public Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color filledColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);

    [HideInInspector] public Inventory inventory;
    [HideInInspector] public int slotIndex;

    private static InventorySlotUI draggedSlot;
    private static GameObject draggedIcon;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(Inventory inv, int index)
    {
        inventory = inv;
        slotIndex = index;
        Refresh();
    }

    public void Refresh()
    {
        if (inventory == null) return;

        ItemStack stack = inventory.GetSlot(slotIndex);

        if (stack == null || stack.IsEmpty())
        {
            if (iconImage != null) iconImage.enabled = false;
            if (amountText != null) amountText.text = "";
            if (backgroundImage != null) backgroundImage.color = emptyColor;
        }
        else
        {
            if (iconImage != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = stack.item.icon;
            }
            if (amountText != null)
                amountText.text = stack.amount > 1 ? stack.amount.ToString() : "";
            if (backgroundImage != null)
                backgroundImage.color = filledColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventory == null) return;

        ItemStack stack = inventory.GetSlot(slotIndex);
        if (stack == null || stack.IsEmpty()) return;

        draggedSlot = this;

        draggedIcon = new GameObject("DragIcon");
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            draggedIcon.transform.SetParent(canvas.transform);

        Image img = draggedIcon.AddComponent<Image>();
        img.sprite = stack.item.icon;
        img.raycastTarget = false;

        RectTransform rect = draggedIcon.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(50, 50);

        canvasGroup.alpha = 0.5f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
            draggedIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon);
            draggedIcon = null;
        }

        canvasGroup.alpha = 1f;
        draggedSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this) return;
        if (inventory == null || draggedSlot.inventory == null) return;

        ItemStack sourceStack = draggedSlot.inventory.GetSlot(draggedSlot.slotIndex);
        ItemStack targetStack = inventory.GetSlot(slotIndex);

        if (sourceStack == null || sourceStack.IsEmpty()) return;

        if (!targetStack.IsEmpty() && targetStack.item == sourceStack.item && !targetStack.IsFull())
        {
            int toMove = sourceStack.amount;
            int remaining = targetStack.Add(toMove);
            sourceStack.amount = remaining;

            if (sourceStack.amount <= 0)
                sourceStack.item = null;
        }
        else if (targetStack.IsEmpty())
        {
            inventory.SetSlot(slotIndex, sourceStack.Clone());
            draggedSlot.inventory.SetSlot(draggedSlot.slotIndex, new ItemStack(null, 0));
        }
        else
        {
            inventory.SetSlot(slotIndex, sourceStack.Clone());
            draggedSlot.inventory.SetSlot(draggedSlot.slotIndex, targetStack.Clone());
        }

        Refresh();
        draggedSlot.Refresh();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.NotifyChange();
    }
}