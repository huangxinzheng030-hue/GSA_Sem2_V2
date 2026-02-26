using System.Collections.Generic;
using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform holdPoint;        // 普通物体手持点（可和工具点共用）
    public Transform toolHoldPoint;    // 工具装备点（建议单独一个，位置更好调）
    public Collider playerCollider;    // Player 的 CapsuleCollider

    [Header("Ray Settings")]
    public float interactDistance = 3f;
    public LayerMask interactLayer = ~0;
    public string selectableTag = "Pickup"; // 用于高亮筛选（可选）

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.Q;
    public int throwMouseButton = 0;

    [Header("Hold (Normal Item)")]
    public float holdSmoothSpeed = 25f;
    public bool keepUpright = false;

    [Header("Throw (Normal Item)")]
    public float throwForce = 8f;
    public float throwUpForce = 1.2f;

    [Header("Tool Input")]
    public KeyCode slot1Key = KeyCode.Alpha1;      // Flashlight
    public KeyCode slot2Key = KeyCode.Alpha2;      // Crowbar
    public KeyCode flashlightToggleKey = KeyCode.F;
    public int crowbarUseMouseButton = 0;
    public KeyCode throwToolKey = KeyCode.G;
    public float toolThrowForce = 8f;
    public float toolThrowUpForce = 1.2f;

    [Header("Highlight")]
    public Material highlightMaterial;

    // ----- Normal held item -----
    private Rigidbody heldRb;
    private Collider heldCollider;
    private float cooldown;

    // ----- Tools owned -----
    private readonly Dictionary<ToolType, ToolPickup> ownedTools = new();
    private ToolType? equippedTool = null;
    private ToolPickup currentEquippedTool;
    // ----- Highlight state -----
    private Transform currentHighlightRoot;
    private readonly Dictionary<Renderer, Material[]> savedSharedMats = new();

    // ----- Inventory reference (for UI) -----
    public PlayerInventory inventory;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) Debug.LogWarning("PickupSystem: cam 未设置且 Camera.main 为空。");
        if (highlightMaterial == null) Debug.LogWarning("PickupSystem: highlightMaterial 未设置，高亮将不生效。");
        if (toolHoldPoint == null) Debug.LogWarning("PickupSystem: toolHoldPoint 未设置（工具拾取将无法放到手上）。");
    }

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;

        UpdateSelection();

        // 统一交互键：优先拾取工具，其次普通物体；若已拿普通物体则 E=放下
        if (Input.GetKeyDown(interactKey))
        {
            if (heldRb != null)
            {
                DropNormal();
            }
            else
            {
                InteractTryPickupToolOrNormal();
            }
        }

        if (Input.GetKeyDown(throwToolKey))
            ThrowEquippedTool();
        // 普通物体：放下 / 扔出
        if (Input.GetKeyDown(dropKey) && heldRb != null)
            DropNormal();

        if (Input.GetMouseButtonDown(throwMouseButton) && heldRb != null)
            ThrowNormal();

        // 工具：切换与使用
        if (Input.GetKeyDown(slot1Key)) EquipTool(ToolType.Flashlight);
        if (Input.GetKeyDown(slot2Key)) EquipTool(ToolType.Crowbar);

        if (Input.GetKeyDown(flashlightToggleKey) && equippedTool == ToolType.Flashlight)
            ToggleFlashlight();

        if (Input.GetMouseButtonDown(crowbarUseMouseButton) && equippedTool == ToolType.Crowbar)
            UseCrowbar();

        // 1~9 切换（示例：只写1、2，你可以扩到9）
        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.SelectSlot(1);
        // ...Alpha9 => SelectSlot(8)

        // G 丢弃当前槽位工具（自然下落）
        if (Input.GetKeyDown(KeyCode.G)) inventory.DropSelectedTool();
    }


    void FixedUpdate()
    {
        if (heldRb == null) return;

        // 你喜欢的穿墙方案：kinematic + parent
        if (heldRb.isKinematic)
        {
            if (heldRb.transform.parent == holdPoint)
            {
                heldRb.transform.localPosition = Vector3.zero;
                heldRb.transform.localRotation = Quaternion.identity;
            }
            else
            {
                heldRb.transform.position = holdPoint.position;
                heldRb.transform.rotation = holdPoint.rotation;
            }
            return;
        }

        // 备用：物理平滑（一般用不到）
        Vector3 targetPos = holdPoint.position;
        Vector3 newPos = Vector3.Lerp(heldRb.position, targetPos, holdSmoothSpeed * Time.fixedDeltaTime);
        heldRb.MovePosition(newPos);

        Quaternion targetRot = keepUpright
            ? Quaternion.LookRotation(cam.transform.forward, Vector3.up)
            : holdPoint.rotation;

        Quaternion newRot = Quaternion.Slerp(heldRb.rotation, targetRot, holdSmoothSpeed * Time.fixedDeltaTime);
        heldRb.MoveRotation(newRot);
    }

    // -------------------- Core Interact --------------------

    void InteractTryPickupToolOrNormal()
    {
        if (cooldown > 0f || cam == null) return;

        if (!RaycastCenter(out RaycastHit hit)) return;

        // 1) 优先：工具拾取
        ToolPickup tool = hit.collider.GetComponentInParent<ToolPickup>();
        if (tool != null)
        {
            PickupTool(tool);
            return;
        }

        // 2) 否则：普通物体拾取（要求 Tag=Pickup）
        if (!hit.collider.CompareTag("Pickup")) return;

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null) return;

        PickupNormal(rb, hit.collider);
        ToolItem toolItem = hit.collider.GetComponentInParent<ToolItem>();
        if (toolItem != null)
        {
            if (inventory != null)
            {
                bool ok = inventory.AddTool(toolItem);
                // 背包满了就不捡
                if (ok) return;
            }
        }
    }

    bool RaycastCenter(out RaycastHit hit)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return Physics.Raycast(ray, out hit, interactDistance, interactLayer);
    }

    // -------------------- Normal Item --------------------

    void PickupNormal(Rigidbody rb, Collider col)
    {
        heldRb = rb;
        heldCollider = col;

        ClearHighlight();

        if (playerCollider != null && heldCollider != null)
            Physics.IgnoreCollision(heldCollider, playerCollider, true);

        heldRb.linearVelocity = Vector3.zero;
        heldRb.angularVelocity = Vector3.zero;

        heldRb.isKinematic = true;
        heldRb.useGravity = false;

        heldRb.transform.SetParent(holdPoint, worldPositionStays: false);
        heldRb.transform.localPosition = Vector3.zero;
        heldRb.transform.localRotation = Quaternion.identity;

        // 你想穿墙：不需要禁用 collider，但若你想更“完全无阻”，可以启用这一行
        // heldCollider.enabled = false;
    }

    public void DropNormal()
    {
        if (heldRb == null) return;

        heldRb.transform.SetParent(null);
        RestoreNormalPhysics();

        heldRb = null;
        heldCollider = null;
        cooldown = 0.15f;
    }

    public void ThrowNormal()
    {
        if (heldRb == null) return;

        heldRb.transform.SetParent(null);
        RestoreNormalPhysics();

        Vector3 forward = cam.transform.forward;
        Vector3 impulse = forward * throwForce + Vector3.up * throwUpForce;
        heldRb.AddForce(impulse, ForceMode.Impulse);

        heldRb = null;
        heldCollider = null;
        cooldown = 0.2f;
    }

    void RestoreNormalPhysics()
    {
        if (heldRb == null) return;

        if (playerCollider != null && heldCollider != null)
            Physics.IgnoreCollision(heldCollider, playerCollider, false);

        heldRb.isKinematic = false;
        heldRb.useGravity = true;

        // 如果你之前禁用过 collider，记得打开
        if (heldCollider != null) heldCollider.enabled = true;
    }

    // -------------------- Tools --------------------

    void PickupTool(ToolPickup tool)
    {
        if (toolHoldPoint == null) return;

        // 已拥有：直接装备
        if (ownedTools.ContainsKey(tool.toolType))
        {
            EquipTool(tool.toolType);
            return;
        }

        ownedTools.Add(tool.toolType, tool);

        // 先把它挂到手上（非常关键：先Parent）
        tool.transform.SetParent(toolHoldPoint, false);
        tool.transform.localPosition = tool.holdLocalPosition;
        tool.transform.localRotation = Quaternion.Euler(tool.holdLocalEuler);

        // 再处理物理
        PrepareToolOwned(tool);

        // 默认装备刚捡到的工具
        EquipTool(tool.toolType);
    }

    void PrepareToolOwned(ToolPickup tool)
    {
        // 处理所有刚体（防止刚体在子物体上）
        var rbs = tool.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
        }

        // 忽略与玩家碰撞
        var cols = tool.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            if (c == null) continue;
            if (playerCollider != null) Physics.IgnoreCollision(c, playerCollider, true);
            if (tool.disableCollidersWhenOwned) c.enabled = false;
        }

        ClearHighlight();
    }
    void EquipTool(ToolType type)
    {
        if (!ownedTools.ContainsKey(type)) return;

        foreach (var kv in ownedTools)
            kv.Value.gameObject.SetActive(false);

        ownedTools[type].gameObject.SetActive(true);
        equippedTool = type;

        // 记录当前工具
        currentEquippedTool = ownedTools[type];
    }

    void ThrowEquippedTool()
    {
        if (!equippedTool.HasValue) return;
        if (!ownedTools.ContainsKey(equippedTool.Value)) return;

        ToolPickup tool = ownedTools[equippedTool.Value];
        if (tool == null) return;

        // 解除父子关系（保持当前位置）
        tool.transform.SetParent(null, true);

        // 恢复刚体，让它自然掉落
        var rbs = tool.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            if (rb == null) continue;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 恢复碰撞
        var cols = tool.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            if (c == null) continue;
            c.enabled = true;

            if (playerCollider != null)
                Physics.IgnoreCollision(c, playerCollider, false);
        }

        // 清除装备状态
        ownedTools.Remove(equippedTool.Value);
        equippedTool = null;
        currentEquippedTool = null;
    }
        void ToggleFlashlight()
    {
        if (!ownedTools.ContainsKey(ToolType.Flashlight)) return;

        // 手电灯组件建议挂在手电工具模型子物体上
        Light lightComp = ownedTools[ToolType.Flashlight].GetComponentInChildren<Light>(true);
        if (lightComp == null) return;

        lightComp.enabled = !lightComp.enabled;
    }

    void UseCrowbar()
    {
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, 2f, interactLayer)) return;

        CrowbarTarget target = hit.collider.GetComponentInParent<CrowbarTarget>();
        if (target == null) return;

        target.PryOpen();
    }

    // -------------------- Highlight --------------------

    void UpdateSelection()
    {
        // 拿着普通物体时不高亮；工具装备不影响高亮（你也可改成不高亮）
        if (heldRb != null || cam == null || highlightMaterial == null)
        {
            ClearHighlight();
            return;
        }

        if (!RaycastCenter(out RaycastHit hit))
        {
            ClearHighlight();
            return;
        }

        // 高亮对象：优先 Rigidbody 根
        Rigidbody rb = hit.collider.attachedRigidbody;
        Transform root = rb != null ? rb.transform : hit.collider.transform;

        if (!string.IsNullOrEmpty(selectableTag) && !HasTagInHierarchy(root.gameObject, selectableTag))
        {
            ClearHighlight();
            return;
        }

        if (currentHighlightRoot != root)
        {
            ClearHighlight();
            ApplyHighlight(root);
        }
    }

    bool HasTagInHierarchy(GameObject go, string tag)
    {
        Transform t = go.transform;
        while (t != null)
        {
            if (t.CompareTag(tag)) return true;
            t = t.parent;
        }
        return false;
    }

    void ApplyHighlight(Transform root)
    {
        if (root == null) return;

        var renderers = root.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r == null) continue;

            if (!savedSharedMats.ContainsKey(r))
                savedSharedMats[r] = r.sharedMaterials;

            var mats = r.sharedMaterials;
            var newMats = new Material[mats.Length];
            for (int i = 0; i < newMats.Length; i++) newMats[i] = highlightMaterial;
            r.sharedMaterials = newMats;
        }

        currentHighlightRoot = root;
    }

    void ClearHighlight()
    {
        if (currentHighlightRoot == null) return;

        foreach (var kv in savedSharedMats)
        {
            if (kv.Key != null)
                kv.Key.sharedMaterials = kv.Value;
        }

        savedSharedMats.Clear();
        currentHighlightRoot = null;
    }

    void OnDisable()
    {
        ClearHighlight();
    }

    void LateUpdate()
    {

        if (currentEquippedTool == null || toolHoldPoint == null || cam == null)return;

        Transform tool = currentEquippedTool.transform;

        // 确保在 toolHoldPoint 下面
        if (tool.parent != toolHoldPoint)
            tool.SetParent(toolHoldPoint, false);

        // 保持位置偏移
        tool.localPosition = currentEquippedTool.holdLocalPosition;

        // 永远朝向准星
        tool.rotation = cam.transform.rotation *
                        Quaternion.Euler(currentEquippedTool.holdLocalEuler);
    }
}
