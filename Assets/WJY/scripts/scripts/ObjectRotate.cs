using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class ObjectRotate : MonoBehaviour
{
    public enum RotateDirection
    {
        Clockwise,
        CounterClockwise
    }

    [Header("Trigger")]
    public string playerTag = "Player";

    [Header("Target")]
    public Transform target;

    [Header("Pivot (Local To Target)")]
    public Vector3 pivotLocalOffset = Vector3.zero;

    [Header("Rotation")]
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 90f;
    public bool rotateOnlyOnce = false;
    public float maxRotateAngle = 90f;
    public bool limitAngle = false;

    [Header("Rotation Direction")]
    public RotateDirection rotateDirection = RotateDirection.Clockwise;

    [Header("Trigger Mode")]
    public bool rotateWhileInside = true;
    public bool toggleRotateOnEnter = false;

    [Header("Delay Trigger")]
    public bool useTriggerDelay = false;
    public float triggerDelay = 1f;

    [Tooltip("勾上后，只要玩家进入过 Trigger，就算之后离开，也会在延迟结束后开始旋转")]
    public bool rotateAfterDelayEvenIfPlayerLeft = true;

    [Header("Debug View")]
    public float gizmoSphereSize = 0.08f;
    public float gizmoAxisLength = 0.6f;

    private bool isPlayerInside = false;
    private bool isRotating = false;
    private bool hasTriggered = false;
    private float rotatedAngle = 0f;

    private Coroutine delayCoroutine;

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

        if (rotateOnlyOnce && hasTriggered && !limitAngle)
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

                if (rotateOnlyOnce)
                {
                    isRotating = false;
                }
            }
        }

        target.RotateAround(PivotWorldPosition, AxisWorldDirection, step * directionSign);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (rotateOnlyOnce && hasTriggered) return;

        isPlayerInside = true;

        if (useTriggerDelay)
        {
            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
            }

            delayCoroutine = StartCoroutine(DelayedTrigger());
        }
        else
        {
            TriggerRotate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        isPlayerInside = false;

        if (useTriggerDelay && !rotateAfterDelayEvenIfPlayerLeft)
        {
            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
                delayCoroutine = null;
            }
        }

        if (rotateWhileInside && !toggleRotateOnEnter && !useTriggerDelay)
        {
            isRotating = false;
        }
    }

    private IEnumerator DelayedTrigger()
    {
        yield return new WaitForSeconds(triggerDelay);

        if (!rotateAfterDelayEvenIfPlayerLeft && !isPlayerInside)
        {
            delayCoroutine = null;
            yield break;
        }

        TriggerRotate();

        delayCoroutine = null;
    }

    private void TriggerRotate()
    {
        if (rotateOnlyOnce && hasTriggered) return;

        if (toggleRotateOnEnter)
        {
            isRotating = !isRotating;
        }
        else
        {
            isRotating = true;
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