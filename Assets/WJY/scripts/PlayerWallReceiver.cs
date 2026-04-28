using UnityEngine;

[DisallowMultipleComponent]
public class PlayerWallReceiver : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody rb;

    [Header("State")]
    public bool onWall;
    public WallSurface currentWall;

    [Header("Timing")]
    public float reattachBlockTime = 0.15f;

    [Header("Wall Jump Audio")]
    public AudioSource wallJumpAudioSource;
    public AudioClip wallJumpClip;
    [Range(0f, 1f)]
    public float wallJumpVolume = 1f;

    [Header("Debug")]
    public bool enableDebugLog = true;

    public static bool ConsumeJumpThisFrame { get; private set; }

    float noAttachUntil;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        if (wallJumpAudioSource == null)
            wallJumpAudioSource = GetComponent<AudioSource>();

        if (enableDebugLog)
        {
            Debug.Log($"[PlayerWallReceiver:{name}] Awake | rb={(rb != null ? rb.name : "NULL")}");
        }
    }

    void Update()
    {
        ConsumeJumpThisFrame = false;

        if (currentWall == null) return;

        if (enableDebugLog && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(
                $"[PlayerWallReceiver:{name}] Space detected while on wall | currentWall={currentWall.name} | velocity(before)={rb.linearVelocity}"
            );
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpFromWall();
            ConsumeJumpThisFrame = true;

            if (enableDebugLog)
            {
                Debug.Log($"[PlayerWallReceiver:{name}] ConsumeJumpThisFrame = TRUE");
            }
        }
    }

    void FixedUpdate()
    {
        if (currentWall == null) return;

        if (currentWall.stickToWall)
        {
            Vector3 before = rb.linearVelocity;
            Vector3 v = before;
            v *= Mathf.Clamp01(1f - currentWall.stickDamping * Time.fixedDeltaTime);
            rb.linearVelocity = v;

            if (enableDebugLog)
            {
                Debug.Log(
                    $"[PlayerWallReceiver:{name}] Stick damping | before={before} | after={rb.linearVelocity} | damping={currentWall.stickDamping}"
                );
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        TryAttachWall(collision, "Enter");
    }

    void OnCollisionStay(Collision collision)
    {
        TryAttachWall(collision, "Stay");
    }

    void TryAttachWall(Collision collision, string phase)
    {
        if (Time.time < noAttachUntil)
        {
            if (enableDebugLog)
            {
                Debug.Log(
                    $"[PlayerWallReceiver:{name}] TryAttachWall blocked by noAttachUntil | now={Time.time:F3} | noAttachUntil={noAttachUntil:F3}"
                );
            }
            return;
        }

        WallSurface wall =
            collision.collider.GetComponent<WallSurface>()
            ?? collision.collider.GetComponentInParent<WallSurface>();

        if (wall == null) return;

        if (!CompareTag(wall.playerTag))
        {
            if (enableDebugLog)
            {
                Debug.Log(
                    $"[PlayerWallReceiver:{name}] Collision with wall but tag mismatch | myTag={tag} | requiredTag={wall.playerTag}"
                );
            }
            return;
        }

        if (currentWall == wall) return;

        currentWall = wall;
        onWall = true;

        if (wall.disableGravityOnWall)
            rb.useGravity = false;

        if (enableDebugLog)
        {
            Debug.Log(
                $"[PlayerWallReceiver:{name}] Attached to wall | phase={phase} | wall={wall.name} | disableGravity={wall.disableGravityOnWall} | velocity={rb.linearVelocity}"
            );
        }
    }

    void OnCollisionExit(Collision collision)
    {
        WallSurface wall =
            collision.collider.GetComponent<WallSurface>()
            ?? collision.collider.GetComponentInParent<WallSurface>();

        if (wall == null) return;
        if (wall != currentWall) return;

        if (enableDebugLog)
        {
            Debug.Log($"[PlayerWallReceiver:{name}] OnCollisionExit from wall={wall.name}");
        }

        LeaveWall();
    }

    void JumpFromWall()
    {
        if (currentWall == null)
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"[PlayerWallReceiver:{name}] JumpFromWall called but currentWall is NULL");
            }
            return;
        }

        Vector3 force = currentWall.GetJumpForceWorld();
        Vector3 beforeVel = rb.linearVelocity;

        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(force, ForceMode.VelocityChange);

        PlayWallJumpSound();

        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.NotifyWallJump();
        }

        Vector3 afterVel = rb.linearVelocity;

        if (enableDebugLog)
        {
            Debug.Log(
                $"[PlayerWallReceiver:{name}] JumpFromWall EXECUTED | wall={currentWall.name} | force={force} | velocity(before reset)={beforeVel} | velocity(after addforce)={afterVel}"
            );
        }

        noAttachUntil = Time.time + reattachBlockTime;

        if (enableDebugLog)
        {
            Debug.Log(
                $"[PlayerWallReceiver:{name}] noAttachUntil set | now={Time.time:F3} | blockUntil={noAttachUntil:F3}"
            );
        }

        LeaveWall();
    }

    void PlayWallJumpSound()
    {
        if (wallJumpAudioSource == null) return;
        if (wallJumpClip == null) return;

        wallJumpAudioSource.PlayOneShot(wallJumpClip, wallJumpVolume);
    }

    void LeaveWall()
    {
        if (enableDebugLog)
        {
            Debug.Log(
                $"[PlayerWallReceiver:{name}] LeaveWall | oldWall={(currentWall != null ? currentWall.name : "NULL")} | velocity={rb.linearVelocity}"
            );
        }

        onWall = false;
        currentWall = null;
        rb.useGravity = true;
    }
}