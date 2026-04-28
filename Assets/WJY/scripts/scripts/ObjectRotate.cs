using UnityEngine;

[DisallowMultipleComponent]
public class ObjectRotate : MonoBehaviour
{
    public enum RotateDirection
    {
        Clockwise,
        CounterClockwise
    }

    public enum RotateMode
    {
        NormalRotate,
        Swing
    }

    [Header("Trigger")]
    public string playerTag = "Player";

    [Header("Target")]
    public Transform target;

    [Header("Pivot (Local To Target)")]
    public Vector3 pivotLocalOffset = Vector3.zero;

    [Header("Mode")]
    public RotateMode rotateMode = RotateMode.NormalRotate;

    [Header("Normal Rotation")]
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 90f;
    public bool rotateOnlyOnce = false;
    public float maxRotateAngle = 90f;
    public bool limitAngle = false;

    [Header("Rotation Direction")]
    public RotateDirection rotateDirection = RotateDirection.Clockwise;

    [Header("Swing Mode")]
    [Tooltip("默认关闭。勾上后，不需要玩家进入 Trigger，游戏一开始就自动来回摆动。")]
    public bool startSwingOnAwake = false;

    [Tooltip("摆动最大角度。例如 45 表示从 -45 到 +45 来回摆。")]
    public float swingAngle = 45f;

    [Tooltip("摆动速度。数值越大，摆得越快。")]
    public float swingSpeed = 1f;

    [Tooltip("摆动中心是否使用物体当前角度。一般保持勾选。")]
    public bool useCurrentRotationAsSwingCenter = true;

    [Header("Trigger Mode")]
    public bool rotateWhileInside = true;
    public bool toggleRotateOnEnter = false;

    [Header("Auto Start Normal Rotate")]
    [Tooltip("默认关闭。勾上后，不需要玩家进入 Trigger，游戏开始时就自动单向旋转。")]
    public bool startRotatingOnAwake = false;

    [Tooltip("如果勾上自动开始，是否仍然保留 Trigger 触发功能。一般可以不勾。")]
    public bool keepTriggerAfterAutoStart = false;

    [Header("Debug View")]
    public float gizmoSphereSize = 0.08f;
    public float gizmoAxisLength = 0.6f;

    private bool isPlayerInside = false;
    private bool isRotating = false;
    private bool hasTriggered = false;
    private float rotatedAngle = 0f;

    private Quaternion startLocalRotation;
    private Quaternion swingCenterLocalRotation;
    private float swingTimer = 0f;

    private Vector3 PivotWorldPosition
    {
        get
        {
            if (target == null) return transform.position;
            return target.TransformPoint(pivotLocalOffset);
        }
    }

    private Vector3 AxisWorldDirection
    {
        get
        {
            if (target == null) return Vector3.up;

            Vector3 dir = target.TransformDirection(rotationAxis.normalized);
            if (dir == Vector3.zero) dir = Vector3.up;

            return dir.normalized;
        }
    }

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        if (target == null)
        {
            target = transform;
        }

        startLocalRotation = target.localRotation;
        swingCenterLocalRotation = useCurrentRotationAsSwingCenter ? target.localRotation : Quaternion.identity;

        if (rotateMode == RotateMode.Swing && startSwingOnAwake)
        {
            isRotating = true;
            hasTriggered = true;
        }

        if (rotateMode == RotateMode.NormalRotate && startRotatingOnAwake)
        {
            isRotating = true;
            hasTriggered = true;
        }
    }

    private void Update()
    {
        if (target == null) return;

        bool shouldRotate = false;

        if (rotateWhileInside && isPlayerInside)
        {
            shouldRotate = true;
        }

        if (toggleRotateOnEnter && isRotating)
        {
            shouldRotate = true;
        }

        if (!toggleRotateOnEnter && isRotating)
        {
            shouldRotate = true;
        }

        if (!shouldRotate) return;

        if (rotateMode == RotateMode.Swing)
        {
            UpdateSwing();
        }
        else
        {
            UpdateNormalRotate();
        }
    }

    private void UpdateNormalRotate()
    {
        if (rotateOnlyOnce && hasTriggered && !limitAngle && !startRotatingOnAwake)
        {
            return;
        }

        float directionSign = rotateDirection == RotateDirection.Clockwise ? 1f : -1f;

        float step = rotationSpeed * Time.deltaTime;

        if (limitAngle)
        {
            float remain = maxRotateAngle - rotatedAngle;

            if (remain <= 0f)
            {
                isRotating = false;
                hasTriggered = true;
                return;
            }

            step = Mathf.Min(step, remain);
            rotatedAngle += step;

            if (rotatedAngle >= maxRotateAngle)
            {
                hasTriggered = true;

                if (rotateOnlyOnce || startRotatingOnAwake)
                {
                    isRotating = false;
                }
            }
        }

        target.RotateAround(PivotWorldPosition, AxisWorldDirection, step * directionSign);
    }

    private void UpdateSwing()
    {
        swingTimer += Time.deltaTime * swingSpeed;

        float angle = Mathf.Sin(swingTimer) * swingAngle;

        if (rotateDirection == RotateDirection.CounterClockwise)
        {
            angle = -angle;
        }

        Quaternion offsetRotation = Quaternion.AngleAxis(angle, rotationAxis.normalized);

        target.localRotation = swingCenterLocalRotation * offsetRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((startRotatingOnAwake || startSwingOnAwake) && !keepTriggerAfterAutoStart) return;

        if (!other.CompareTag(playerTag)) return;
        if (rotateOnlyOnce && hasTriggered) return;

        isPlayerInside = true;

        if (toggleRotateOnEnter)
        {
            isRotating = !isRotating;
        }
        else
        {
            isRotating = true;
        }

        hasTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if ((startRotatingOnAwake || startSwingOnAwake) && !keepTriggerAfterAutoStart) return;

        if (!other.CompareTag(playerTag)) return;

        isPlayerInside = false;

        if (rotateWhileInside && !toggleRotateOnEnter)
        {
            isRotating = false;
        }
    }

    private void OnDrawGizmos()
    {
        DrawPivotGizmo(false);
    }

    private void OnDrawGizmosSelected()
    {
        DrawPivotGizmo(true);
    }

    private void DrawPivotGizmo(bool selected)
    {
        Vector3 pivot = PivotWorldPosition;
        Vector3 axis = AxisWorldDirection;

        Gizmos.color = selected ? Color.yellow : Color.cyan;
        Gizmos.DrawSphere(pivot, gizmoSphereSize);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(pivot, pivot + axis * gizmoAxisLength);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, pivot);
    }
}