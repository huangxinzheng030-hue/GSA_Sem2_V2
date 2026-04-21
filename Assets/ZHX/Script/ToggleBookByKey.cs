using UnityEngine;

public class ToggleBookWithCursor : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.T;

    [Header("Book UI")]
    public GameObject bookObject;

    [Header("Book Logo")]
    public GameObject bookLogoObject;

    [Header("Cursor Settings")]
    public bool showCursorWhenBookOpen = true;

    void Start()
    {
        if (bookObject != null)
        {
            bookObject.SetActive(false);
        }

        if (bookLogoObject != null)
        {
            bookLogoObject.SetActive(true);
        }

        HideCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleBook();
        }
    }

    public void ToggleBook()
    {
        if (bookObject == null) return;

        bool willOpen = !bookObject.activeSelf;
        bookObject.SetActive(willOpen);

        if (bookLogoObject != null)
        {
            bookLogoObject.SetActive(!willOpen);
        }

        if (showCursorWhenBookOpen)
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