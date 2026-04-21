using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BGMDialogueSequence : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip firstBGM;
    public AudioClip secondBGM;

    [Header("Dialogue UI")]
    public GameObject dialogueRoot;
    public RectTransform dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Book Logo UI")]
    public GameObject taskRoot;

    [Header("Inventory Logo UI")]
    public GameObject inventoryLogoRoot;

    [Header("Fade")]
    public CanvasGroup fadeOverlay;
    public float fadeDuration = 1.5f;
    public string nextSceneName = "Mus¨¦e d'Orsay";

    [Header("First Dialogue Content")]
    [TextArea(2, 5)]
    public string[] firstDialogueLines;

    [Header("Second Dialogue Content")]
    [TextArea(2, 5)]
    public string[] secondDialogueLines;

    [Header("Third Dialogue Content")]
    [TextArea(2, 5)]
    public string[] thirdDialogueLines;

    [Header("Typing Settings")]
    public float typingSpeed = 0.08f;
    public float waitAfterLine = 1.0f;

    [Header("Panel Settings")]
    public float maxTextWidth = 600f;
    public float minPanelWidth = 120f;
    public float panelPaddingX = 30f;
    public float panelPaddingY = 20f;

    private bool secondDialogueStarted = false;
    private bool thirdDialogueStarted = false;

    void Start()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        if (taskRoot != null)
            taskRoot.SetActive(false);

        if (inventoryLogoRoot != null)
            inventoryLogoRoot.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        if (fadeOverlay != null)
            fadeOverlay.alpha = 0f;

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        if (firstBGM != null)
        {
            audioSource.clip = firstBGM;
            audioSource.loop = false;
            audioSource.Play();
            yield return new WaitUntil(() => !audioSource.isPlaying);
        }

        if (secondBGM != null)
        {
            audioSource.clip = secondBGM;
            audioSource.loop = false;
            audioSource.Play();
            yield return new WaitUntil(() => !audioSource.isPlaying);
        }

        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        for (int i = 0; i < firstDialogueLines.Length; i++)
        {
            yield return StartCoroutine(TypeLine(firstDialogueLines[i]));
            yield return new WaitForSeconds(waitAfterLine);
        }

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        if (taskRoot != null)
            taskRoot.SetActive(true);
    }

    public void StartSecondDialogue()
    {
        if (!secondDialogueStarted)
        {
            secondDialogueStarted = true;

            if (inventoryLogoRoot != null)
                inventoryLogoRoot.SetActive(false);

            StartCoroutine(PlaySecondDialogueSequence());
        }
    }

    IEnumerator PlaySecondDialogueSequence()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = "";

        for (int i = 0; i < secondDialogueLines.Length; i++)
        {
            yield return StartCoroutine(TypeLine(secondDialogueLines[i]));
            yield return new WaitForSeconds(waitAfterLine);
        }

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        if (inventoryLogoRoot != null)
            inventoryLogoRoot.SetActive(true);
    }

    public void StartThirdDialogue()
    {
        if (!thirdDialogueStarted)
        {
            thirdDialogueStarted = true;
            StartCoroutine(PlayThirdDialogueSequence());
        }
    }

    IEnumerator PlayThirdDialogueSequence()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = "";

        for (int i = 0; i < thirdDialogueLines.Length; i++)
        {
            yield return StartCoroutine(TypeLine(thirdDialogueLines[i]));
            yield return new WaitForSeconds(waitAfterLine);
        }

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        yield return StartCoroutine(FadeAndLoadScene());
    }

    IEnumerator FadeAndLoadScene()
    {
        if (fadeOverlay != null)
        {
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }

            fadeOverlay.alpha = 1f;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator TypeLine(string line)
    {
        if (dialogueText == null || dialoguePanel == null) yield break;

        RectTransform textRect = dialogueText.GetComponent<RectTransform>();

        Vector2 preferredSize = dialogueText.GetPreferredValues(line, maxTextWidth, 0f);

        float finalTextWidth = Mathf.Min(preferredSize.x, maxTextWidth);
        float finalTextHeight = preferredSize.y;

        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalTextWidth);
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalTextHeight);

        float panelWidth = Mathf.Max(minPanelWidth, finalTextWidth + panelPaddingX * 2f);
        float panelHeight = finalTextHeight + panelPaddingY * 2f;

        dialoguePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
        dialoguePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);

        LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanel);

        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}