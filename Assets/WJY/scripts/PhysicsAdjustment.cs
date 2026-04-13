using UnityEngine;

[DisallowMultipleComponent]

public class PhysicsAdjustment : MonoBehaviour
{

    [Header("Refs")]
    public Rigidbody rb;
    public CapsuleCollider capsule;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckOffset = 0.05f;
    public float groundCheckRadius = 0.25f;

    [Header("Gravity")]
    [Tooltip("额外向下加速度。0=不用额外重力")]
    public float extraGravity = 20f;

    [Tooltip("最大下落速度，防止无限加速")]
    public float maxFallSpeed = 35f;

    [Tooltip("松开跳跃键后是否更快下落")]
    public bool useLowJumpCut = false;

    [Tooltip("松开跳跃键后额外下拉")]
    public float lowJumpExtraGravity = 12f;

    [Header("Braking")]
    [Tooltip("地面无输入时的水平刹车")]
    public float groundBrake = 18f;

    [Tooltip("空中无输入时的水平刹车")]
    public float airBrake = 2f;

    [Tooltip("地面有输入时额外阻尼（一般保持0）")]
    public float groundMoveDamping = 0f;

    [Header("Debug")]
    public bool showDebug = false;

    private bool grounded;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (capsule == null) capsule = GetComponent<CapsuleCollider>();
    }

    void FixedUpdate()
    {
        if (rb == null || capsule == null) return;

        grounded = CheckGrounded();

        ApplyExtraGravity();
        ApplyHorizontalBraking();
        ClampFallSpeed();
    }

    bool CheckGrounded()
    {
        Bounds b = capsule.bounds;

        Vector3 checkPos = new Vector3(
            b.center.x,
            b.min.y + groundCheckOffset,
            b.center.z
        );

        bool hit = Physics.CheckSphere(
            checkPos,
            groundCheckRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        if (showDebug)
        {
            Debug.DrawLine(b.center, checkPos, hit ? Color.green : Color.red);
        }

        return hit;
    }

    void ApplyExtraGravity()
    {
        // 下落时额外向下加速度
        if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }

        // 可选：松开空格后让上升更快结束
        if (useLowJumpCut && rb.linearVelocity.y > 0f && !Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.down * lowJumpExtraGravity, ForceMode.Acceleration);
        }
    }

    void ApplyHorizontalBraking()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool hasInput = new Vector2(h, v).sqrMagnitude > 0.01f;

        Vector3 flat = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (!hasInput)
        {
            float brake = grounded ? groundBrake : airBrake;
            flat = Vector3.Lerp(flat, Vector3.zero, brake * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(flat.x, rb.linearVelocity.y, flat.z);
        }
        else if (grounded && groundMoveDamping > 0f)
        {
            flat = Vector3.Lerp(flat, flat * 0.98f, groundMoveDamping * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(flat.x, rb.linearVelocity.y, flat.z);
        }
    }

    void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                -maxFallSpeed,
                rb.linearVelocity.z
            );
        }
    }
}
