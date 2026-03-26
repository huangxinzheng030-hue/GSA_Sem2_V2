using UnityEngine;

public class BoardSquare : MonoBehaviour
{
    public string squareName;   // e.g. A1, E4, H8

    [Header("Highlight")]
    public GameObject highlightVisual;   // 子物体，高亮用

    public void SetHighlight(bool isOn)
    {
        if (highlightVisual != null)
            highlightVisual.SetActive(isOn);
    }
}