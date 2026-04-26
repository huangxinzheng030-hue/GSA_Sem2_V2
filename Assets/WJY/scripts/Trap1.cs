using System.Collections;
using UnityEngine;

public class Trap1 : MonoBehaviour
{
    [Header("UI")]
    public GameObject trapUI;
    public bool unlockCursor = true;
    public bool showCursor = true;

    [Header("Lock Player Control")]
    public MonoBehaviour[] disableOnTrigger;
    public bool onlyTriggerOnce = true;

    [Header("Freeze Player Physics")]
    public CharacterController playerController;
    public Rigidbody playerRb;

    [Header("Respawn Anti Re-trigger")]
    [Tooltip("传送回 checkpoint 后，短时间内忽略所有 Trap1，防止刚传送/刚恢复碰撞时再次触发陷阱")]
    public float ignoreTrapAfterRespawnTime = 0.5f;

    private bool triggered = false;

    // 全局陷阱冷却：防止多个 Trap1 共用一个 Panel 时，传送瞬间又被别的 Trap1 触发
    private static bool globalIgnoreTrap = false;

    // 记录当前真正触发 UI 的那个陷阱
    private static Trap1 currentTrap;

    void Start()
    {
        if (trapUI != null)
            trapUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (globalIgnoreTrap) return;
        if (triggered && onlyTriggerOnce) return;
        if (!other.CompareTag("Player")) return;

        TriggerTrap();
    }

    private void TriggerTrap()
    {
        triggered = true;
        currentTrap = this;

        if (disableOnTrigger != null)
        {
            foreach (var mb in disableOnTrigger)
            {
                if (mb != null)
                    mb.enabled = false;
            }
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.useGravity = false;
            playerRb.isKinematic = true;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (trapUI != null)
            trapUI.SetActive(true);

        if (unlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = showCursor;
        }
    }

    /// <summary>
    /// 这个函数给 UI Button 调用。
    /// 不要再让按钮同时调用 GameController2.RespawnPlayer 和 Trap1.ReleasePlayer。
    /// </summary>
    public void RespawnAndReleasePlayer()
    {
        Trap1 trapToHandle = currentTrap != null ? currentTrap : this;
        trapToHandle.StartCoroutine(trapToHandle.RespawnAndReleaseRoutine());
    }

    private IEnumerator RespawnAndReleaseRoutine()
    {
        globalIgnoreTrap = true;

        // 先关 UI，避免你看到 UI 关了又弹
        if (trapUI != null)
            trapUI.SetActive(false);

        // 传送前继续保持玩家控制器关闭
        if (playerController != null)
            playerController.enabled = false;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true;
            playerRb.useGravity = false;
        }

        // 传送回最近 checkpoint
        if (GameController2.Instance != null)
        {
            GameController2.Instance.TeleportPlayerToRespawn();
        }
        else
        {
            Debug.LogWarning("GameController2.Instance is null, cannot respawn player.");
        }

        // 强制刷新 Transform / Physics 状态
        Physics.SyncTransforms();

        // 等一帧，避免同一帧内关闭 UI、传送、恢复碰撞导致 Trigger 重复判定
        yield return null;

        ReleasePlayer();

        // 再等一小段时间，防止玩家刚落到 checkpoint 还在某个陷阱 trigger 边缘
        yield return new WaitForSeconds(ignoreTrapAfterRespawnTime);

        globalIgnoreTrap = false;
    }

    /// <summary>
    /// 恢复玩家控制。
    /// 一般不要让按钮直接调用这个，按钮应该调用 RespawnAndReleasePlayer。
    /// </summary>
    public void ReleasePlayer()
    {
        if (disableOnTrigger != null)
        {
            foreach (var mb in disableOnTrigger)
            {
                if (mb != null)
                    mb.enabled = true;
            }
        }

        if (playerRb != null)
        {
            playerRb.isKinematic = false;
            playerRb.useGravity = true;
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (trapUI != null)
            trapUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}