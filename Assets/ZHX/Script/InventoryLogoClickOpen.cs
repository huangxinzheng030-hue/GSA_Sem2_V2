using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryLogoClickOpen : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Logo UI")]
    public GameObject logoRoot;
    public GameObject glowObject;

    [Header("Open Target")]
    public GameObject targetCanvas;

    void Start()
    {
        if (glowObject != null)
        {
            glowObject.SetActive(false);
        }

        if (targetCanvas != null)
        {
            targetCanvas.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (glowObject != null)
        {
            glowObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (glowObject != null)
        {
            glowObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (logoRoot != null)
        {
            logoRoot.SetActive(false);
        }

        if (targetCanvas != null)
        {
            targetCanvas.SetActive(true);
        }
    }
}