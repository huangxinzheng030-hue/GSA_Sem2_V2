using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public PlayerInventory inventory;
    public Image[] slotImages;          // 9个
    public Image[] slotHighlights;      // 9个（用于高亮，可用半透明框）

    private void Start()
    {
        if (inventory != null)
            inventory.OnChanged += Refresh;

        Refresh();
    }

    public void Refresh()
    {
        if (inventory == null || slotImages == null) return;

        for (int i = 0; i < slotImages.Length; i++)
        {
            var tool = inventory.GetSlot(i);
            var img = slotImages[i];

            if (tool != null && tool.data != null && tool.data.icon != null)
            {
                img.enabled = true;
                img.sprite = tool.data.icon;
            }
            else
            {
                img.enabled = false;
                img.sprite = null;
            }

            if (slotHighlights != null && i < slotHighlights.Length && slotHighlights[i] != null)
                slotHighlights[i].enabled = (i == inventory.SelectedIndex);
        }
    }
}