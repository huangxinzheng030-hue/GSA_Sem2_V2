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

    private bool restoredFromState = false;

    public void Start()
    {
        RestoreFromGameState();
    }
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
    public void SaveToGameState()
    {
        if (GameStateManager.Instance == null) return;

        string[] ids = new string[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            ids[i] = (slots[i] != null && slots[i].data != null)
                ? slots[i].data.toolId
                : "";
        }

        GameStateManager.Instance.SaveInventory(ids, SelectedIndex);
    }

    public void RestoreFromGameState()
    {
        if (restoredFromState) return;
        restoredFromState = true;

        if (GameStateManager.Instance == null || ToolRegistry.Instance == null) return;

        var saved = GameStateManager.Instance.GetInventoryState();
        if (saved == null || saved.slotToolIds == null || saved.slotToolIds.Length == 0) return;

        // 清空现有槽位
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                Destroy(slots[i].gameObject);

            slots[i] = null;
        }

        equipped = null;

        int restoreCount = Mathf.Min(slotCount, saved.slotToolIds.Length);

        for (int i = 0; i < restoreCount; i++)
        {
            string toolId = saved.slotToolIds[i];
            if (string.IsNullOrWhiteSpace(toolId)) continue;

            ToolItem tool = ToolRegistry.Instance.SpawnById(toolId);
            if (tool == null) continue;

            slots[i] = tool;
            StoreTool(tool);

            if (GameStateManager.Instance != null)
                GameStateManager.Instance.UnlockPainting(toolId);
        }

        SelectedIndex = Mathf.Clamp(saved.selectedIndex, 0, slotCount - 1);

        EquipFromSlot(SelectedIndex);
        OnChanged?.Invoke();

        StartCoroutine(RefreshInventoryNextFrame());
    }
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

        if (tool.data != null)
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.UnlockPainting(tool.data.toolId);

            if (PaintingCodexUI.Instance != null)
                PaintingCodexUI.Instance.UnlockPainting(tool.data.toolId);
        }

        SaveToGameState();

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
        SaveToGameState();
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

        SaveToGameState();

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

        SaveToGameState();

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
        SaveToGameState();
        OnChanged?.Invoke();
    }

    public void ForceRefreshInventoryView()
    {
        EquipFromSlot(SelectedIndex);
        OnChanged?.Invoke();
    }
    private System.Collections.IEnumerator RefreshInventoryNextFrame()
    {
        yield return null;

        EquipFromSlot(SelectedIndex);
        OnChanged?.Invoke();
    }
}