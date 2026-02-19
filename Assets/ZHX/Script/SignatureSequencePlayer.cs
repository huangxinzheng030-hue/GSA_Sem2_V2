using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SignatureSequencePlayer : MonoBehaviour
{
    [Header("Target UI Image")]
    public Image targetImage;        
    public Sprite[] frames;           
    public float fps = 24f;           
    public bool hideWhenFinished = false;

    Coroutine routine;

    void Awake()
    {
        if (targetImage != null)
            targetImage.enabled = false; 
    }

    public void PlayOnce()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PlayOnceRoutine());
    }

    public IEnumerator PlayOnceRoutine()
    {
        if (targetImage == null || frames == null || frames.Length == 0)
            yield break;

        targetImage.enabled = true;

        float frameTime = 1f / Mathf.Max(1f, fps);

        for (int i = 0; i < frames.Length; i++)
        {
            targetImage.sprite = frames[i];
            yield return new WaitForSeconds(frameTime);
        }

        if (hideWhenFinished)
            targetImage.enabled = false;

        routine = null;
    }

    public float GetDuration()
    {
        if (frames == null || frames.Length == 0) return 0f;
        return frames.Length / Mathf.Max(1f, fps);
    }
}
