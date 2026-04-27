using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class ChangeWallGravity : MonoBehaviour
{
    public enum GravityDirection
    {
        WorldDown,
        WorldUp,
        WorldLeft,
        WorldRight,
        WorldForward,
        WorldBack,
        Custom
    }

    [Header("Interaction")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;
    public bool triggerOnlyOnce = false;

    [Tooltip("玩家进入触发区后，是否需要按键才触发。建议勾上。")]
    public bool requireKeyPress = true;

    [Header("Player")]
    public Transform playerRoot;
    public Rigidbody playerRb;

    [Tooltip("切换重力时临时禁用的脚本，比如 PlayerMovement、MouseLook")]
    public MonoBehaviour[] disableDuringTurn;

    [Header("Gravity")]
    public GravityDirection gravityDirection = GravityDirection.WorldRight;
    public Vector3 customGravityDirection = Vector3.right;
    public float gravityStrength = 9.81f;

    [Header("Player Rotation")]
    public bool rotatePlayerToNewUp = true;

    [Tooltip("0 = 瞬间转过去；0.5 = 半秒转过去")]
    public float rotateDuration = 0.25f;

    public bool clearVelocityOnTurn = true;

    [Header("Reset Option")]
    [Tooltip("勾上后，这个区域会恢复普通地面重力，也就是 WorldDown")]
    public bool resetToNormalGravity = false;

    [Header("Debug")]
    public bool enableDebugLog = true;

    private bool playerInside = false;
    private bool hasTriggered = false;
    private bool isTurning = false;

    private Collider currentPlayerCollider;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Update()
    {
        if (!playerInside) return;
        if (isTurning) return;
        if (triggerOnlyOnce && hasTriggered) return;

        if (!requireKeyPress)
        {
            StartTurn();
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            StartTurn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;
        currentPlayerCollider = other;

        if (playerRoot == null)
            playerRoot = other.transform;

        if (playerRb == null)
            playerRb = other.GetComponent<Rigidbody>();

        if (enableDebugLog)
        {
            Debug.Log("[GravityInteractZone] Player entered. Press " + interactKey + " to switch gravity.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;
        currentPlayerCollider = null;

        if (enableDebugLog)
        {
            Debug.Log("[GravityInteractZone] Player exited.");
        }
    }

    private void StartTurn()
    {
        if (playerRoot == null)
        {
            if (currentPlayerCollider != null)
                playerRoot = currentPlayerCollider.transform;
            else
                return;
        }

        if (playerRb == null)
            playerRb = playerRoot.GetComponent<Rigidbody>();

        hasTriggered = true;

        StartCoroutine(TurnGravityRoutine());
    }

    private IEnumerator TurnGravityRoutine()
    {
        isTurning = true;

        Vector3 newGravityDirection;

        if (resetToNormalGravity)
        {
            newGravityDirection = Vector3.down;
        }
        else
        {
            newGravityDirection = GetGravityDirection().normalized;
        }

        Vector3 newGravity = newGravityDirection * gravityStrength;
        Vector3 newPlayerUp = -newGravityDirection;

        if (enableDebugLog)
        {
            Debug.Log("[GravityInteractZone] New gravity = " + newGravity);
            Debug.Log("[GravityInteractZone] New player up = " + newPlayerUp);
        }

        SetPlayerControl(false);

        if (clearVelocityOnTurn && playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        Quaternion startRotation = playerRoot.rotation;
        Quaternion targetRotation = playerRoot.rotation;

        if (rotatePlayerToNewUp)
        {
            Quaternion alignToNewUp = Quaternion.FromToRotation(playerRoot.up, newPlayerUp);
            targetRotation = alignToNewUp * playerRoot.rotation;
        }

        Physics.gravity = newGravity;

        if (rotatePlayerToNewUp)
        {
            if (rotateDuration > 0f)
            {
                float timer = 0f;

                while (timer < rotateDuration)
                {
                    timer += Time.deltaTime;
                    float t = Mathf.Clamp01(timer / rotateDuration);

                    playerRoot.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

                    yield return null;
                }

                playerRoot.rotation = targetRotation;
            }
            else
            {
                playerRoot.rotation = targetRotation;
            }
        }

        Physics.SyncTransforms();

        yield return null;

        SetPlayerControl(true);

        isTurning = false;
    }

    private Vector3 GetGravityDirection()
    {
        switch (gravityDirection)
        {
            case GravityDirection.WorldDown:
                return Vector3.down;

            case GravityDirection.WorldUp:
                return Vector3.up;

            case GravityDirection.WorldLeft:
                return Vector3.left;

            case GravityDirection.WorldRight:
                return Vector3.right;

            case GravityDirection.WorldForward:
                return Vector3.forward;

            case GravityDirection.WorldBack:
                return Vector3.back;

            case GravityDirection.Custom:
                if (customGravityDirection == Vector3.zero)
                    return Vector3.down;

                return customGravityDirection;

            default:
                return Vector3.down;
        }
    }

    private void SetPlayerControl(bool enabled)
    {
        if (disableDuringTurn == null) return;

        foreach (MonoBehaviour mb in disableDuringTurn)
        {
            if (mb != null)
                mb.enabled = enabled;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 dir;

        if (resetToNormalGravity)
            dir = Vector3.down;
        else
            dir = GetGravityDirection();

        if (dir == Vector3.zero)
            dir = Vector3.down;

        dir.Normalize();

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, dir * 2f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -dir * 1.5f);
    }
}
