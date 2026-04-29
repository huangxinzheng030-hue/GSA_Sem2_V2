using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BagPaintingAlphaTrigger : MonoBehaviour
{
    [Header("Painting UI Images to check")]
    public Image[] requiredPaintingImages;

    [Header("Alpha check")]
    [Tooltip("Check this if the image is visible when unlocked. If the image becomes fully transparent when unlocked, uncheck this.")]
    public bool unlockedWhenAlphaGreaterThan = true;

    [Range(0f, 1f)]
    public float alphaThreshold = 0.5f;

    [Header("Trigger story only once")]
    public string storyFlagId = "Story.AllRequiredPaintingsUnlocked";
    public bool triggerOnlyOnce = true;

    [Header("Check interval")]
    public float checkInterval = 0.5f;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [TextArea(2, 5)]
    public string dialogueContent = "All key paintings have been collected. The exit is now open.";

    public float dialogueDuration = 3f;

    [Header("Exit unlock")]
    public GameObject exitObject;
    public GameObject blockedObject;
    public Collider exitCollider;

    [Header("Optional audio")]
    public AudioSource storyAudio;

    private bool triggered = false;

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (triggerOnlyOnce &&
            GameStateManager.Instance != null &&
            GameStateManager.Instance.GetFlag(storyFlagId))
        {
            triggered = true;
            UnlockExit();
            return;
        }

        StartCoroutine(CheckLoop());
    }

    private IEnumerator CheckLoop()
    {
        while (!triggered)
        {
            CheckPaintings();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void CheckPaintings()
    {
        if (triggered) return;

        if (requiredPaintingImages == null || requiredPaintingImages.Length == 0)
            return;

        foreach (Image img in requiredPaintingImages)
        {
            if (img == null)
                return;

            float alpha = img.color.a;

            bool unlocked;

            if (unlockedWhenAlphaGreaterThan)
            {
                unlocked = alpha >= alphaThreshold;
            }
            else
            {
                unlocked = alpha <= alphaThreshold;
            }

            if (!unlocked)
                return;
        }

        triggered = true;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetFlag(storyFlagId, true);

        StartCoroutine(TriggerStory());
    }

    private IEnumerator TriggerStory()
    {
        if (storyAudio != null)
            storyAudio.Play();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = dialogueContent;

        yield return new WaitForSeconds(dialogueDuration);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        UnlockExit();
    }

    private void UnlockExit()
    {
        if (exitObject != null)
            exitObject.SetActive(true);

        if (blockedObject != null)
            blockedObject.SetActive(false);

        if (exitCollider != null)
            exitCollider.enabled = true;

        Debug.Log("BagPaintingAlphaTrigger: All specified paintings have been unlocked. Exit opened.");
    }
}