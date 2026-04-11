using UnityEngine;


using UnityEngine;

[DisallowMultipleComponent]
public class WallJump : MonoBehaviour
{
    [Header("Wall Detect")]
    public LayerMask wallMask;
    public float wallCheckDistance = 0.4f;
    public float wallCheckHeightOffset = 0.2f;   // 从身体中部略偏上发射射线
    public float detachDelay = 0.05f;            // 防止刚跳就立刻又粘回去

    [Header("Cling")]
    public bool freezeOnWall = true;             // 吸附时是否冻结（更“贴墙”）
    public float moveToDetachThreshold = 0.2f;   // 按住WASD超过这个阈值就掉落

    [Header("Wall Jump")]
    public float wallJumpUpForce = 7.5f;
    public float wallJumpOutForce = 5.5f;        // 朝离开墙的方向弹出去
    public float afterJumpNoClingTime = 0.15f;   // 扶墙跳后短时间内不再吸附

    // 给“原本跳跃脚本”用：如果这里处理了空格，就让它别再跳
    public static bool ConsumeJumpThisFrame { get; private set; }

    Rigidbody rb;
    Collider col;

    bool isClinging;
    Vector3 wallNormal; // 指向“离开墙”的方向
    float noClingUntil;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        // 每帧先清空“吃掉跳跃”的标记
        ConsumeJumpThisFrame = false;

        // WASD 输入：有输入就掉落
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool hasMoveInput = new Vector2(h, v).sqrMagnitude >= (moveToDetachThreshold * moveToDetachThreshold);

        // 空格扶墙跳（优先）
        if (isClinging && Input.GetKeyDown(KeyCode.Space))
        {
            DoWallJump();
            ConsumeJumpThisFrame = true; // 告诉其他脚本：这一帧空格已经被我用了
            return;
        }

        // 按住 WASD -> 掉落
        if (isClinging && hasMoveInput)
        {
            DetachFromWall();
        }
    }

    void FixedUpdate()
    {
        if (Time.time < noClingUntil) return;

        // 仅在“空中下落/接近静止”时尝试吸附（你也可以删掉这个限制）
        if (!isClinging && rb.linearVelocity.y <= 0.5f)
        {
            if (CheckWall(out wallNormal))
            {
                AttachToWall(wallNormal);
            }
        }
    }

    bool CheckWall(out Vector3 outNormal)
    {
        outNormal = Vector3.zero;

        Vector3 origin = col.bounds.center + Vector3.up * wallCheckHeightOffset;

        Vector3[] dirs =
        {
        transform.forward,
        -transform.forward,
        transform.right,
        -transform.right
    };

        float dist = wallCheckDistance;

        foreach (var dir in dirs)
        {
            // ✅ 永远画出来（Scene 里才能看到）
            Debug.DrawRay(origin, dir * dist, Color.red);

            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, wallMask, QueryTriggerInteraction.Ignore))
            {
                outNormal = hit.normal;
                return true;
            }
        }

        return false;
    }

    void AttachToWall(Vector3 normal)
    {
        isClinging = true;
        wallNormal = normal;

        // 停住并取消重力，形成“吸在墙上”
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;

        if (freezeOnWall)
        {
            // 冻结位置/旋转（更稳更“粘”）
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void DetachFromWall()
    {
        isClinging = false;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        // 如果你需要保留角色原本的旋转冻结（比如 FreezeRotation），可以在这里改成你自己的约束
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // 防止刚掉下来立刻又检测到墙再次吸住
        noClingUntil = Time.time + detachDelay;
    }

    void DoWallJump()
    {
        // 先解除吸附
        isClinging = false;

        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;

        // 清一下速度，避免“原本速度 + 新加力”叠得太怪
        rb.linearVelocity = Vector3.zero;

        // 组合力：向上 + 向离墙方向
        Vector3 jumpVel = (Vector3.up * wallJumpUpForce) + (wallNormal * wallJumpOutForce);
        rb.AddForce(jumpVel, ForceMode.VelocityChange);

        // 扶墙跳后短时间不吸回去
        noClingUntil = Time.time + afterJumpNoClingTime;

        // 同样把旋转冻结（按你的角色需求调整）
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
