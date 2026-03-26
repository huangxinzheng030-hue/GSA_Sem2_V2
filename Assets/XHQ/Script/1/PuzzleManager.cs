using UnityEngine;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    [Header("关联组件")]
    public PuzzleRing[] rings;      // 留空则自动寻找
    public GameObject detailCamera; // 近景相机
    public Animator safeAnimator;   // 保险箱Animator

    [Header("时间参数")]
    public float dissolveTime = 1.5f;   // 溶解时长
    public float blendWaitTime = 2.0f;  // 相机拉远时长（需与Brain设置一致）

    private bool hasWon = false;

    void Start()
    {
        // --- 核心修复：自动寻找圆环 ---
        if (rings == null || rings.Length == 0)
        {
            rings = GetComponentsInChildren<PuzzleRing>();
            Debug.Log($"<color=cyan>PuzzleManager:</color> 自动找到了 {rings.Length} 个圆环");
        }

        // 初始自动从远及近（前提是相机优先级已设好，detailCamera初始Inactive）
        if (detailCamera != null)
        {
            detailCamera.SetActive(true);
        }
    }

    void Update()
    {
        if (!hasWon && CheckAllRings())
        {
            hasWon = true; // 锁定逻辑
            StartCoroutine(WinSequence());
        }
    }

    bool CheckAllRings()
    {
        if (rings.Length == 0) return false;
        foreach (var ring in rings)
        {
            if (ring == null || !ring.IsCorrect()) return false;
        }
        return true;
    }

    IEnumerator WinSequence()
    {
        Debug.Log("<color=green>解密成功！</color> 开始执行后续动画...");

        // 1. 锁定并溶解图标
        foreach (var ring in rings)
        {
            ring.LockRing();
            StartCoroutine(ring.DissolveRoutine(dissolveTime));
        }
        yield return new WaitForSeconds(dissolveTime);

        // 2. 拉远相机
        if (detailCamera != null)
        {
            detailCamera.SetActive(false);
            Debug.Log("正在拉远相机...");
        }

        // 等待相机飞行到位
        yield return new WaitForSeconds(blendWaitTime);

        // 3. 开启保险箱
        if (safeAnimator != null)
        {
            safeAnimator.SetTrigger("OpenSafe");
            Debug.Log("保险箱开启！");
        }
    }
}