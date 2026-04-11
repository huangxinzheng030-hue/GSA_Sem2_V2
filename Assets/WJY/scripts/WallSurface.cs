using UnityEngine;

[DisallowMultipleComponent]
public class WallSurface : MonoBehaviour
{
    [Header("Check")]
    public string playerTag = "Player";

    [Header("Attach")]
    public bool freezeOnWall = true;
    public bool detachOnMove = true;
    public float detachDelay = 0.05f;
    public float minVerticalSpeedToAttach = 0.5f;

    [Header("Wall Jump")]
    public bool allowWallJump = true;
    public float wallJumpUpForce = 7.5f;
    public float wallJumpOutForce = 5.5f;
    public float afterJumpNoClingTime = 0.15f;

    [Header("Extra Direction")]
    public bool useLocalDirection = true;
    public Vector3 extraJumpDirection = Vector3.zero;

    [Header("Override Direction")]
    public bool overrideJumpDirection = false;
    public Vector3 customJumpDirection = new Vector3(0, 1, 1);
    public float customJumpForce = 8f;

    void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag(playerTag)) return;

        PlayerWallReceiver receiver = collision.gameObject.GetComponent<PlayerWallReceiver>();
        if (receiver == null) return;
        if (!receiver.CanAttachToWall()) return;
        if (receiver.rb == null) return;

        // 只在玩家接近静止/下落时允许吸墙，避免乱吸
        if (receiver.rb.linearVelocity.y > minVerticalSpeedToAttach) return;

        // 从接触点法线里找“墙面朝外方向”
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 normal = contact.normal;

            // 排除地面/天花板，只保留近似垂直墙面
            if (Mathf.Abs(normal.y) < 0.3f)
            {
                receiver.AttachToWall(this, normal);
                return;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 origin = transform.position;
        Vector3 dir;

        if (overrideJumpDirection)
        {
            dir = useLocalDirection
                ? transform.TransformDirection(customJumpDirection.normalized) * 2f
                : customJumpDirection.normalized * 2f;
        }
        else
        {
            Vector3 extra = useLocalDirection
                ? transform.TransformDirection(extraJumpDirection)
                : extraJumpDirection;

            dir = (Vector3.up * wallJumpUpForce + transform.forward * wallJumpOutForce + extra).normalized * 2f;
        }

        Gizmos.DrawRay(origin, dir);
    }
}