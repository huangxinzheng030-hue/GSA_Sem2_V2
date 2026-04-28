using UnityEngine;

[DisallowMultipleComponent]
public class WallJump : MonoBehaviour
{
    [Header("Wall Detect")]
    public LayerMask wallMask;
    public float wallCheckDistance = 0.4f;
    public float wallCheckHeightOffset = 0.2f;
    public float detachDelay = 0.05f;

    [Header("Cling")]
    public bool freezeOnWall = true;
    public float moveToDetachThreshold = 0.2f;

    [Header("Wall Jump")]
    public float wallJumpUpForce = 7.5f;
    public float wallJumpOutForce = 5.5f;
    public float afterJumpNoClingTime = 0.15f;

    [Header("Wall Jump Audio")]
    public AudioSource wallJumpAudioSource;
    public AudioClip wallJumpClip;
    [Range(0f, 1f)]
    public float wallJumpVolume = 1f;

    public static bool ConsumeJumpThisFrame { get; private set; }

    Rigidbody rb;
    Collider col;

    bool isClinging;
    Vector3 wallNormal;
    float noClingUntil;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (wallJumpAudioSource == null)
        {
            wallJumpAudioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        ConsumeJumpThisFrame = false;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool hasMoveInput = new Vector2(h, v).sqrMagnitude >= (moveToDetachThreshold * moveToDetachThreshold);

        if (isClinging && Input.GetKeyDown(KeyCode.Space))
        {
            DoWallJump();
            ConsumeJumpThisFrame = true;
            return;
        }

        if (isClinging && hasMoveInput)
        {
            DetachFromWall();
        }
    }

    void FixedUpdate()
    {
        if (Time.time < noClingUntil) return;

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

        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;

        if (freezeOnWall)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void DetachFromWall()
    {
        isClinging = false;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        noClingUntil = Time.time + detachDelay;
    }

    void DoWallJump()
    {
        isClinging = false;

        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;

        Vector3 jumpVel = (Vector3.up * wallJumpUpForce) + (wallNormal * wallJumpOutForce);
        rb.AddForce(jumpVel, ForceMode.VelocityChange);

        PlayWallJumpSound();

        noClingUntil = Time.time + afterJumpNoClingTime;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void PlayWallJumpSound()
    {
        if (wallJumpAudioSource == null) return;
        if (wallJumpClip == null) return;

        wallJumpAudioSource.PlayOneShot(wallJumpClip, wallJumpVolume);
    }
}