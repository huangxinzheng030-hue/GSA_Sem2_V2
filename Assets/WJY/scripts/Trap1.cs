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

        if (disableOnTrigger != null)
        {
            foreach (var mb in disableOnTrigger)
            {
                if (mb != null) mb.enabled = false;
            }
        }

        // 关键：立刻冻结玩家本体
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

        if (trapUI != null) trapUI.SetActive(true);

        if (unlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = showCursor;
        }
    }

    public void ReleasePlayer()
    {
        if (disableOnTrigger != null)
        {
            foreach (var mb in disableOnTrigger)
            {
                if (mb != null) mb.enabled = true;
            }
        }

        if (playerRb != null)
        {
            playerRb.isKinematic = false;
            playerRb.useGravity = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (trapUI != null) trapUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}