using UnityEngine;

public enum HoldPointType
{
    ToolHoldPoint,
    HoldPoint
}

[CreateAssetMenu(menuName = "Game/Tool Data")]
public class ToolData : ScriptableObject
{
    public string toolId;
    public Sprite icon;

    [Header("Hold Target")]
    public HoldPointType holdPointType = HoldPointType.ToolHoldPoint;

    [Header("Hold Pose (Local)")]
    public Vector3 holdLocalPosition = Vector3.zero;
    public Vector3 holdLocalEuler = Vector3.zero;
}