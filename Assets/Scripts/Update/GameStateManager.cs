using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour, ISceneFlowService
{
    public static GameStateManager Instance { get; private set; }

    [Serializable]
    public class InventoryState
    {
        public string[] slotToolIds = Array.Empty<string>();
        public int selectedIndex = 0;
    }

    private class ReturnData
    {
        public string sceneName;
        public string spawnPointId;
    }

    private readonly Stack<ReturnData> returnStack = new Stack<ReturnData>();

    private readonly HashSet<string> playedIntroIds = new HashSet<string>();
    private readonly HashSet<string> unlockedPaintingIds = new HashSet<string>();
    private readonly HashSet<string> collectedWorldItemIds = new HashSet<string>();
    private readonly HashSet<string> completedPuzzleIds = new HashSet<string>();
    private readonly HashSet<string> flags = new HashSet<string>();

    private InventoryState inventoryState = new InventoryState();

    private bool shouldRestorePlayerPosition = false;
    private string pendingReturnSceneName = "";
    private string pendingSpawnPointId = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ---------- Intro ----------
    public bool HasPlayedIntro(string introId)
    {
        return !string.IsNullOrWhiteSpace(introId) && playedIntroIds.Contains(introId);
    }

    public void MarkIntroPlayed(string introId)
    {
        if (!string.IsNullOrWhiteSpace(introId))
            playedIntroIds.Add(introId);
    }

    // ---------- Paintings / Codex ----------
    public void UnlockPainting(string paintingId)
    {
        if (!string.IsNullOrWhiteSpace(paintingId))
            unlockedPaintingIds.Add(paintingId);
    }

    public bool IsPaintingUnlocked(string paintingId)
    {
        return !string.IsNullOrWhiteSpace(paintingId) && unlockedPaintingIds.Contains(paintingId);
    }

    // ---------- World Collectibles ----------
    public void MarkWorldItemCollected(string worldItemId)
    {
        if (!string.IsNullOrWhiteSpace(worldItemId))
            collectedWorldItemIds.Add(worldItemId);
    }

    public bool IsWorldItemCollected(string worldItemId)
    {
        return !string.IsNullOrWhiteSpace(worldItemId) && collectedWorldItemIds.Contains(worldItemId);
    }

    // ---------- Generic Flags ----------
    public void SetFlag(string flagId, bool value)
    {
        if (string.IsNullOrWhiteSpace(flagId)) return;

        if (value) flags.Add(flagId);
        else flags.Remove(flagId);
    }

    public bool GetFlag(string flagId)
    {
        return !string.IsNullOrWhiteSpace(flagId) && flags.Contains(flagId);
    }

    // ---------- Puzzle Completion ----------
    public void MarkPuzzleCompleted(string puzzleId)
    {
        if (!string.IsNullOrWhiteSpace(puzzleId))
            completedPuzzleIds.Add(puzzleId);
    }

    public bool IsPuzzleCompleted(string puzzleId)
    {
        return !string.IsNullOrWhiteSpace(puzzleId) && completedPuzzleIds.Contains(puzzleId);
    }

    // ---------- Inventory ----------
    public void SaveInventory(string[] slotToolIds, int selectedIndex)
    {
        inventoryState.slotToolIds = (slotToolIds != null) ? (string[])slotToolIds.Clone() : Array.Empty<string>();
        inventoryState.selectedIndex = selectedIndex;
    }

    public InventoryState GetInventoryState()
    {
        return inventoryState;
    }

    // ---------- Scene Flow ----------
    public void EnterPuzzle(string puzzleSceneName, string returnSpawnPointId)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        returnStack.Push(new ReturnData
        {
            sceneName = currentSceneName,
            spawnPointId = returnSpawnPointId
        });

        SceneManager.LoadScene(puzzleSceneName);
    }

    public void ReturnFromPuzzle()
    {
        if (returnStack.Count == 0)
        {
            Debug.LogWarning("GameStateManager: returnStack 为空，无法返回。");
            return;
        }

        ReturnData data = returnStack.Pop();

        pendingReturnSceneName = data.sceneName;
        pendingSpawnPointId = data.spawnPointId;
        shouldRestorePlayerPosition = true;

        SetFlag("ReturningFromPuzzle", true);

        SceneManager.LoadScene(data.sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!shouldRestorePlayerPosition)
            return;

        if (scene.name != pendingReturnSceneName)
            return;

        SceneSpawnPoint[] spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsSortMode.None);
        SceneSpawnPoint targetPoint = null;

        foreach (var point in spawnPoints)
        {
            if (point != null && point.spawnPointId == pendingSpawnPointId)
            {
                targetPoint = point;
                break;
            }
        }

        if (targetPoint == null)
        {
            Debug.LogWarning("GameStateManager: 没找到返回点 -> " + pendingSpawnPointId);
            shouldRestorePlayerPosition = false;
            pendingReturnSceneName = "";
            pendingSpawnPointId = "";
            SetFlag("ReturningFromPuzzle", false);
            return;
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("GameStateManager: 没找到 Player Tag。");
            shouldRestorePlayerPosition = false;
            pendingReturnSceneName = "";
            pendingSpawnPointId = "";
            SetFlag("ReturningFromPuzzle", false);
            return;
        }

        Debug.Log("GameStateManager: 强制传送到 -> " + targetPoint.spawnPointId);

        StartCoroutine(ForceRestorePlayerPosition(player, targetPoint));
    }
    private System.Collections.IEnumerator ForceRestorePlayerPosition(GameObject player, SceneSpawnPoint targetPoint)
    {
        if (player == null || targetPoint == null)
        {
            shouldRestorePlayerPosition = false;
            pendingReturnSceneName = "";
            pendingSpawnPointId = "";
            SetFlag("ReturningFromPuzzle", false);
            yield break;
        }

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null)
            cc = player.GetComponentInChildren<CharacterController>();

        // 连续多帧强制纠正位置，防止被别的出生脚本覆盖
        for (int i = 0; i < 10; i++)
        {
            if (cc != null) cc.enabled = false;

            player.transform.position = targetPoint.transform.position;
            player.transform.rotation = targetPoint.transform.rotation;

            if (cc != null) cc.enabled = true;

            yield return null;
        }

        Debug.Log("ForceRestorePlayerPosition 完成 -> " + targetPoint.spawnPointId);

        shouldRestorePlayerPosition = false;
        pendingReturnSceneName = "";
        pendingSpawnPointId = "";
        SetFlag("ReturningFromPuzzle", false);
    }
}