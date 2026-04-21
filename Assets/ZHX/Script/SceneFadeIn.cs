using System.Collections;
using UnityEngine;

public class SceneFadeIn : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 2f;
    public float startDelay = 0f;

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        float time = 0f;
        canvasGroup.alpha = 1f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = 1f - (time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}