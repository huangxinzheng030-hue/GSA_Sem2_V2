using UnityEngine;

public class ToolPickup : MonoBehaviour
{
    public ToolType toolType;

    [Header("Hold Pose (Local)")]
    public Vector3 holdLocalPosition = Vector3.zero;
    public Vector3 holdLocalEuler = Vector3.zero;

    [Header("Options")]
    public bool disableCollidersWhenOwned = true; // 工具放到手里后禁用碰撞器（你想穿墙就 true）
}
