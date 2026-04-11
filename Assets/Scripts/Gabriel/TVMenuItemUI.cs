using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TVMenuItemUI : MonoBehaviour
{
    [Header("References")]
    public Image highlight;
    public TextMeshProUGUI label;

    [Header("Colors")]
    public Color normalTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color selectedTextColor = Color.black;

    public void SetSelected(bool selected)
    {
        if (highlight != null)
            highlight.enabled = selected;

        if (label != null)
            label.color = selected ? selectedTextColor : normalTextColor;
    }

    public void SetText(string text)
    {
        if (label != null)
            label.text = text;
    }
}