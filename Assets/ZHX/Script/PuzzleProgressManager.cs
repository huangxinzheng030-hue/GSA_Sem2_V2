using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleProgressManager : MonoBehaviour
{
    [Header("Background")]
    public Image backgroundImage;

    [Header("Puzzle Settings")]
    public int totalStars = 7;

    [Range(0, 255)] public int startAlpha = 159;
    [Range(0, 255)] public int endAlpha = 255;
    public float fadeDuration = 1.5f;

    private int litStars = 0;
    private bool completed = false;

    private void Start()
    {
        SetAlpha(startAlpha);
    }

    public void OnStarLit()
    {
        litStars++;

        if (!completed && litStars >= totalStars)
        {
            completed = true;
            StartCoroutine(FadeToFull());
        }
    }

    void SetAlpha(int alpha)
    {
        if (backgroundImage == null) return;

        Color c = backgroundImage.color;
        c.a = alpha / 255f;
        backgroundImage.color = c;
    }

    IEnumerator FadeToFull()
    {
        if (backgroundImage == null) yield break;

        float time = 0f;
        float from = startAlpha / 255f;
        float to = endAlpha / 255f;

        Color c = backgroundImage.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            c.a = Mathf.Lerp(from, to, t);
            backgroundImage.color = c;
            yield return null;
        }

        c.a = to;
        backgroundImage.color = c;
    }
}