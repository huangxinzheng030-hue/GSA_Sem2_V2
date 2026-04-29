using UnityEngine;

// 拾取系统：处理玩家面向中心射线的物体交互、拾取至手中、放下、投掷、工具类物品加入背包以及高亮显示可拾取物体。
public class PickupSystem : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform holdPoint;
    public Collider playerCollider;
    public PlayerInventory inventory;

    [Header("Ray Settings")]
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    [Header("Normal Pickup")]
    public float holdSmoothSpeed = 12f;
    public bool keepUpright = true;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.Q;
    public int throwMouseButton = 0;

    [Header("Throw Settings")]
    public float throwForce = 8f;
    public float throwUpForce = 1.2f;

    [Header("Selection Highlight")]
    public string selectableTag = "Pickup";
    public Material highlightMaterial;

    private Rigidbody heldRb;
    private Collider heldCollider;

    private float cooldown;

    private Renderer currentRenderer;
    private Material[] originalMaterials;

    private void Start()
    {
        if (holdPoint == null)
            Debug.LogWarning("PickupSystem: holdPoint not set");

        if (inventory == null)
            Debug.LogWarning("PickupSystem: inventory not set");

        if (cam == null)
            Debug.LogWarning("PickupSystem: cam not set");
    }

    private void Update()
    {
        // 每帧更新冷却、选择高亮与输入处理
        UpdateCooldown();
        UpdateSelection();
        HandleInteractInput();
        HandleNormalItemInput();
    }

    private void FixedUpdate()
    {
        // 在物理更新中保持被持有物体跟随 holdPoint（位置与旋转）
        if (heldRb == null || holdPoint == null) return;

        heldRb.transform.position = holdPoint.position;
        heldRb.transform.rotation = holdPoint.rotation;
    }

    // =========================
    // 输入逻辑（按键响应）
    // =========================

    // 更新交互冷却计时器
    private void UpdateCooldown()
    {
        if (cooldown > 0f)
            cooldown -= Time.deltaTime;
    }

    // 处理“交互”按键（拾取/放下）
    private void HandleInteractInput()
    {
        if (!Input.GetKeyDown(interactKey)) return;

        if (heldRb != null)
        {
            DropNormal();
        }
        else
        {
            InteractTryPickup();
        }
    }

    // 处理持有物体的丢弃与投掷输入
    private void HandleNormalItemInput()
    {
        if (heldRb == null) return;

        if (Input.GetKeyDown(dropKey))
            DropNormal();

        if (Input.GetMouseButtonDown(throwMouseButton))
            ThrowNormal();
    }

    // =========================
    // 核心交互逻辑
    // =========================

    // 尝试通过中心射线进行交互（拾取或将工具类加入背包）
    private void InteractTryPickup()
    {
        if (cooldown > 0f || cam == null) return;

        if (!RaycastCenter(out RaycastHit hit)) return;

        // 1. 优先检测 ToolItem：如果是工具类物品则尝试加入背包
        ToolItem toolItem = hit.collider.GetComponentInParent<ToolItem>();

        if (toolItem != null)
        {
            TryPickupToolItem(hit, toolItem);
            return;
        }

        // 2. 普通可拾取物：检查 Tag，并尝试拾取到手中
        if (!hit.collider.CompareTag(selectableTag) && !HasTagInHierarchy(hit.collider.gameObject, selectableTag))
            return;

        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null)
            rb = hit.collider.GetComponentInParent<Rigidbody>();

        if (rb == null) return;

        PickupNormal(rb, hit.collider);
    }

    // 处理工具类物品拾取（加入背包并标记场景物体已被收集）
    private void TryPickupToolItem(RaycastHit hit, ToolItem toolItem)
    {
        if (inventory == null)
        {
            Debug.LogWarning("PickupSystem: ToolItem detected but inventory is null.");
            return;
        }

        // 注意：必须在 AddTool 之前先拿 WorldCollectible。
        // 因为 AddTool 可能会把 ToolItem 从场景父物体中拆出去并收进背包。
        WorldCollectible collectible = hit.collider.GetComponentInParent<WorldCollectible>();

        bool added = inventory.AddTool(toolItem);

        if (!added)
        {
            Debug.Log("PickupSystem: 背包已满或无法添加该物品。");
            return;
        }

        // 只有真正成功加入背包，才记录这个场景物体已经被拿走。
        if (collectible != null)
        {
            collectible.MarkCollected();
        }

        // 确保当前没有持有刚刚拾取的物体（工具直接移入背包）
        heldRb = null;
        heldCollider = null;
        cooldown = 0.1f;
    }



    // 将普通物体拾取到玩家手中（禁用重力并设为 kinematic，附到 holdPoint）
    private void PickupNormal(Rigidbody rb, Collider col)
    {
        if (holdPoint == null) return;

        heldRb = rb;
        heldCollider = col;

        heldRb.useGravity = false;
        heldRb.isKinematic = true;

        if (playerCollider != null && heldCollider != null)
            Physics.IgnoreCollision(heldCollider, playerCollider, true);

        heldRb.transform.SetParent(holdPoint);
        heldRb.transform.localPosition = Vector3.zero;
        heldRb.transform.localRotation = Quaternion.identity;

        cooldown = 0.1f;
    }

    // 放下当前手持物体（恢复物理属性并取消父子关系）
    private void DropNormal()
    {
        if (heldRb == null) return;

        heldRb.transform.SetParent(null);

        heldRb.useGravity = true;
        heldRb.isKinematic = false;

        if (playerCollider != null && heldCollider != null)
            Physics.IgnoreCollision(heldCollider, playerCollider, false);

        heldRb = null;
        heldCollider = null;

        cooldown = 0.1f;
    }

    // 投掷当前手持物体（先放下再施加冲量）
    private void ThrowNormal()
    {
        if (heldRb == null) return;

        Rigidbody rb = heldRb;

        DropNormal();

        Vector3 dir = cam != null ? cam.transform.forward : transform.forward;

        rb.AddForce(dir * throwForce + Vector3.up * throwUpForce, ForceMode.Impulse);
    }



    // 从摄像机中心发射射线检测可交互物体
    private bool RaycastCenter(out RaycastHit hit)
    {
        if (cam == null)
        {
            hit = default;
            return false;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        return Physics.Raycast(ray, out hit, interactDistance, interactLayer);
    }


    // 更新中心准星处物体的高亮显示（使用替换材质实现）
    private void UpdateSelection()
    {
        if (cam == null || highlightMaterial == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            Transform root = hit.collider.transform;

            if (HasTagInHierarchy(root.gameObject, selectableTag))
            {
                Renderer r = root.GetComponentInChildren<Renderer>();

                if (r != null)
                {
                    if (currentRenderer != r)
                    {
                        ClearHighlight();

                        currentRenderer = r;
                        originalMaterials = r.sharedMaterials;

                        Material[] newMats = new Material[originalMaterials.Length];

                        for (int i = 0; i < newMats.Length; i++)
                        {
                            newMats[i] = highlightMaterial;
                        }

                        r.sharedMaterials = newMats;
                    }

                    return;
                }
            }
        }

        ClearHighlight();
    }

    // 清除当前高亮，恢复原始材质
    private void ClearHighlight()
    {
        if (currentRenderer != null && originalMaterials != null)
        {
            currentRenderer.sharedMaterials = originalMaterials;
        }

        currentRenderer = null;
        originalMaterials = null;
    }

    // 向上遍历父级以检测指定 Tag（用于支持父对象标记）
    private bool HasTagInHierarchy(GameObject obj, string tag)
    {
        Transform t = obj.transform;

        while (t != null)
        {
            if (t.CompareTag(tag))
                return true;

            t = t.parent;
        }

        return false;
    }
}