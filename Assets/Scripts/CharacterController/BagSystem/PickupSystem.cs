using UnityEngine;

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

    Rigidbody heldRb;
    Collider heldCollider;

    float cooldown;

    Renderer currentRenderer;
    Material[] originalMaterials;

    void Start()
    {
        if (holdPoint == null)
            Debug.LogWarning("PickupSystem: holdPoint not set");

        if (inventory == null)
            Debug.LogWarning("PickupSystem: inventory not set");
    }

    void Update()
    {
        UpdateCooldown();
        UpdateSelection();
        HandleInteractInput();
        HandleNormalItemInput();
    }

    void FixedUpdate()
    {
        if (heldRb == null || holdPoint == null) return;

        // 强制贴住 hand point（简单稳定）
        heldRb.transform.position = holdPoint.position;
        heldRb.transform.rotation = holdPoint.rotation;
    }

    // =========================
    // 输入逻辑
    // =========================

    void UpdateCooldown()
    {
        if (cooldown > 0f)
            cooldown -= Time.deltaTime;
    }

    void HandleInteractInput()
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

    void HandleNormalItemInput()
    {
        if (heldRb == null) return;

        if (Input.GetKeyDown(dropKey))
            DropNormal();

        if (Input.GetMouseButtonDown(throwMouseButton))
            ThrowNormal();
    }

    // =========================
    // 核心逻辑
    // =========================

    void InteractTryPickup()
    {
        if (cooldown > 0f || cam == null) return;

        if (!RaycastCenter(out RaycastHit hit)) return;

        // ✅ 优先 ToolItem（进入背包）
        ToolItem toolItem = hit.collider.GetComponentInParent<ToolItem>();

        if (toolItem != null)
        {
            if (inventory == null)
            {
                Debug.LogWarning("PickupSystem: ToolItem detect there is no inventory");
                return;
            }

            WorldCollectible collectible = hit.collider.GetComponentInParent<WorldCollectible>();

            bool ok = inventory.AddTool(toolItem);

            if (ok)
            {
                if (collectible != null)
                    collectible.MarkCollected();

                heldRb = null;
                heldCollider = null;
            }

            return;
        }

        if (toolItem != null)
        {
            if (inventory == null)
            {
                Debug.LogWarning("PickupSystem: ToolItem detect there is no inventory");
                return;
            }

            bool ok = inventory.AddTool(toolItem);

            if (ok)
            {
                heldRb = null;
                heldCollider = null;
            }

            return; // 🔥 关键：不再走普通拾取
        }

        // 普通物体
        if (!hit.collider.CompareTag("Pickup")) return;

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null) return;

        PickupNormal(rb, hit.collider);
    }

    void PickupNormal(Rigidbody rb, Collider col)
    {
        if (holdPoint == null) return;

        heldRb = rb;
        heldCollider = col;

        heldRb.useGravity = false;
        heldRb.isKinematic = true;

        if (playerCollider != null)
            Physics.IgnoreCollision(heldCollider, playerCollider, true);

        heldRb.transform.SetParent(holdPoint);
        heldRb.transform.localPosition = Vector3.zero;
        heldRb.transform.localRotation = Quaternion.identity;

        cooldown = 0.1f;
    }

    void DropNormal()
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

    void ThrowNormal()
    {
        if (heldRb == null) return;

        Rigidbody rb = heldRb;

        DropNormal();

        Vector3 dir = (cam != null) ? cam.transform.forward : transform.forward;

        rb.AddForce(dir * throwForce + Vector3.up * throwUpForce, ForceMode.Impulse);
    }

    // =========================
    // Raycast
    // =========================

    bool RaycastCenter(out RaycastHit hit)
    {
        if (cam == null)
        {
            hit = default;
            return false;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        return Physics.Raycast(ray, out hit, interactDistance, interactLayer);
    }

    // =========================
    // 高亮系统
    // =========================

    void UpdateSelection()
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
                            newMats[i] = highlightMaterial;

                        r.sharedMaterials = newMats;
                    }
                    return;
                }
            }
        }

        ClearHighlight();
    }

    void ClearHighlight()
    {
        if (currentRenderer != null && originalMaterials != null)
        {
            currentRenderer.sharedMaterials = originalMaterials;
        }

        currentRenderer = null;
        originalMaterials = null;
    }

    bool HasTagInHierarchy(GameObject obj, string tag)
    {
        Transform t = obj.transform;

        while (t != null)
        {
            if (t.CompareTag(tag)) return true;
            t = t.parent;
        }

        return false;
    }
}