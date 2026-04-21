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

    // 最大跳跃次数（包含地面跳）
    public int maxJumps = 2;
    private int jumpsRemaining;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public LayerMask whatIsGround;
    public float groundedRememberTime = 0.15f;
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

    [Tooltip("下落时额外向下加速度")]
    public float fallGravityMultiplier = 2.5f;

    [Tooltip("上升时松开跳跃键后，额外向下加速度，让跳跃不那么飘")]
    public float lowJumpGravityMultiplier = 2f;

    [Tooltip("最大下落速度，防止越掉越快")]
    public float maxFallSpeed = 35f;

    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsRunning { get; private set; }
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
            Debug.LogWarning("cameraHolder 未被设置！请在 Inspector 中拖拽相机对象。");
        }
    }

    private void Update()
    {
        // 只要最近一小段时间内碰到过地面，就认为 grounded
        grounded = Time.time - lastGroundedTime <= groundedRememberTime;

        // 落地时重置跳跃次数
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

        // Handle Drag
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

        // 普通跳跃：如果墙跳已经消耗了这次空格，就不要再处理
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
                if (enableDebugLog)
                {
                    Debug.Log($"[PlayerMovement:{name}] NORMAL JUMP EXECUTED before decrement | jumpsRemaining={jumpsRemaining}");
                }

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

        // Crouch
        if (Input.GetKey(crouchKey))
        {
            Crouch();
        }
        else
        {
            UnCrouch();
        }

        // Sprint
        if (Input.GetKey(sprintKey) && !isCrouching)
        {
            currentSpeed = sprintSpeed;
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }
    }

    private void MovePlayer()
    {
        if (orientation == null) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        // 墙跳后的短时间内，不限制水平速度
        if (Time.time < wallJumpPreserveUntil)
            return;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > currentSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        if (enableDebugLog)
        {
            Debug.Log($"[PlayerMovement:{name}] Jump() called | velocity(before)={rb.linearVelocity} | jumpForce={jumpForce}");
        }

        // 只清空Y速度，保留水平速度
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

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
            Debug.LogWarning("cameraHolder 为 null，无法恢复相机位置！");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // 只认 whatIsGround 里的层
        if (((1 << collision.gameObject.layer) & whatIsGround) == 0)
            return;

        foreach (ContactPoint contact in collision.contacts)
        {
            // 接触面法线朝上，说明是地面，不是墙
            if (contact.normal.y > 0.3f)
            {
                lastGroundedTime = Time.time;

                if (enableDebugLog)
                {
                    Debug.Log($"[GroundCollision] touching ground object={collision.gameObject.name} | normal={contact.normal}");
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

        Vector3 vel = rb.linearVelocity;

        // 下落时额外加重力
        if (vel.y < 0f)
        {
            rb.AddForce(
                Physics.gravity * (fallGravityMultiplier - 1f),
                ForceMode.Acceleration
            );
        }
        // 上升时，如果已经松开跳跃键，让上升更快结束
        else if (vel.y > 0f && !Input.GetKey(jumpKey))
        {
            rb.AddForce(
                Physics.gravity * (lowJumpGravityMultiplier - 1f),
                ForceMode.Acceleration
            );
        }

        // 限制最大下落速度
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