using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BagPaintingAlphaTrigger : MonoBehaviour
{
    [Header("Required Painting UI Images")]
    public Image[] requiredPaintingImages;

    [Header("Alpha Check")]
    [Tooltip("Enable this if unlocked paintings become visible. Disable it if unlocked paintings become transparent.")]
    public bool unlockedWhenAlphaGreaterThan = true;

    [Range(0f, 1f)]
    public float alphaThreshold = 0.5f;

    [Header("Trigger Once")]
    public string storyFlagId = "Story.AllRequiredPaintingsUnlocked";
    public bool triggerOnlyOnce = true;

    [Header("Check Timing")]
    public float checkInterval = 0.5f;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [TextArea(2, 5)]
    public string dialogueContent = "All required paintings have been collected. The exit is now unlocked.";

    public float dialogueDuration = 3f;

    [Header("Exit Unlock - Objects To Show")]
    public GameObject[] exitObjects;

    [Header("Exit Unlock - Objects To Hide")]
    public GameObject[] blockedObjects;

    [Header("Exit Trigger")]
    public Collider exitCollider;

    [Header("Optional Audio")]
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
        // Show multiple exit-related objects.
        if (exitObjects != null)
        {
            foreach (GameObject obj in exitObjects)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        // Hide multiple blocking objects.
        if (blockedObjects != null)
        {
            foreach (GameObject obj in blockedObjects)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        // Enable the exit trigger.
        if (exitCollider != null)
            exitCollider.enabled = true;

        Debug.Log("BagPaintingAlphaTrigger: All required paintings are unlocked. Exit is now available.");
    }
}