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
    // 保留按键字段以便后续扩展，但工具管理逻辑已移除
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
        if (toolHoldPoint == null) Debug.LogWarning("PickupSystem: toolHoldPoint 未设置（工具拾取可能无法放到手上）。");
    }

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;

        UpdateSelection();

        // 统一交互键：优先拾取工具（通过 ToolItem -> PlayerInventory），其次普通物体；若已拿普通物体则 E=放下
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

        // 普通物体：放下 / 扔出
        if (Input.GetKeyDown(dropKey) && heldRb != null)
            DropNormal();

        if (Input.GetMouseButtonDown(throwMouseButton) && heldRb != null)
            ThrowNormal();

        // 注意：已删除 ownedTools/EquipTool 等工具拥有逻辑，
        // 这里不再调用与 ownedTools 相关的函数（例如 ThrowEquippedTool/ToggleFlashlight/UseCrowbar）。
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

        // ✅ 1) 先看是不是 ToolItem：是的话就加入 PlayerInventory（触发 UI icon）
        ToolItem toolItem = hit.collider.GetComponentInParent<ToolItem>();
        if (toolItem != null && inventory != null)
        {
            bool ok = inventory.AddTool(toolItem);
            if (ok)
            {
                // 保险：避免之前误抓普通物体残留
                heldRb = null;
                heldCollider = null;
                return;
            }
        }

        // 2) 最后才当普通物体拾取
        if (!hit.collider.CompareTag("Pickup")) return;

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null) return;

        PickupNormal(rb, hit.collider);
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

    // -------------------- Highlight --------------------

    void UpdateSelection()
    {
        // 拿着普通物体时不高亮
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
}
