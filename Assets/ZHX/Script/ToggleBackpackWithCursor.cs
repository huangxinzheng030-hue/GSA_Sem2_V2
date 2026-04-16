using UnityEngine;

public class ToggleBackpackWithCursor : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Backpack UI")]
    public GameObject backpackCanvas;

    [Header("Backpack Logo")]
    public GameObject backpackLogo;

    [Header("Cursor")]
    public bool showCursorWhenOpen = true;

    void Start()
    {
        if (backpackCanvas != null)
        {
            backpackCanvas.SetActive(false);
        }

        if (backpackLogo != null)
        {
            backpackLogo.SetActive(true);
        }

        HideCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleBackpack();
        }
    }

    public void ToggleBackpack()
    {
        if (backpackCanvas == null) return;

        bool willOpen = !backpackCanvas.activeSelf;
        backpackCanvas.SetActive(willOpen);

        if (backpackLogo != null)
        {
            backpackLogo.SetActive(!willOpen);
        }

        if (showCursorWhenOpen)
        {
            if (willOpen)
            {
                ShowCursor();
            }
            else
            {
                HideCursor();
            }
        }
    }

    void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}