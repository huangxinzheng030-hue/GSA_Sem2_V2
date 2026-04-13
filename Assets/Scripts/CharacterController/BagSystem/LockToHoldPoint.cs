using UnityEngine;

public class LockToHoldPoint : MonoBehaviour
{
    public Transform holdPoint;
    public Vector3 localPosOffset = Vector3.zero;
    public Vector3 localEulerOffset = Vector3.zero;

    public bool lockRotation = true;

    private void LateUpdate()
    {
        if (holdPoint == null) return;

        if (transform.parent != holdPoint)
            transform.SetParent(holdPoint, false);

        transform.localPosition = localPosOffset;

        if (lockRotation)
        {
            transform.localRotation = Quaternion.Euler(localEulerOffset);
        }
    }
}