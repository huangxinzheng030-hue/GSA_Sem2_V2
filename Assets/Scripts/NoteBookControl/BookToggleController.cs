using UnityEngine;

public class BookToggleController : MonoBehaviour
{
    [Header("Book")]
    public GameObject bookAnchor;   // 书本根物体
    public KeyCode toggleKey = KeyCode.T;

    [Header("Cursor")]
    public bool unlockCursorWhenOpen = true;

    private bool isBookOpen = false;

    // 记录这次是不是从 logo 点开的
    private bool openedFromLogo = false;
    private GameObject currentLogoRoot;

    private void Start()
    {
        if (bookAnchor != null)
        {
            bookAnchor.SetActive(false);
        }

        SetCursorState(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isBookOpen)
            {
                CloseBook();
            }
            else
            {
                OpenBook();
            }
        }
    }

    // 普通打开（按T）
    public void OpenBook()
    {
        if (bookAnchor == null)
        {
            Debug.LogWarning("BookToggleController: bookAnchor 没有绑定。");
            return;
        }

        bookAnchor.SetActive(true);
        isBookOpen = true;

        SetCursorState(true);
    }

    // 从logo点击打开
    public void OpenFromLogo(GameObject logoRoot)
    {
        currentLogoRoot = logoRoot;
        openedFromLogo = true;

        if (currentLogoRoot != null)
        {
            currentLogoRoot.SetActive(false);
        }

        OpenBook();
    }

    public void CloseBook()
    {
        if (bookAnchor != null)
        {
            bookAnchor.SetActive(false);
        }

        isBookOpen = false;

        // 如果这次是从logo点开的，关闭时让logo重新出现
        if (openedFromLogo && currentLogoRoot != null)
        {
            currentLogoRoot.SetActive(true);
        }

        currentLogoRoot = null;
        openedFromLogo = false;

        SetCursorState(false);
    }

    private void SetCursorState(bool bookOpen)
    {
        if (!unlockCursorWhenOpen) return;

        if (bookOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public bool IsBookOpen()
    {
        return isBookOpen;
    }
}