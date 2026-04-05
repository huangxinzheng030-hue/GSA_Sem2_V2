using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    public static SFXPlayer Instance;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip selectClip;
    public AudioClip moveClip;
    public AudioClip wrongClip;
    public AudioClip successClip;
    public AudioClip knockdownClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySelect()
    {
        PlayClip(selectClip);
    }

    public void PlayMove()
    {
        PlayClip(moveClip);
    }

    public void PlayWrong()
    {
        PlayClip(wrongClip);
    }

    public void PlaySuccess()
    {
        PlayClip(successClip);
    }

    public void PlayKnockdown()
    {
        PlayClip(knockdownClip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}