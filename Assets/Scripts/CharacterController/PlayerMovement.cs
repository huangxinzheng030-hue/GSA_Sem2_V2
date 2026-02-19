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

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Crouch")]
    public float crouchScale = 0.5f;
    public float crouchYOffset = 0.5f;
    
    public Transform orientation;
    public Transform cameraHolder;
    
    [Header("Camera")]
    public float normalCameraY = 0.6f;
    public float crouchCameraY = 0.3f;

    // 新增：可配置的最大跳跃次数（包含地面跳），例如 2 表示双跳
    [Header("Jump")]
    public int maxJumps = 2;
    private int jumpsRemaining;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    
    float currentSpeed;
    bool isCrouching;
    float normalHeight;

    Rigidbody rb;
    CapsuleCollider capsuleCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        
        capsuleCollider = GetComponent<CapsuleCollider>();
        normalHeight = capsuleCollider.height;
        
        // 初始化跳跃次数
        jumpsRemaining = maxJumps;
        
        if (cameraHolder != null)
        {
            // 设置相机初始位置为固定的 Y 值
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
        // Ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        // 落地时重置可用跳跃次数
        if (grounded)
        {
            jumpsRemaining = maxJumps;
        }

        MyInput();
        SpeedControl();
        // Handle Drag
        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }

    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jumping - 使用 Input.GetKeyDown，允许空中二段跳（只要 jumpsRemaining>0）
        if (Input.GetKeyDown(jumpKey) && jumpsRemaining > 0)
        {
            Jump();
            jumpsRemaining--;
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
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // On Ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);
        // In Air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
         Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if(flatVel.magnitude > currentSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

       private void Jump()
        {
            // 重置垂直速度后施加瞬时力
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    
        private void ResetJump()
        {
            readyToJump = true;
        }

        private void Crouch()
        {
            if (isCrouching) return;
            
            isCrouching = true;
            
            // 调整碰撞体高度
            if (capsuleCollider != null)
            {
                capsuleCollider.height = normalHeight * crouchScale;
            }
            
            // 调整相机位置为蹲伏Y值
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
            
            // 恢复碰撞体高度
            if (capsuleCollider != null)
            {
                capsuleCollider.height = normalHeight;
            }
            
            // 恢复相机位置为原始Y值
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
}

