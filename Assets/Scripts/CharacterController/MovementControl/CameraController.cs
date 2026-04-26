using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensX;
    public float sensY;

    [Header("Refs")]
    public Transform playerRoot;
    public Transform orientation;

    float xRotation;
    float yRotation;

    private void Start()
    {
        if (playerRoot == null)
        {
            playerRoot = orientation != null ? orientation.root : transform.root;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            ToggleCursor();
        }

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        if (playerRoot == null || orientation == null)
            return;

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        /*
         * 核心修改：
         * 不再使用 Quaternion.Euler(x, y, 0) 这种世界 Y 轴逻辑。
         * 而是让 orientation 继承玩家当前的 up 方向。
         */

        Quaternion yawRotation = Quaternion.AngleAxis(yRotation, playerRoot.up);

        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(playerRoot.forward, playerRoot.up).normalized;
        if (forwardOnPlane == Vector3.zero)
            forwardOnPlane = Vector3.ProjectOnPlane(transform.forward, playerRoot.up).normalized;

        Quaternion baseRotation = Quaternion.LookRotation(forwardOnPlane, playerRoot.up);

        orientation.rotation = yawRotation * baseRotation;

        transform.rotation =
            Quaternion.AngleAxis(xRotation, orientation.right) * orientation.rotation;
    }

    private void ToggleCursor()
    {
        bool nowVisible = !Cursor.visible;
        Cursor.visible = nowVisible;
        Cursor.lockState = nowVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}