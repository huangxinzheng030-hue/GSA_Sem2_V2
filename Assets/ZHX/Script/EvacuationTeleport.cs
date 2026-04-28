using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EvacuationTeleport : MonoBehaviour
{
    [Header("Scene")]
    public string targetSceneName = "loading2";
    public float delayBeforeLoad = 1.5f;

    [Header("Player")]
    public string playerTag = "Player";

    [Header("Animation")]
    public Animator animator;
    public string triggerName = "Play";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip teleportSFX;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag(playerTag))
        {
            hasTriggered = true;
            StartCoroutine(TeleportSequence());
        }
    }

    private IEnumerator TeleportSequence()
    {
        // 播放动画
        if (animator != null && !string.IsNullOrWhiteSpace(triggerName))
        {
            animator.SetTrigger(triggerName);
        }

        // 播放音效
        if (audioSource != null)
        {
            if (teleportSFX != null)
            {
                audioSource.PlayOneShot(teleportSFX);
            }
            else
            {
                audioSource.Play();
            }
        }

        yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(targetSceneName);
    }
}