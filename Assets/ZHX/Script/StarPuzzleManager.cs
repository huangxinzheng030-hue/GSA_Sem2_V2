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

        Debug.Log("已点亮星星数量：" + litCount);

        if (litCount >= totalStars)
        {
            victoryPlayed = true;

            if (victoryAudio != null)
            {
                victoryAudio.Play();
            }

            Debug.Log("七颗星星全部点亮，胜利音效播放");
        }
    }
}