using UnityEngine;

[DisallowMultipleComponent]
public class WallSurface : MonoBehaviour
{
    [Header("Player Check")]
    public string playerTag = "Player";

    [Header("Stick")]
    public bool stickToWall = true;
    public bool disableGravityOnWall = true;
    public float stickDamping = 12f;

    [Header("Jump Force")]
    public bool useLocalDirection = true;

    [Tooltip("弹射方向。比如 (0,1,1) = 向上+向前")]
    public Vector3 jumpDirection = new Vector3(0, 1, 1);

    [Tooltip("弹射力度")]
    public float jumpForce = 8f;

    [Header("Reattach Delay")]
    public float reattachDelay = 0.15f;

    [Header("Debug")]
    public bool enableDebugLog = true;

    public Vector3 GetJumpForceWorld()
    {
        Vector3 dir = jumpDirection.normalized;
        if (useLocalDirection)
            dir = transform.TransformDirection(dir);

        Vector3 result = dir * jumpForce;

        if (enableDebugLog)
        {
            Debug.Log(
                $"[WallSurface:{name}] GetJumpForceWorld | useLocal={useLocalDirection} | jumpDirection={jumpDirection} | normalizedDir={dir} | jumpForce={jumpForce} | resultForce={result}"
            );
        }

        return result;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 origin = transform.position;
        Vector3 force = GetJumpForceWorld();

        Gizmos.DrawRay(origin, force.normalized * 2f);

        Vector3 tip = origin + force.normalized * 2f;
        Vector3 right = Quaternion.LookRotation(force.normalized) * Quaternion.Euler(0, 160, 0) * Vector3.forward * 0.3f;
        Vector3 left = Quaternion.LookRotation(force.normalized) * Quaternion.Euler(0, 200, 0) * Vector3.forward * 0.3f;

        Gizmos.DrawRay(tip, right);
        Gizmos.DrawRay(tip, left);
    }
}