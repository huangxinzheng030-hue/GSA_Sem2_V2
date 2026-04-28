using System.Collections.Generic;
using UnityEngine;

public class ToolRegistry : MonoBehaviour
{
    public static ToolRegistry Instance { get; private set; }

    [System.Serializable]
    public class Entry
    {
        public string toolId;
        public ToolItem prefab;
    }

    public List<Entry> entries = new List<Entry>();

    private Dictionary<string, ToolItem> map = new Dictionary<string, ToolItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildMap();
    }

    private void BuildMap()
    {
        map.Clear();

        foreach (var entry in entries)
        {
            if (entry == null || entry.prefab == null) continue;

            string id = entry.toolId;

            if (string.IsNullOrWhiteSpace(id) && entry.prefab.data != null)
                id = entry.prefab.data.toolId;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("ToolRegistry: 有条目没填 toolId。");
                continue;
            }

            map[id] = entry.prefab;
        }
    }

    public ToolItem SpawnById(string toolId)
    {
        if (string.IsNullOrWhiteSpace(toolId)) return null;

        if (!map.TryGetValue(toolId, out var prefab) || prefab == null)
        {
            Debug.LogWarning("ToolRegistry: 找不到 toolId -> " + toolId);
            return null;
        }

        ToolItem instance = Instantiate(prefab);
        instance.name = prefab.name;
        return instance;
    }
}