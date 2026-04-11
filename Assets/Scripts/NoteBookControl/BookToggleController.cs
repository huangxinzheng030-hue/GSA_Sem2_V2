using UnityEngine;

public class BookToggleController : MonoBehaviour
{
    [Header("Book")]
    public GameObject bookRoot;
    public Transform bookAnchor;
    public KeyCode toggleKey = KeyCode.T;

    [Header("Player Control Scripts To Disable")]
    public MonoBehaviour[] scriptsToDisable;

    [Header("Cursor")]
    public bool lockCursorWhenClosed = true;

    private bool isBookOpen = false;

    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    void Start()
    {
        if (bookRoot != null)
        {
            originalParent = bookRoot.transform.parent;
            originalPosition = bookRoot.transform.position;
            originalRotation = bookRoot.transform.rotation;
            originalScale = bookRoot.transform.localScale;

            bookRoot.SetActive(false);
        }

        if (lockCursorWhenClosed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isBookOpen)
                CloseBook();
            else
                OpenBook();
        }
    }

    public void OpenBook()
    {
        if (bookRoot == null || bookAnchor == null) return;

        isBookOpen = true;

        bookRoot.SetActive(true);
        bookRoot.transform.SetParent(bookAnchor);
        bookRoot.transform.localPosition = Vector3.zero;
        bookRoot.transform.localRotation = Quaternion.identity;

        SetPlayerControl(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseBook()
    {
        if (bookRoot == null) return;

        isBookOpen = false;

        bookRoot.transform.SetParent(originalParent);
        bookRoot.transform.position = originalPosition;
        bookRoot.transform.rotation = originalRotation;
        bookRoot.transform.localScale = originalScale;

        bookRoot.SetActive(false);

        SetPlayerControl(true);

        if (lockCursorWhenClosed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void SetPlayerControl(bool enabledState)
    {
        if (scriptsToDisable == null) return;

        foreach (var script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = enabledState;
        }
    }
}