using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource oneShotSource;     // 播放通用一次性音效
    public AudioSource footstepSource;    // 专门给脚步声，避免和别的音效打架

    [Header("Clips")]
    public AudioClip pickupClip;
    public AudioClip throwClip;
    public AudioClip dropClip;

    [Header("Footsteps")]
    public AudioClip[] footstepClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayPickup()
    {
        PlayOneShot(pickupClip);
    }

    public void PlayThrow()
    {
        PlayOneShot(throwClip);
    }

    public void PlayDrop()
    {
        PlayOneShot(dropClip);
    }

    public void PlayFootstep()
    {
        if (footstepSource == null) return;
        if (footstepClips == null || footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        footstepSource.PlayOneShot(footstepClips[index]);
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (oneShotSource == null || clip == null) return;
        oneShotSource.PlayOneShot(clip, volume);
    }
}