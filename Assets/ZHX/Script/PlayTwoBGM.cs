using UnityEngine;
using System.Collections;

public class PlayTwoBGM : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip firstBGM;
    public AudioClip secondBGM;

    void Start()
    {
        StartCoroutine(PlayMusicSequence());
    }

    IEnumerator PlayMusicSequence()
    {
        audioSource.clip = firstBGM;
        audioSource.loop = false;
        audioSource.Play();

        yield return new WaitForSeconds(firstBGM.length);

        audioSource.clip = secondBGM;
        audioSource.loop = false;
        audioSource.Play();
    }
}