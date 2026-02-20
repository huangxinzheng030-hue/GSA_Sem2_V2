using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class PuzzleManager : MonoBehaviour
{
    public PuzzleRing[] rings;      
    public Animator safeAnimator; 
    public GameObject detailCamera; 
    public float blendWaitTime = 2.0f; 
    public AudioClip winSound;      
    private AudioSource audioSource;
    private bool hasWon = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 核心：启动即推近
        if (detailCamera != null) 
        {
            detailCamera.SetActive(true); 
        }
    }

    void Update()
    {
        if (!hasWon && CheckWinCondition())
        {
            StartCoroutine(WinSequence());
        }
    }

    bool CheckWinCondition()
    {
        foreach (PuzzleRing ring in rings)
        {
            if (ring == null || !ring.IsCorrect()) return false;
        }
        return true;
    }

    IEnumerator WinSequence()
    {
        hasWon = true;
        
        // 1. 播放动画
        if (winSound != null) audioSource.PlayOneShot(winSound);
        if (safeAnimator != null) safeAnimator.SetTrigger("OpenSafe");

        // 2. 欣赏动画
        yield return new WaitForSeconds(blendWaitTime);

        // 3. 自动拉远
        if (detailCamera != null) detailCamera.SetActive(false);
    }
}