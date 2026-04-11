using UnityEngine;

[DisallowMultipleComponent]
public class PlayerWallReceiver : MonoBehaviour
{
    [Header("Player Base")]
    public Rigidbody rb;

    [Header("State")]
    public bool isOnWall = false;

    WallSurface currentWall;
    Vector3 currentWallNormal;
    float noWallAttachUntil = 0f;

    // 给你原本跳跃脚本用
    public static bool ConsumeJumpThisFrame { get; private set; }

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        ConsumeJumpThisFrame = false;

        if (!isOnWall || currentWall == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool hasMoveInput = new Vector2(h, v).sqrMagnitude > 0.01f;

        // 按WASD掉落
        if (currentWall.detachOnMove && hasMoveInput)
        {
            DetachFromWall(currentWall.detachDelay);
            return;
        }

        // 空格扶墙跳
        if (currentWall.allowWallJump && Input.GetKeyDown(KeyCode.Space))
        {
            DoWallJump();
            ConsumeJumpThisFrame = true;
            return;
        }
    }

    public bool CanAttachToWall()
    {
        return Time.time >= noWallAttachUntil;
    }

    public void AttachToWall(WallSurface wall, Vector3 wallNormal)
    {
        if (!CanAttachToWall()) return;
        if (wall == null) return;

        currentWall = wall;
        currentWallNormal = wallNormal;
        isOnWall = true;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;

        if (wall.freezeOnWall)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    public void DetachFromWall(float delay)
    {
        isOnWall = false;
        currentWall = null;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        noWallAttachUntil = Time.time + delay;
    }

    void DoWallJump()
    {
        if (currentWall == null) return;

        WallSurface wall = currentWall;

        isOnWall = false;
        currentWall = null;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearVelocity = Vector3.zero;

        Vector3 jumpDir;

        if (wall.overrideJumpDirection)
        {
            Vector3 customDir = wall.useLocalDirection
                ? wall.transform.TransformDirection(wall.customJumpDirection.normalized)
                : wall.customJumpDirection.normalized;

            jumpDir = customDir * wall.customJumpForce;
        }
        else
        {
            Vector3 extra = wall.useLocalDirection
                ? wall.transform.TransformDirection(wall.extraJumpDirection)
                : wall.extraJumpDirection;

            jumpDir =
                Vector3.up * wall.wallJumpUpForce +
                currentWallNormal * wall.wallJumpOutForce +
                extra;
        }

        rb.AddForce(jumpDir, ForceMode.VelocityChange);

        noWallAttachUntil = Time.time + wall.afterJumpNoClingTime;
    }
}