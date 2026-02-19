using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PuzzleManager : MonoBehaviour
{
    [Header("核心关联")]
    public PuzzleRing[] rings;      // 把所有 Ring 物体拖进这里
    
    [Header("UI 反馈")]
    public GameObject winTextObject; // 拖入显示胜利文字的 UI 物体

    [Header("音效")]
    public AudioClip winSound;      // 拖入胜利音效
    
    private bool hasWon = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // 游戏开始时隐藏胜利文字
        if (winTextObject != null)
        {
            winTextObject.SetActive(false);
        }
    }

    void Update()
    {
        // 如果还没赢，每帧检查一次
        if (!hasWon)
        {
            if (CheckWinCondition())
            {
                PerformWinSequence();
            }
        }
    }

    // 检查所有环是否都对齐了
    bool CheckWinCondition()
    {
        foreach (PuzzleRing ring in rings)
        {
            // 只要有一个没对齐，就返回 false
            if (!ring.IsCorrect())
            {
                return false;
            }
        }
        return true;
    }

    // 胜利时的处理
    void PerformWinSequence()
    {
        hasWon = true;
        Debug.Log("🎉 游戏胜利！");

        // 1. 播放胜利音效
        if (winSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(winSound);
        }

        // 2. 显示 UI 文字
        if (winTextObject != null)
        {
            winTextObject.SetActive(true);
        }

        // 3. 锁定所有圆环，防止玩家继续乱点
        foreach (PuzzleRing ring in rings)
        {
            ring.LockRing();
        }
    }
}