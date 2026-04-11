using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Hotbar")]
    public int slotCount = 9;

    [Header("References")]
    public Transform toolHoldPoint;   // 工具（手电筒）
    public Transform holdPoint;       // ObjectItem（普通物体）
    public Transform throwOrigin;     // 一般拖 Camera
    public Collider playerCollider;

    [Header("Throw Settings")]
    public float throwForce = 8f;
    public float throwUpwardForce = 1.5f;

    public int SelectedIndex { get; private set; } = 0;

    private ToolItem[] slots;
    private ToolItem equipped;

    public System.Action OnChanged;

    private void Awake()
    {
        slots = new ToolItem[slotCount];
    }

    private void Update()
    {
        // 滚轮切换
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            int next = (SelectedIndex - 1 + slotCount) % slotCount;
            SelectSlot(next);
        }
        else if (scroll < 0f)
        {
            int next = (SelectedIndex + 1) % slotCount;
            SelectSlot(next);
        }

        // G 丢弃
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropSelectedTool();
        }

        // 左键抛出（仅 ObjectItem）
        if (Input.GetMouseButtonDown(1) && CanThrowSelectedObject())
        {
            ThrowSelectedTool();
        }
    }

    // =========================
    // 基础功能
    // =========================

    public ToolItem GetSlot(int index)
    {
        return (index >= 0 && index < slotCount) ? slots[index] : null;
    }

    public bool AddTool(ToolItem tool)
    {
        if (tool == null) return false;

        int empty = -1;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                empty = i;
                break;
            }
        }

        if (empty == -1) return false;

        slots[empty] = tool;

        StoreTool(tool);

        if (equipped == null && SelectedIndex == empty)
        {
            SelectSlot(empty);
        }

        OnChanged?.Invoke();
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayPickup();
        }
        return true;
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        SelectedIndex = index;
        EquipFromSlot(index);
        OnChanged?.Invoke();
    }

    // =========================
    // 装备逻辑（核心）
    // =========================

    private void EquipFromSlot(int index)
    {
        if (equipped != null)
        {
            StoreTool(equipped);
            equipped = null;
        }

        ToolItem tool = slots[index];
        if (tool == null) return;

        equipped = tool;

        // 判断挂点
        Transform targetPoint = toolHoldPoint;

        if (tool.data != null && tool.data.holdPointType == HoldPointType.HoldPoint)
        {
            targetPoint = holdPoint;
        }

        if (targetPoint == null)
        {
            Debug.LogWarning($"{tool.name} 没有挂点，回退 toolHoldPoint");
            targetPoint = toolHoldPoint;
        }

        tool.gameObject.SetActive(true);
        tool.transform.SetParent(targetPoint, false);

        var lockComp = tool.GetComponent<LockToHoldPoint>();
        if (lockComp == null)
            lockComp = tool.gameObject.AddComponent<LockToHoldPoint>();

        lockComp.enabled = true;
        lockComp.holdPoint = targetPoint;

        if (tool.data != null)
        {
            lockComp.localPosOffset = tool.data.holdLocalPosition;
            lockComp.localEulerOffset = tool.data.holdLocalEuler;

            tool.transform.localPosition = tool.data.holdLocalPosition;
            tool.transform.localRotation = Quaternion.Euler(tool.data.holdLocalEuler);
        }
        else
        {
            tool.transform.localPosition = Vector3.zero;
            tool.transform.localRotation = Quaternion.identity;
        }

        foreach (var rb in tool.Rbs)
        {
            if (rb == null) continue;

            //rb.linearVelocity = Vector3.zero;
            //rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        foreach (var c in tool.Cols)
        {
            if (c == null) continue;

            if (playerCollider != null)
                Physics.IgnoreCollision(c, playerCollider, true);
        }
    }

    // =========================
    // 收纳
    // =========================

    private void StoreTool(ToolItem tool)
    {
        var lockComp = tool.GetComponent<LockToHoldPoint>();
        if (lockComp != null)
        {
            lockComp.enabled = false;
            lockComp.holdPoint = null;
        }

        tool.transform.SetParent(null, true);

        foreach (var rb in tool.Rbs)
        {
            if (rb == null) continue;

            //rb.linearVelocity = Vector3.zero;
            //rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (tool.disableCollidersWhenStored)
        {
            foreach (var c in tool.Cols)
            {
                if (c == null) continue;
                c.enabled = false;
            }
        }

        tool.gameObject.SetActive(false);
    }

    // =========================
    // 丢弃（G）
    // =========================

    public void DropSelectedTool()
    {
        ToolItem tool = slots[SelectedIndex];
        if (tool == null) return;

        slots[SelectedIndex] = null;

        if (equipped == tool) equipped = null;

        DisableLock(tool);

        tool.gameObject.SetActive(true);
        tool.transform.SetParent(null, true);

        RestorePhysics(tool);

        OnChanged?.Invoke();
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayDrop();
        }
    }

    // =========================
    // 抛出（左键）
    // =========================

    public void ThrowSelectedTool()
    {
        ToolItem tool = slots[SelectedIndex];
        if (tool == null) return;

        slots[SelectedIndex] = null;

        if (equipped == tool) equipped = null;

        DisableLock(tool);

        tool.gameObject.SetActive(true);
        tool.transform.SetParent(null, true);

        RestorePhysics(tool);

        Vector3 dir = (throwOrigin != null) ? throwOrigin.forward : transform.forward;

        foreach (var rb in tool.Rbs)
        {
            if (rb == null) continue;

            rb.AddForce((dir * throwForce) + (Vector3.up * throwUpwardForce), ForceMode.Impulse);
        }

        OnChanged?.Invoke();
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayThrow();
        }
    }

    // =========================
    // 限制：只能抛 ObjectItem
    // =========================

    private bool CanThrowSelectedObject()
    {
        ToolItem tool = slots[SelectedIndex];
        if (tool == null) return false;
        if (tool.data == null) return false;

        return tool.data.holdPointType == HoldPointType.HoldPoint;
    }

    // =========================
    // 工具方法
    // =========================

    private void DisableLock(ToolItem tool)
    {
        var lockComp = tool.GetComponent<LockToHoldPoint>();
        if (lockComp != null)
        {
            lockComp.enabled = false;
            lockComp.holdPoint = null;
        }
    }

    private void RestorePhysics(ToolItem tool)
    {
        foreach (var c in tool.Cols)
        {
            if (c == null) continue;

            c.enabled = true;

            if (playerCollider != null)
                Physics.IgnoreCollision(c, playerCollider, false);
        }

        foreach (var rb in tool.Rbs)
        {
            if (rb == null) continue;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    internal void MoveSlot(int from, int to)
    {
        throw new NotImplementedException();
    }
    public void SwapSlots(int a, int b)
    {
        if (a < 0 || a >= slotCount || b < 0 || b >= slotCount) return;
        if (a == b) return;

        ToolItem temp = slots[a];
        slots[a] = slots[b];
        slots[b] = temp;

        EquipFromSlot(SelectedIndex);
        OnChanged?.Invoke();
    }
}