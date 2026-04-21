using UnityEngine;

public class CloseUICursorLock : MonoBehaviour
{
    public void LockAndHideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}