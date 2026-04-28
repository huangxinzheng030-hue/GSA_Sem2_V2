using UnityEngine;

public class WorldCollectible : MonoBehaviour
{
    public string worldItemId;
    public ToolItem toolItem;

    private void Awake()
    {
        if (toolItem == null)
            toolItem = GetComponentInChildren<ToolItem>(true);
    }

    private void Start()
    {
        if (GameStateManager.Instance != null &&
            GameStateManager.Instance.IsWorldItemCollected(worldItemId))
        {
            gameObject.SetActive(false);
        }
    }

    public void MarkCollected()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.MarkWorldItemCollected(worldItemId);
    }
}