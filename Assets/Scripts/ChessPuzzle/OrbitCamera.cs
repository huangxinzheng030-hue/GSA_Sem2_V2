using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;              // 棋盘中心点
    public Vector3 targetOffset = Vector3.zero;

    [Header("Rotation")]
    public float rotateSpeedX = 180f;     // 水平旋转速度
    public float rotateSpeedY = 120f;     // 垂直旋转速度
    public float minVerticalAngle = 15f;  // 最低俯角
    public float maxVerticalAngle = 80f;  // 最高俯角

    [Header("Zoom")]
    public float distance = 10f;          // 当前距离
    public float minDistance = 4f;
    public float maxDistance = 18f;
    public float zoomSpeed = 5f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.08f;
    public float rotationSmoothSpeed = 10f;

    private float yaw;
    private float pitch = 35f;
    private Vector3 currentVelocity;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        if (target != null)
        {
            UpdateCamera(true);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleRotationInput();
        HandleZoomInput();
        UpdateCamera(false);
    }

    private void HandleRotationInput()
    {
        // 按住鼠标右键旋转
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotateSpeedX * Time.deltaTime;
            pitch -= mouseY * rotateSpeedY * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        }
    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    private void UpdateCamera(bool instant)
    {
        Vector3 focusPoint = target.position + targetOffset;

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = focusPoint - targetRotation * Vector3.forward * distance;

        if (instant)
        {
            transform.position = desiredPosition;
            transform.rotation = targetRotation;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                positionSmoothTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
        }
    }
}