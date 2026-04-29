using UnityEngine;
using TMPro;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("圆环列表（从内到外）")]
    public PuzzleRing[] rings;

    [Header("UI 提示（可选）")]
    public TMP_Text ringIndexText;

    [Header("Complete UI")]
    public GameObject completePanel; // 这里拖你的完成面板，里面放返回按钮

    [Header("开场指导文字")]
    public TMP_Text tutorialText;          // 拖入 Inspector，显示操作说明
    public float tutorialDisplayTime = 3f; // 显示秒数
    public float tutorialFadeTime = 1f;    // 淡出秒数

    private bool isSolved = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (completePanel != null)
            completePanel.SetActive(false);

        foreach (var ring in rings)
            ring.Randomize();

        if (tutorialText != null)
            StartCoroutine(ShowTutorial());
    }

    // 显示指导文字，停留后淡出
    private IEnumerator ShowTutorial()
    {
        // 确保完全不透明
        SetTutorialAlpha(1f);
        tutorialText.gameObject.SetActive(true);

        // 停留阶段
        yield return new WaitForSeconds(tutorialDisplayTime);

        // 淡出阶段
        float elapsed = 0f;
        while (elapsed < tutorialFadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / tutorialFadeTime);
            SetTutorialAlpha(alpha);
            yield return null;
        }

        tutorialText.gameObject.SetActive(false);
    }

    private void SetTutorialAlpha(float alpha)
    {
        if (tutorialText == null) return;
        Color c = tutorialText.color;
        c.a = alpha;
        tutorialText.color = c;
    }

    public void CheckSolved()
    {
        if (isSolved) return;

        foreach (var ring in rings)
        {
            if (!ring.IsSolved()) return;
        }
        
        isSolved = true;
        // 谜题完成后显示按钮/面板
        completePanel.SetActive(true);
        Debug.Log("The puzzle is complete!");
        DrawerController.Instance.OpenDrawer();
    }
}
