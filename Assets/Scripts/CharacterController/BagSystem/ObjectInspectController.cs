using UnityEngine;

public class ObjectInspectController : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;

    [Header("Inspect Settings")]
    public float rotateSpeed = 180f;
    public KeyCode exitInspectKey = KeyCode.Mouse1;

    private bool isInspecting = false;
    private ToolItem currentTool;

    private void Update()
    {
        // 左键进入观察模式（仅 HoldPoint 物体）
        if (Input.GetMouseButtonDown(0))
        {
            if (!isInspecting)
            {
                TryEnterInspectMode();
            }
            else
            {
                ExitInspectMode();
            }
        }

        // 右键退出观察模式
        if (isInspecting && Input.GetKeyDown(exitInspectKey))
        {
            ExitInspectMode();
        }

        // 观察中：鼠标旋转物体
        if (isInspecting && currentTool != null)
        {
            RotateObject();
        }
    }

    private void TryEnterInspectMode()
    {
        if (inventory == null) return;

        ToolItem tool = inventory.GetSlot(inventory.SelectedIndex);
        if (tool == null || tool.data == null) return;
        if (tool.data.holdPointType != HoldPointType.HoldPoint) return;

        currentTool = tool;
        isInspecting = true;

        var lockComp = currentTool.GetComponent<LockToHoldPoint>();
        if (lockComp != null)
        {
            lockComp.lockRotation = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    private void ExitInspectMode()
    {
        if (currentTool != null)
        {
            var lockComp = currentTool.GetComponent<LockToHoldPoint>();
            if (lockComp != null)
            {
                lockComp.lockRotation = true;
            }
        }

        isInspecting = false;
        currentTool = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RotateObject()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 水平绕世界Y轴，垂直绕自身X轴
        currentTool.transform.Rotate(Vector3.up, -mouseX * rotateSpeed * Time.deltaTime, Space.World);
        currentTool.transform.Rotate(Vector3.right, mouseY * rotateSpeed * Time.deltaTime, Space.Self);
    }

    public bool IsInspecting()
    {
        return isInspecting;
    }
}