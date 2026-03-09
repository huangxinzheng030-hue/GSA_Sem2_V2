using UnityEngine;

[CreateAssetMenu(menuName = "Game/Tool Data")]
public class ToolData : ScriptableObject
{
    public string toolId;                 // Î¨Ò»ID£¬ÀýÈç "flashlight"
    public Sprite icon;

    [Header("Hold Pose (Local, relative to ToolHoldPoint)")]
    public Vector3 holdLocalPosition = Vector3.zero;
    public Vector3 holdLocalEuler = Vector3.zero;
}