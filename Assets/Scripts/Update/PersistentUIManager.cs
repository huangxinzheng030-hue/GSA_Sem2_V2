using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentUIManager : MonoBehaviour
{
    public HotbarUI hotbarUI;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(RebindNextFrame());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RebindNextFrame());
    }

    private IEnumerator RebindNextFrame()
    {
        yield return null;
        yield return null;

        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();

        if (inventory != null)
        {
            if (hotbarUI != null)
            {
                hotbarUI.RebindInventory(inventory);
            }

            inventory.RestoreFromGameState();
            inventory.ForceRefreshInventoryView();

            Debug.Log("PersistentUIManager: 已重新绑定当前场景 PlayerInventory。");
        }
        else
        {
            Debug.LogWarning("PersistentUIManager: 当前场景没有找到 PlayerInventory。");
        }
    }
}