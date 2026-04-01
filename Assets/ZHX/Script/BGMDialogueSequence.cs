using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

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

    [Header("Dialogue Content")]
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;
    public float waitAfterLine = 1f;

    [Header("Panel Settings")]
    public float maxTextWidth = 600f;
    public float minPanelWidth = 120f;
    public float panelPaddingX = 30f;
    public float panelPaddingY = 20f;

    void Start()
    {
        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(false);
        }

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
        {
            dialogueRoot.SetActive(true);
        }

        for (int i = 0; i < dialogueLines.Length; i++)
        {
            yield return StartCoroutine(TypeLine(dialogueLines[i]));
            yield return new WaitForSeconds(waitAfterLine);
        }
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