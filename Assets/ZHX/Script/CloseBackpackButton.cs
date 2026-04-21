using UnityEngine;

public class CloseBackpackButton : MonoBehaviour
{
    [Header("Backpack UI")]
    public GameObject backpackCanvas;

    [Header("Backpack Logo")]
    public GameObject backpackLogo;

    public void CloseBackpack()
    {
        if (backpackCanvas != null)
        {
            backpackCanvas.SetActive(false);
        }

        if (backpackLogo != null)
        {
            backpackLogo.SetActive(true);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}