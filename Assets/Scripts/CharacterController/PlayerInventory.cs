using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Hotbar")]
    public int slotCount = 9;

    [Header("References")]
    public Transform toolHoldPoint;      // Camera 下的 ToolHoldPoint
    public Collider playerCollider;      // Player CapsuleCollider（用于 IgnoreCollision）

    public int SelectedIndex { get; private set; } = 0;

    private ToolItem[] slots;
    private ToolItem equipped;           // 当前手上显示的工具

    public System.Action OnChanged;      // 给UI刷新用

    private void Awake()
    {
        slots = new ToolItem[slotCount];
    }

    public ToolItem GetSlot(int index) => (index >= 0 && index < slotCount) ? slots[index] : null;

    public bool AddTool(ToolItem tool)
    {
        // 找空槽位
        int empty = -1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) { empty = i; break; }
        }
        if (empty == -1) return false;

        slots[empty] = tool;

        StoreTool(tool);

        // 如果当前手上没装备任何工具，就自动切到新工具槽位
        if (equipped == null)
        {
            SelectSlot(empty);
        }

        OnChanged?.Invoke();
        return true;
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        SelectedIndex = index;
        EquipFromSlot(index);
        OnChanged?.Invoke();
    }

    private void EquipFromSlot(int index)
    {
        // 先卸下当前装备
        if (equipped != null)
        {
            // 不丢弃，只是隐藏回“库存状态”
            StoreTool(equipped);
            equipped = null;
        }

        ToolItem tool = slots[index];
        if (tool == null) return;

        equipped = tool;

        // 显示并挂到手上
        tool.gameObject.SetActive(true);
        tool.transform.SetParent(toolHoldPoint, false);

        // 使用 ToolData 的手持姿势
        if (tool.data != null)
        {
            tool.transform.localPosition = tool.data.holdLocalPosition;
            tool.transform.localRotation = Quaternion.Euler(tool.data.holdLocalEuler);
        }
        else
        {
            tool.transform.localPosition = Vector3.zero;
            tool.transform.localRotation = Quaternion.identity;
        }

        // 手持时：不受物理影响
        foreach (var rb in tool.Rbs)
        {
            if (rb == null) continue;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // 避免与玩家碰撞（可选）
        foreach (var c in tool.Cols)
        {
            if (c == null) continue;
            if (playerCollider != null) Physics.IgnoreCollision(c, playerCollider, true);
        }
    }

    private void StoreTool(ToolItem tool)
    {
        // 收纳：隐藏 + 解除父子（避免跟着相机乱跑）
        tool.transform.SetParent(null, true);

        // 收纳时：不需要物理
        foreach (var rb in tool.Rbs)
        {
            if (rb == null) continue;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // collider 可以关掉（像 MC 的“收进背包”）
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

    public void DropSelectedTool()
    {
        ToolItem tool = slots[SelectedIndex];
        if (tool == null) return;

        // 从槽位移除
        slots[SelectedIndex] = null;

        // 若它正在装备，清掉
        if (equipped == tool) equipped = null;

        // 掉落到世界：显示 + 开物理 + 开碰撞 + 解除 IgnoreCollision
        tool.gameObject.SetActive(true);
        tool.transform.SetParent(null, true);

        foreach (var c in tool.Cols)
        {
            if (c == null) continue;
            c.enabled = true;
            if (playerCollider != null) Physics.IgnoreCollision(c, playerCollider, false);
        }

        foreach (var rb in tool.Rbs)
        {
            if (rb == null) continue;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        OnChanged?.Invoke();

        // 掉落后：自动装备该槽位的新工具（如果有）
        EquipFromSlot(SelectedIndex);
    }
}