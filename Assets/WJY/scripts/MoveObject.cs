using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MoveObject : MonoBehaviour
{
     public float moveSpeed = 5f;
    public float jumpForce = 6f;

    [Header("Ground Check")]
    public LayerMask groundMask = ~0;
    public float groundCheckExtra = 0.15f; // 允许离地一点点也算在地面上

    private Rigidbody rb;
    private Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            // 清掉向下速度，让跳更稳定
            var v = rb.linearVelocity;
            if (v.y < 0f) v.y = 0f;
            rb.linearVelocity = v;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        Debug.Log(IsGrounded());
    }

    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(x, 0f, z).normalized;
        Vector3 move = transform.TransformDirection(input) * moveSpeed;

        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(move.x, vel.y, move.z);
    }

    bool IsGrounded()
    {
        Bounds b = col.bounds;
        float radius = Mathf.Max(0.05f, Mathf.Min(b.extents.x, b.extents.z) * 0.9f);
        Vector3 origin = new Vector3(b.center.x, b.min.y + 0.02f, b.center.z);

        float castDist = groundCheckExtra;
        return Physics.SphereCast(origin, radius, Vector3.down, out _, castDist, groundMask, QueryTriggerInteraction.Ignore);
    }
    
}
