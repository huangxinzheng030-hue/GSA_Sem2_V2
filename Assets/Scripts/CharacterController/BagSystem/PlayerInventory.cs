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

        // G 丢弃当前选中工具
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropSelectedTool();
        }
    }

    public ToolItem GetSlot(int index) => (index >= 0 && index < slotCount) ? slots[index] : null;

    public bool AddTool(ToolItem tool)
    {
        if (tool == null) return false;

        int empty = -1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) { empty = i; break; }
        }
        if (empty == -1) return false;

        slots[empty] = tool;

        StoreTool(tool);

        // ✅ 只在“当前选中槽位就是这个空位”时，才自动装备（更像MC）
        if (equipped == null && SelectedIndex == empty)
        {
            SelectSlot(empty);
        }

        Debug.Log($"AddTool: {tool.name}, data={(tool.data ? tool.data.name : "NULL")}, icon={(tool.data && tool.data.icon ? tool.data.icon.name : "NULL")}");
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

        tool.gameObject.SetActive(true);
        tool.transform.SetParent(toolHoldPoint, false);

        // 获取或添加 LockToHoldPoint
        var lockComp = tool.GetComponent<LockToHoldPoint>();
        if (lockComp == null)
        {
            lockComp = tool.gameObject.AddComponent<LockToHoldPoint>();
        }

        // 启用锁定
        lockComp.enabled = true;
        lockComp.holdPoint = toolHoldPoint;
        lockComp.localPosOffset = Vector3.zero;
        lockComp.localEulerOffset = Vector3.zero;

        // 立即贴到手上
        tool.transform.localPosition = Vector3.zero;
        tool.transform.localRotation = Quaternion.identity;
        //tool.transform.localScale = Vector3.one;

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
        var lockComp = tool.GetComponent<LockToHoldPoint>();
        if (lockComp != null)
        {
            lockComp.enabled = false;   // ✅ 只禁用
            lockComp.holdPoint = null;
        }

        tool.transform.SetParent(null, true);

        foreach (var rb in tool.Rbs)
        {
            if (rb == null) continue;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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

    public void DropSelectedTool()
    {
        ToolItem tool = slots[SelectedIndex];
        if (tool == null) return;

        slots[SelectedIndex] = null;

        // 若它正在装备，清掉
        if (equipped == tool) equipped = null;

        // ✅ 关闭锁定（不销毁）
        var lockComp = tool.GetComponent<LockToHoldPoint>();
        if (lockComp != null)
        {
            lockComp.enabled = false;
            lockComp.holdPoint = null;
        }

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

        // ✅ 不要再 EquipFromSlot(SelectedIndex) —— 否则会让你觉得“扔不掉”
    }

    public void SwapSlots(int a, int b)
    {
        if (a < 0 || a >= slotCount || b < 0 || b >= slotCount) return;
        if (a == b) return;

        var tmp = slots[a];
        slots[a] = slots[b];
        slots[b] = tmp;

        // 如果当前选中槽位被交换，需要保持“手上装备”正确
        // 简单做法：重新装备当前 SelectedIndex
        EquipFromSlot(SelectedIndex);

        OnChanged?.Invoke();
    }

    public void MoveSlot(int from, int to)
    {
        if (from < 0 || from >= slotCount || to < 0 || to >= slotCount) return;
        if (from == to) return;

        if (slots[to] == null)
        {
            slots[to] = slots[from];
            slots[from] = null;
        }
        else
        {
            // 目标有东西就交换
            var tmp = slots[to];
            slots[to] = slots[from];
            slots[from] = tmp;
        }

        EquipFromSlot(SelectedIndex);
        OnChanged?.Invoke();
    }
}

