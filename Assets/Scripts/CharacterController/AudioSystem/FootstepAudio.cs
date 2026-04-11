using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform playerVisual; // 没有就留空
    public PlayerMovement playerMovement; // 如果你有自己的移动脚本可拖进来

    [Header("Settings")]
    public float stepInterval = 0.45f;
    public float minMoveThreshold = 0.1f;

    private float stepTimer;

    private void Update()
    {
        bool isMoving = IsMoving();
        bool grounded = IsGrounded();

        if (!isMoving || !grounded)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            stepTimer = 0f;

            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlayFootstep();
            }
        }
    }

    private bool IsMoving()
    {
        if (controller != null)
        {
            Vector3 horizontal = controller.velocity;
            horizontal.y = 0f;
            return horizontal.magnitude > minMoveThreshold;
        }

        if (playerMovement != null)
        {
            // 如果你之后想接你自己的状态变量，可以在这里扩展
            return true;
        }

        return false;
    }

    private bool IsGrounded()
    {
        if (controller != null)
            return controller.isGrounded;

        return true;
    }
}