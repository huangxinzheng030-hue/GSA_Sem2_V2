using System.Collections.Generic;
using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint;               // 相机前的空物体
    public Collider playerCollider;           // 拖 Player 的 CapsuleCollider 进来（很关键！）

    [Header("Pickup Settings")]
    public string pickupTag = "Pickup";
    public float pickupDistance = 3f;

    [Header("Input")]
    public KeyCode pickupKey = KeyCode.E;     // 拾取/放下（切换）
    public KeyCode dropKey = KeyCode.Q;       // 强制放下
    public int throwMouseButton = 0;          // 0=左键

    [Header("Hold")]
    public float holdSmoothSpeed = 25f;
    public bool keepUpright = false;

    [Header("Throw")]
    public float throwForce = 8f;
    public float throwUpForce = 1.2f;

    // Selection / Highlight 设置（已内置）
    [Header("Selection")]
    public Camera cam;                        // 不设置则自动使用 Camera.main
    public LayerMask selectableLayer = ~0;
    public string selectableTag = "Pickup";   // 可选：使用 tag 筛选
    public Material highlightMaterial;        // 高亮材质（在 Inspector 指定）

    private Rigidbody heldRb;
    private Collider heldCollider;
    private float cooldown;

    // 高亮状态
    private GameObject currentTarget;
    private readonly Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) Debug.LogWarning("PickupSystem: cam 未设置且 Camera.main 为空。");
        if (highlightMaterial == null) Debug.LogWarning("PickupSystem: highlightMaterial 未设置，无法高亮显示。");
    }

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;

        // 每帧检测并高亮当前可选目标（仅在未持有物体时）
        UpdateSelection();

        if (Input.GetKeyDown(pickupKey))
        {
            if (heldRb == null) TryPickup();
            else Drop();
        }

        if (Input.GetKeyDown(dropKey))
        {
            if (heldRb != null) Drop();
        }

        if (Input.GetMouseButtonDown(throwMouseButton))
        {
            if (heldRb != null) Throw();
        }
    }

    void FixedUpdate()
    {
        if (heldRb == null) return;

        // 如果物体为 kinematic（已被 parent 到 holdPoint），直接精确同步 transform，消除物理抖动
        if (heldRb.isKinematic)
        {
            // 直接设定 transform（父级通常已是 holdPoint）
            if (heldRb.transform.parent == holdPoint)
            {
                // 已经父级到 holdPoint 时，本身 transform 会随父物体自动跟随。
                // 这里仍强制同步本地 transform（防止少量偏移）
                heldRb.transform.localPosition = Vector3.zero;
                heldRb.transform.localRotation = Quaternion.identity;
            }
            else
            {
                // 未父级的情况下直接对齐世界坐标
                heldRb.transform.position = holdPoint.position;
                heldRb.transform.rotation = holdPoint.rotation;
            }
            return;
        }

        // 物理驱动的持有（如果不使用 kinematic）保留原有平滑跟随逻辑
        Vector3 targetPos = holdPoint.position;
        Vector3 newPos = Vector3.Lerp(heldRb.position, targetPos, holdSmoothSpeed * Time.fixedDeltaTime);
        heldRb.MovePosition(newPos);

        Quaternion targetRot = keepUpright
            ? Quaternion.LookRotation(transform.forward, Vector3.up)
            : holdPoint.rotation;

        Quaternion newRot = Quaternion.Slerp(heldRb.rotation, targetRot, holdSmoothSpeed * Time.fixedDeltaTime);
        heldRb.MoveRotation(newRot);
    }

    void TryPickup()
    {
        if (cooldown > 0f) return;

        Ray ray;
        if (cam != null)
            ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        else
            ray = new Ray(transform.position, transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, pickupDistance, selectableLayer)) return;

        // 可选 Tag 筛选
        if (!string.IsNullOrEmpty(selectableTag) && !HasTagInHierarchy(hit.collider.gameObject, selectableTag)) return;

        if (!hit.collider.CompareTag(pickupTag) && !string.IsNullOrEmpty(pickupTag)) return;

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null) return;

        heldRb = rb;
        heldCollider = hit.collider;

        // 清除高亮（拿起后不再高亮）
        ClearCurrent();

        // 忽略与玩家的碰撞（核心稳定点）
        if (playerCollider != null && heldCollider != null)
            Physics.IgnoreCollision(heldCollider, playerCollider, true);

        // 拿起：先清速度
        heldRb.linearVelocity = Vector3.zero;
        heldRb.angularVelocity = Vector3.zero;

        // 选择 kinematic 持有，避免物理抖动
        heldRb.isKinematic = true;

        // 父级到 holdPoint，位置与旋转对齐
        heldRb.transform.SetParent(holdPoint, worldPositionStays: false);
        heldRb.transform.localPosition = Vector3.zero;
        heldRb.transform.localRotation = Quaternion.identity;

        // （可选）关闭重力与阻尼设置（kineamtic 时无效但保留以防切换）
        heldRb.useGravity = false;
        heldCollider.enabled = false; // 可选：禁用碰撞器（如果不需要与其他物体交互）
        heldRb.linearDamping = 10f;
        heldRb.angularDamping = 10f;
    }

    public void Drop()
    {
        if (heldRb == null) return;

        // 取消父级并恢复物理
        heldRb.transform.SetParent(null);
        heldCollider.enabled = true; // 确保碰撞器启用
        RestorePhysics();

        heldRb = null;
        heldCollider = null;
        cooldown = 0.15f;
    }

    public void Throw()
    {
        if (heldRb == null) return;

        // 取消父级并恢复物理
        heldRb.transform.SetParent(null);
        heldCollider.enabled = true; // 确保碰撞器启用
        RestorePhysics();

        // 再加冲量（方向用相机 forward）
        Vector3 impulse = transform.forward * throwForce + Vector3.up * throwUpForce;
        heldRb.AddForce(impulse, ForceMode.Impulse);

        heldRb = null;
        heldCollider = null;
        cooldown = 0.2f;
    }

    void RestorePhysics()
    {
        if (heldRb == null) return;

        // 恢复与玩家碰撞
        if (playerCollider != null && heldCollider != null)
            Physics.IgnoreCollision(heldCollider, playerCollider, false);

        // 恢复物理属性
        heldRb.isKinematic = false;
        heldRb.useGravity = true;
        heldRb.linearDamping = 0f;
        heldRb.angularDamping = 0.05f;
        heldCollider.enabled = true; // 确保碰撞器启用
    }

    // ----------------- Selection / Highlight 方法 -----------------

    private void UpdateSelection()
    {
        // 仅在未持有物体并且配置了高亮材质时进行高亮检测
        if (heldRb != null || cam == null || highlightMaterial == null)
        {
            ClearCurrent();
            return;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, selectableLayer))
        {
            GameObject hitRoot = hit.collider.gameObject;

            // 如果设置了 tag 则要求命中对象或其父对象带有该 tag
            if (!string.IsNullOrEmpty(selectableTag) && !HasTagInHierarchy(hitRoot, selectableTag))
            {
                ClearCurrent();
                return;
            }

            if (hitRoot != currentTarget)
            {
                ClearCurrent();
                ApplyHighlight(hitRoot);
            }
        }
        else
        {
            ClearCurrent();
        }
    }

    private bool HasTagInHierarchy(GameObject go, string tag)
    {
        Transform t = go.transform;
        while (t != null)
        {
            if (t.gameObject.CompareTag(tag)) return true;
            t = t.parent;
        }
        return false;
    }

    private void ApplyHighlight(GameObject target)
    {
        if (target == null) return;

        var renderers = target.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r == null) continue;
            // 保存原始材质数组（若已保存则跳过）
            if (!originalMaterials.ContainsKey(r))
                originalMaterials[r] = r.materials;

            // 用高亮材质替换（保持 sub-mesh 数量）
            Material[] mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++) mats[i] = highlightMaterial;
            r.materials = mats;
        }

        currentTarget = target;
    }

    private void ClearCurrent()
    {
        if (currentTarget == null) return;

        foreach (var kv in originalMaterials)
        {
            if (kv.Key == null) continue;
            kv.Key.materials = kv.Value;
        }

        originalMaterials.Clear();
        currentTarget = null;
    }

    private void OnDisable()
    {
        // 确保禁用脚本时恢复材质
        ClearCurrent();
    }
}
