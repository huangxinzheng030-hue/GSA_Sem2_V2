using UnityEngine;
using UnityEngine.EventSystems;

public class TaskBookClickOpen : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public GameObject taskRoot;      // logo根物体
    public GameObject glowObject;    // 鼠标移上去时显示的高亮/发光物体

    [Header("Book Controller")]
    public BookToggleController bookToggleController;

    private void Start()
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (bookToggleController == null)
        {
            Debug.LogWarning("TaskBookClickOpen: 没有绑定 BookToggleController。");
            return;
        }

        bookToggleController.OpenFromLogo(taskRoot);
    }
}