using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [Header("References")]
    public Transform playerRoot;          // 一般拖 Player 自己
    public LayerMask groundLayers;        // 地面层
    public float groundCheckDistance = 0.3f;

    [Header("Movement Check")]
    public float minMoveDistance = 0.01f; // 位移阈值，太小不算走路

    [Header("Step Timing")]
    public float stepInterval = 0.45f;

    private Vector3 lastPosition;
    private float stepTimer;

    private void Start()
    {
        if (playerRoot == null)
            playerRoot = transform;

        lastPosition = playerRoot.position;
    }

    private void Update()
    {
        bool isMoving = IsMoving();
        bool isGrounded = IsGrounded();

        if (!isMoving || !isGrounded)
        {
            stepTimer = 0f;
            lastPosition = playerRoot.position;
            return;
        }

        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            stepTimer = 0f;
            SFXManager.Instance?.PlayFootstep();
        }

        lastPosition = playerRoot.position;
    }

    private bool IsMoving()
    {
        Vector3 currentPos = playerRoot.position;
        Vector3 delta = currentPos - lastPosition;

        // 只判断水平移动，不算上下跳动
        delta.y = 0f;

        return delta.magnitude > minMoveDistance;
    }

    private bool IsGrounded()
    {
        Vector3 origin = playerRoot.position + Vector3.up * 0.05f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayers);
    }
}