using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotbarSlotDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Wiring")]
    public PlayerInventory inventory;
    public int slotIndex;

    [Header("UI")]
    public Image iconImage;               // 这个格子的 icon Image
    public Canvas rootCanvas;             // 用于拖拽时跟随鼠标

    private RectTransform iconRect;
    private Transform iconOriginalParent;
    private Vector2 iconOriginalAnchoredPos;

    private static int draggingFromIndex = -1;
    private static HotbarSlotDrag draggingFromSlot = null;

    private void Awake()
    {
        if (iconImage != null) iconRect = iconImage.rectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventory == null || iconImage == null || rootCanvas == null) return;

        // 空槽不允许拖拽
        if (inventory.GetSlot(slotIndex) == null) return;

        draggingFromIndex = slotIndex;
        draggingFromSlot = this;

        iconOriginalParent = iconRect.parent;
        iconOriginalAnchoredPos = iconRect.anchoredPosition;

        // 让 icon 跑到 Canvas 顶层，避免被遮挡
        iconRect.SetParent(rootCanvas.transform, true);
        iconImage.raycastTarget = false; // 关键：让 drop 能被下面的 slot 接到
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingFromSlot != this || iconRect == null) return;

        // 跟随鼠标
        iconRect.position = eventData.position;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventory == null) return;
        if (draggingFromIndex < 0) return;

        int from = draggingFromIndex;
        int to = slotIndex;

        inventory.MoveSlot(from, to);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingFromSlot != this || iconRect == null || iconImage == null) return;

        // icon 放回原 slot
        iconRect.SetParent(iconOriginalParent, true);
        iconRect.anchoredPosition = iconOriginalAnchoredPos;
        iconImage.raycastTarget = true;

        draggingFromIndex = -1;
        draggingFromSlot = null;
    }
}