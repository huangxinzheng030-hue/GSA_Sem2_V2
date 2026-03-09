using UnityEngine;

public class ToolItem : MonoBehaviour
{
    public ToolData data;

    [Header("Pickup Behavior")]
    public bool disableCollidersWhenStored = true;

    public Rigidbody[] Rbs { get; private set; }
    public Collider[] Cols { get; private set; }

    private void Awake()
    {
        Rbs = GetComponentsInChildren<Rigidbody>(true);
        Cols = GetComponentsInChildren<Collider>(true);
    }
}