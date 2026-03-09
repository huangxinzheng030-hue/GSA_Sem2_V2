using UnityEngine;

public class LockToHoldPoint : MonoBehaviour
{
    public Transform holdPoint;
    public bool lockPosition = true;
    public bool lockRotation = true;
    //public bool lockScale = true;

    // 允许你保留一个固定偏移（如果需要）
    public Vector3 localPosOffset = Vector3.zero;
    public Vector3 localEulerOffset = Vector3.zero;

    void LateUpdate()
    {
        if (holdPoint == null) return;

        // 确保父级正确
        if (transform.parent != holdPoint)
            transform.SetParent(holdPoint, false);

        if (lockPosition) transform.localPosition = localPosOffset;
        if (lockRotation) transform.localRotation = Quaternion.Euler(localEulerOffset);
        //if (lockScale) transform.localScale = Vector3.one;
    }
}