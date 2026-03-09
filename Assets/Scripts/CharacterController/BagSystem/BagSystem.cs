using UnityEngine;

public class BagSystem : MonoBehaviour
{
    public GameObject myBag;
    bool isBagOpen = false;

    void Start()
    {
        if (myBag != null)
            myBag.SetActive(false);

        // 启动时锁定鼠标并隐藏，确保相机可旋转
        SetCursorLocked(true);
    }

    void Update()
    {
        // 每帧都要检查按键以便在已解锁鼠标时也能关闭背包
        OpenBag();
    }

    // 根据参数锁定/解锁鼠标并设置可见性
    void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void OpenBag()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isBagOpen = !isBagOpen;
            if (myBag != null)
                myBag.SetActive(isBagOpen);

            if (isBagOpen)
            {
                // 打开背包：显示鼠标并解锁（CameraController 会检测 lockState 并禁用转向）
                SetCursorLocked(false);
                Debug.Log("Bag Opened");
            }
            else
            {
                // 关闭背包：锁定并隐藏鼠标，恢复转向

            }
        }
    }

    public void CloseBag()
    {
        SetCursorLocked(true);
        Debug.Log("Bag Closed");
        isBagOpen = false;
    }
}
