using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class PaintingCodexUI : MonoBehaviour
{
    public static PaintingCodexUI Instance { get; private set; }

    [System.Serializable]
    public class PaintingEntry
    {
        public string paintingId;              // 必须和 ToolData.toolId 对应
        public Button button;                  // 图鉴里的按钮
        public Image image;                    // 按钮上的图
        public Image selectionHighlight;   // 被选中时显示的边框
        public string displayName;             // 展示名
        [TextArea(3, 8)]
        public string description;             // 介绍文字

        [HideInInspector] public bool unlocked;
    }

    [Header("Entries")]
    public List<PaintingEntry> entries = new List<PaintingEntry>();

    [Header("UI")]
    public GameObject infoPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    [Header("Visual")]
    [Range(0f, 1f)] public float lockedAlpha = 0.25f;
    [Range(0f, 1f)] public float unlockedAlpha = 1f;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip selectClip;
    public AudioClip lockedClip;
    public AudioClip unlockClip;

    [Header("Locked Shake")]
    public float shakeDuration = 0.15f;
    public float shakeStrength = 8f;
    public int shakeVibrato = 10;

    private Dictionary<string, PaintingEntry> entryMap = new Dictionary<string, PaintingEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        entryMap.Clear();

        foreach (var entry in entries)
        {
            if (entry.selectionHighlight != null)
            {
                entry.selectionHighlight.gameObject.SetActive(false);
            }

            if (entry == null || string.IsNullOrWhiteSpace(entry.paintingId)) continue;

            entryMap[entry.paintingId] = entry;

            if (entry.button != null)
            {
                string cachedId = entry.paintingId;
                entry.button.onClick.RemoveAllListeners();
                entry.button.onClick.AddListener(() => OnClickEntry(cachedId));
            }

            bool unlocked = entry.unlocked;

            if (GameStateManager.Instance != null)
            {
                unlocked = GameStateManager.Instance.IsPaintingUnlocked(entry.paintingId);
            }

            entry.unlocked = unlocked;
            SetEntryVisual(entry, unlocked);
        }

        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public void UnlockPainting(string paintingId)
    {
        if (string.IsNullOrWhiteSpace(paintingId)) return;
        if (!entryMap.TryGetValue(paintingId, out var entry)) return;

        // 已经解锁过就不重复播
        if (entry.unlocked) return;

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnlockPainting(paintingId);
        }

        entry.unlocked = true;
        SetEntryVisual(entry, true);

        if (uiAudioSource != null && unlockClip != null)
        {
            uiAudioSource.PlayOneShot(unlockClip);
        }
    }

    public bool IsUnlocked(string paintingId)
    {
        if (string.IsNullOrWhiteSpace(paintingId)) return false;
        if (!entryMap.TryGetValue(paintingId, out var entry)) return false;
        return entry.unlocked;
    }

    private void OnClickEntry(string paintingId)
    {
        if (!entryMap.TryGetValue(paintingId, out var entry)) return;

        // 未解锁：播放失败音效，直接返回
        if (!entry.unlocked)
        {
            if (uiAudioSource != null && lockedClip != null)
            {
                uiAudioSource.PlayOneShot(lockedClip);
            }

            if (entry.button != null)
            {
                StartCoroutine(ShakeUI(entry.button.GetComponent<RectTransform>()));
            }

            return;
        }

        // 已解锁：播放选中音效
        if (uiAudioSource != null && selectClip != null)
        {
            uiAudioSource.PlayOneShot(selectClip);
        }

        // 先关闭所有高亮
        foreach (var e in entries)
        {
            if (e != null && e.selectionHighlight != null)
            {
                e.selectionHighlight.gameObject.SetActive(false);
            }
        }

        // 打开当前高亮
        if (entry.selectionHighlight != null)
        {
            entry.selectionHighlight.gameObject.SetActive(true);
        }

        if (infoPanel != null)
            infoPanel.SetActive(true);

        if (titleText != null)
            titleText.text = entry.displayName;

        if (descriptionText != null)
            descriptionText.text = entry.description;
    }

    private void SetEntryVisual(PaintingEntry entry, bool unlocked)
    {
        if (entry.image != null)
        {
            Color c = entry.image.color;
            c.a = unlocked ? unlockedAlpha : lockedAlpha;
            entry.image.color = c;
        }

        if (entry.button != null)
        {
            // 按钮仍可点也行，但锁定时点击没反应
            // 这里我保留可点，视觉更统一
            entry.button.interactable = true;
        }
    }

    private System.Collections.IEnumerator ShakeUI(RectTransform rect)
    {
        if (rect == null) yield break;

        Vector2 originalPos = rect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float offsetX = Random.Range(-shakeStrength, shakeStrength);
            float offsetY = Random.Range(-shakeStrength * 0.3f, shakeStrength * 0.3f);

            rect.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);

            yield return null;
        }

        rect.anchoredPosition = originalPos;
    }
}