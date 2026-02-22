using UnityEngine;

public class XRotation : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    public Transform cameraPivot;  // 把相机拖进来

    private float mouseX;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
    }
}
