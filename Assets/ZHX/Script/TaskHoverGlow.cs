using UnityEngine;
using UnityEngine.EventSystems;

public class TaskHoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject glowObject;

    void Start()
    {
        if (glowObject != null)
        {
            glowObject.SetActive(false);
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
}
