using UnityEngine;

public class WorldCollectible : MonoBehaviour
{
    [Header("Unique Scene Item ID")]
    [Tooltip("每个场景拾取物必须唯一，例如 Louvre_MonaLisa_Pickup")]
    public string worldItemId;

    [Header("References")]
    public ToolItem toolItem;

    [Header("Object To Hide")]
    [Tooltip("通常拖这个拾取物的根物体。不拖则默认隐藏自己。")]
    public GameObject objectToHide;

    [Header("Optional")]
    [Tooltip("如果 worldItemId 没填，是否自动使用 ToolData.toolId。更推荐手动填写唯一 worldItemId。")]
    public bool useToolIdIfEmpty = false;

    private void Awake()
    {
        if (objectToHide == null)
            objectToHide = gameObject;

        if (toolItem == null)
            toolItem = GetComponentInChildren<ToolItem>(true);

        if (string.IsNullOrWhiteSpace(worldItemId) && useToolIdIfEmpty)
        {
            if (toolItem != null && toolItem.data != null)
                worldItemId = toolItem.data.toolId;
        }
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(worldItemId))
        {
            Debug.LogWarning($"{name}: WorldCollectible 的 worldItemId 没有填写。");
            return;
        }

        if (GameStateManager.Instance != null &&
            GameStateManager.Instance.IsWorldItemCollected(worldItemId))
        {
            HideObject();
        }
    }

    public void MarkCollected()
    {
        if (string.IsNullOrWhiteSpace(worldItemId))
        {
            Debug.LogWarning($"{name}: worldItemId 为空，无法记录已拾取状态。");
            return;
        }

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.MarkWorldItemCollected(worldItemId);
        }

        HideObject();
    }

    private void HideObject()
    {
        if (objectToHide != null)
            objectToHide.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}