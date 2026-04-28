using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;
    public float sprintSpeed;
    public float crouchSpeed;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    public int maxJumps = 2;
    private int jumpsRemaining;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public LayerMask whatIsGround;
    public float groundedRememberTime = 0.15f;
    public float groundNormalDotThreshold = 0.3f;

    private bool grounded;
    private float lastGroundedTime;

    [Header("Crouch")]
    public float crouchScale = 0.5f;
    public float crouchYOffset = 0.5f;

    public Transform orientation;
    public Transform cameraHolder;

    [Header("Camera")]
    public float normalCameraY = 0.6f;
    public float crouchCameraY = 0.3f;

    [Header("Debug")]
    public bool enableDebugLog = true;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    private float currentSpeed;
    private bool isCrouching;
    private float normalHeight;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    [Header("Wall Jump Preserve")]
    public float wallJumpPreserveTime = 0.2f;
    private float wallJumpPreserveUntil = 0f;

    [Header("Extra Gravity")]
    public bool useExtraGravity = true;

    [Tooltip("下落时额外重力倍率")]
    public float fallGravityMultiplier = 2.5f;

    [Tooltip("松开跳跃键后，让上升更快结束")]
    public float lowJumpGravityMultiplier = 2f;

    [Tooltip("最大下落速度")]
    public float maxFallSpeed = 35f;

    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsRunning { get; private set; }

    private Vector3 CurrentUp
    {
        get { return transform.up; }
    }

    private Vector3 CurrentDown
    {
        get { return -transform.up; }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;

        if (capsuleCollider != null)
            normalHeight = capsuleCollider.height;

        jumpsRemaining = maxJumps;

        if (cameraHolder != null)
        {
            Vector3 initialPos = cameraHolder.localPosition;
            initialPos.y = normalCameraY;
            cameraHolder.localPosition = initialPos;
        }
        else
        {
            Debug.LogWarning("cameraHolder 没有设置，请在 Inspector 中拖入相机对象。");
        }
    }

    private void Update()
    {
        grounded = Time.time - lastGroundedTime <= groundedRememberTime;
        IsGrounded = grounded;

        if (grounded)
        {
            jumpsRemaining = maxJumps;
        }

        if (enableDebugLog && Input.GetKeyDown(jumpKey))
        {
            Debug.Log(
                $"[GroundState] grounded={grounded} | jumpsRemaining={jumpsRemaining} | velocity={rb.linearVelocity}"
            );
        }

        MyInput();
        SpeedControl();

        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        if (enableDebugLog && Input.GetKeyDown(jumpKey))
        {
            Debug.Log(
                $"[PlayerMovement:{name}] Space detected in Update | grounded={grounded} | jumpsRemaining={jumpsRemaining} | velocity={rb.linearVelocity} | ConsumeJumpThisFrame={PlayerWallReceiver.ConsumeJumpThisFrame}"
            );
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        ApplyExtraGravity();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        MoveInput = new Vector2(horizontalInput, verticalInput);

        if (Input.GetKeyDown(jumpKey))
        {
            if (enableDebugLog)
            {
                Debug.Log(
                    $"[PlayerMovement:{name}] MyInput Space branch entered | jumpsRemaining={jumpsRemaining} | grounded={grounded} | ConsumeJumpThisFrame={PlayerWallReceiver.ConsumeJumpThisFrame}"
                );
            }

            if (PlayerWallReceiver.ConsumeJumpThisFrame)
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[PlayerMovement:{name}] Space ignored because wall jump consumed it");
                }
            }
            else if (jumpsRemaining > 0)
            {
                Jump();
                jumpsRemaining--;

                if (enableDebugLog)
                {
                    Debug.Log($"[PlayerMovement:{name}] NORMAL JUMP finished | jumpsRemaining(after)={jumpsRemaining}");
                }
            }
            else
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[PlayerMovement:{name}] Space pressed but jumpsRemaining <= 0");
                }
            }
        }

        if (Input.GetKey(crouchKey))
        {
            Crouch();
        }
        else
        {
            UnCrouch();
        }

        if (Input.GetKey(sprintKey) && !isCrouching)
        {
            currentSpeed = sprintSpeed;
            IsRunning = true;
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
            IsRunning = false;
        }
        else
        {
            currentSpeed = moveSpeed;
            IsRunning = false;
        }
    }

    private void MovePlayer()
    {
        if (orientation == null) return;

        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(orientation.forward, CurrentUp).normalized;
        Vector3 rightOnPlane = Vector3.ProjectOnPlane(orientation.right, CurrentUp).normalized;

        moveDirection = forwardOnPlane * verticalInput + rightOnPlane * horizontalInput;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        if (grounded)
        {
            rb.AddForce(moveDirection * currentSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection * currentSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        if (Time.time < wallJumpPreserveUntil)
            return;

        Vector3 velocity = rb.linearVelocity;

        Vector3 verticalVel = Vector3.Project(velocity, CurrentUp);
        Vector3 horizontalVel = velocity - verticalVel;

        if (horizontalVel.magnitude > currentSpeed)
        {
            Vector3 limitedHorizontalVel = horizontalVel.normalized * currentSpeed;
            rb.linearVelocity = limitedHorizontalVel + verticalVel;
        }
    }

    private void Jump()
    {
        if (enableDebugLog)
        {
            Debug.Log($"[PlayerMovement:{name}] Jump() called | velocity(before)={rb.linearVelocity} | jumpForce={jumpForce}");
        }

        Vector3 velocity = rb.linearVelocity;

        Vector3 verticalVel = Vector3.Project(velocity, CurrentUp);
        Vector3 horizontalVel = velocity - verticalVel;

        rb.linearVelocity = horizontalVel;
        rb.AddForce(CurrentUp * jumpForce, ForceMode.Impulse);

        if (enableDebugLog)
        {
            Debug.Log($"[PlayerMovement:{name}] Jump() finished | velocity(after)={rb.linearVelocity}");
        }
    }

    private void Crouch()
    {
        if (isCrouching) return;

        isCrouching = true;

        if (capsuleCollider != null)
        {
            capsuleCollider.height = normalHeight * crouchScale;
        }

        if (cameraHolder != null)
        {
            Vector3 newCameraPos = cameraHolder.localPosition;
            newCameraPos.y = crouchCameraY;
            cameraHolder.localPosition = newCameraPos;
        }
    }

    private void UnCrouch()
    {
        if (!isCrouching) return;

        isCrouching = false;

        if (capsuleCollider != null)
        {
            capsuleCollider.height = normalHeight;
        }

        if (cameraHolder != null)
        {
            Vector3 newCameraPos = cameraHolder.localPosition;
            newCameraPos.y = normalCameraY;
            cameraHolder.localPosition = newCameraPos;
        }
        else
        {
            Debug.LogWarning("cameraHolder 为 null，无法恢复相机位置。");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsGround) == 0)
            return;

        foreach (ContactPoint contact in collision.contacts)
        {
            float dot = Vector3.Dot(contact.normal, CurrentUp);

            if (dot > groundNormalDotThreshold)
            {
                lastGroundedTime = Time.time;

                if (enableDebugLog)
                {
                    Debug.Log($"[GroundCollision] touching ground object={collision.gameObject.name} | normal={contact.normal} | dot={dot}");
                }

                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsGround) == 0)
            return;

        if (enableDebugLog)
        {
            Debug.Log($"[GroundCollision] exit ground object={collision.gameObject.name}");
        }
    }

    public void NotifyWallJump()
    {
        wallJumpPreserveUntil = Time.time + wallJumpPreserveTime;
    }

    private void ApplyExtraGravity()
    {
        if (!useExtraGravity) return;
        if (rb == null) return;
        if (rb.isKinematic) return;

        Vector3 velocity = rb.linearVelocity;

        float verticalSpeed = Vector3.Dot(velocity, CurrentUp);

        if (verticalSpeed < 0f)
        {
            rb.AddForce(
                Physics.gravity * (fallGravityMultiplier - 1f),
                ForceMode.Acceleration
            );
        }
        else if (verticalSpeed > 0f && !Input.GetKey(jumpKey))
        {
            rb.AddForce(
                Physics.gravity * (lowJumpGravityMultiplier - 1f),
                ForceMode.Acceleration
            );
        }

        float fallSpeed = Vector3.Dot(rb.linearVelocity, CurrentDown);

        if (fallSpeed > maxFallSpeed)
        {
            Vector3 verticalVel = Vector3.Project(rb.linearVelocity, CurrentDown);
            Vector3 horizontalVel = rb.linearVelocity - verticalVel;

            rb.linearVelocity = horizontalVel + CurrentDown * maxFallSpeed;
        }
    }
}