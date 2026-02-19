using UnityEngine;

public class Trap1 : MonoBehaviour
{
    [Header("UI")]
    public GameObject trapUI;              // 要弹出的UI面板（Panel）
    public bool unlockCursor = true;       // 是否解锁鼠标给UI用
    public bool showCursor = true;

    [Header("Lock Player Control")]
    public MonoBehaviour[] disableOnTrigger; // 把“移动脚本/视角脚本”等拖进来，触发时禁用它们
    public bool onlyTriggerOnce = true;      // 是否只触发一次

    private bool triggered = false;

    void Start()
    {
        if (trapUI != null) trapUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered && onlyTriggerOnce) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // 1) 禁用控制脚本（锁死视角/移动）
        if (disableOnTrigger != null)
        {
            foreach (var mb in disableOnTrigger)
            {
                if (mb != null) mb.enabled = false;
            }
        }

        // 2) 弹出UI
        if (trapUI != null) trapUI.SetActive(true);

        // 3) 鼠标设置（让你能点UI）
        if (unlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = showCursor;
        }
    }

     // 如果你需要“关闭UI后恢复控制”，可以在UI按钮里调用这个方法
    public void ReleasePlayer()
    {
        // 恢复脚本
        if (disableOnTrigger != null)
        {
            foreach (var mb in disableOnTrigger)
            {
                if (mb != null) mb.enabled = true;
            }
        }

        // 关UI
        if (trapUI != null) trapUI.SetActive(false);

        // 重新锁鼠标回第一人称
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}
