using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotbarSlotDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("References")]
    public PlayerInventory inventory;
    public int slotIndex;
    public Image iconImage;

    private static int dragFromIndex = -1;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventory == null || iconImage == null) return;

        ToolItem tool = inventory.GetSlot(slotIndex);
        if (tool == null) return;

        dragFromIndex = slotIndex;

        // 原图标变灰
        Color c = iconImage.color;
        c.a = 0.3f;
        iconImage.color = c;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ❌ 什么都不做（不再跟随鼠标）
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventory == null) return;
        if (dragFromIndex < 0) return;

        inventory.SwapSlots(dragFromIndex, slotIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (iconImage != null)
        {
            Color c = iconImage.color;
            c.a = 1f;
            iconImage.color = c;
        }

        dragFromIndex = -1;
    }
}