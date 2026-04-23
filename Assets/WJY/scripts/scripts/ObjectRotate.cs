using UnityEngine;

[DisallowMultipleComponent]
public class ObjectRotate : MonoBehaviour
{
    [Header("Trigger")]
    public string playerTag = "Player";

    [Header("Target")]
    public Transform target;                  // 要旋转的物体

    [Header("Pivot (Local To This Trigger Object)")]
    public Vector3 pivotLocalOffset = Vector3.zero;   // 旋转点，相对于当前空物体本地坐标

    [Header("Rotation")]
    public Vector3 rotationAxis = Vector3.up;         // 旋转轴
    public float rotationSpeed = 90f;                // 每秒旋转角度
    public bool rotateOnlyOnce = false;              // 是否只触发一次
    public float maxRotateAngle = 90f;               // 最大旋转角度（只在限制角度时有用）
    public bool limitAngle = false;                  // 是否限制最大旋转角度

    [Header("Trigger Mode")]
    public bool rotateWhileInside = true;            // 玩家在触发区内持续旋转
    public bool toggleRotateOnEnter = false;         // 进入一次后切换旋转开关

    [Header("Debug View")]
    public float gizmoSphereSize = 0.08f;
    public float gizmoAxisLength = 0.6f;

    private bool isPlayerInside = false;
    private bool isRotating = false;
    private bool hasTriggered = false;
    private float rotatedAngle = 0f;

    private Vector3 PivotWorldPosition
    {
        get { return transform.TransformPoint(pivotLocalOffset); }
    }

    private Vector3 AxisWorldDirection
    {
        get
        {
            Vector3 dir = transform.TransformDirection(rotationAxis.normalized);
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

        if (!shouldRotate) return;

        if (rotateOnlyOnce && hasTriggered && !limitAngle)
        {
            return;
        }

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
                if (rotateOnlyOnce)
                {
                    isRotating = false;
                }
            }
        }

        target.RotateAround(PivotWorldPosition, AxisWorldDirection, step);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (rotateOnlyOnce && hasTriggered) return;

        isPlayerInside = true;

        if (toggleRotateOnEnter)
        {
            isRotating = !isRotating;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        isPlayerInside = false;
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

        // 从trigger中心连到旋转点，方便看偏移
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, pivot);
    }
}
