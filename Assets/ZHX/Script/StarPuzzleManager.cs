using System.Collections;
using UnityEngine;

public class StarPuzzleManager : MonoBehaviour
{
    public int totalStars = 7;
    public AudioSource victoryAudio;

    private int litCount = 0;
    private bool victoryPlayed = false;

    public void StarLit()
    {
        if (victoryPlayed) return;

        litCount++;

        Debug.Log("已点亮星星数量: " + litCount);

        if (litCount >= totalStars)
        {
            victoryPlayed = true;
            StartCoroutine(HandlePuzzleSolved());
        }
    }

    private IEnumerator HandlePuzzleSolved()
    {
        if (victoryAudio != null)
        {
            victoryAudio.Play();
        }

        Debug.Log("七颗星全部点亮，胜利音效播放");

        yield return new WaitForSeconds(1f);

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.MarkPuzzleCompleted("StarPuzzle");
            GameStateManager.Instance.SetFlag("StarPuzzle.completed", true);
            GameStateManager.Instance.ReturnFromPuzzle();
        }
    }
}