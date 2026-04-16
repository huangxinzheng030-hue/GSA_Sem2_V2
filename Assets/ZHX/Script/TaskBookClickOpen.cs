using UnityEngine;
using UnityEngine.EventSystems;

public class TaskBookClickOpen : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public GameObject taskRoot;
    public GameObject glowObject;

    [Header("Book")]
    public GameObject bookAnchor;

    public void Start()
    {
        if (glowObject != null)
        {
            glowObject.SetActive(false);
        }

        if (bookAnchor != null)
        {
            bookAnchor.SetActive(false);
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
        if (taskRoot != null)
        {
            taskRoot.SetActive(false);
        }

        if (bookAnchor != null)
        {
            bookAnchor.SetActive(true);
        }
    }
}