using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContractChoiceUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject choicePanel;
    public GameObject buttonsRoot;          // Only hide buttons, keep this script object active
    public Button signButton;
    public Button declineButton;

    [Header("Signature (Animator)")]
    public Animator signatureAnimator;
    public string playTriggerName = "Play";
    public float signClipLength = 1.2f;
    public float extraHoldAfterSignature = 0.8f;

    [Header("Fade & Scene")]
    public CanvasGroup fadeOverlay;
    public float fadeToBlackTime = 0.6f;
    public string nextSceneName = "HeistTutorial";

    [Header("Auto Setup")]
    public bool hideOnStart = true;

    Coroutine routine;

    void Awake()
    {
        if (signButton != null) signButton.onClick.AddListener(OnSign);
        if (declineButton != null) declineButton.onClick.AddListener(OnDecline);
    }

    void Start()
    {
        if (hideOnStart) Hide();
    }

    public void Show()
    {
        if (choicePanel != null) choicePanel.SetActive(true);

        if (buttonsRoot != null) buttonsRoot.SetActive(true);

        if (signButton != null) signButton.interactable = true;
        if (declineButton != null) declineButton.interactable = true;
    }

    public void Hide()
    {
        if (choicePanel != null) choicePanel.SetActive(true); // IMPORTANT: keep active

        if (buttonsRoot != null) buttonsRoot.SetActive(false);

        if (signButton != null) signButton.interactable = false;
        if (declineButton != null) declineButton.interactable = false;
    }

    public void OnSign()
    {
        if (routine != null) return;
        routine = StartCoroutine(SignFlow());
    }

    IEnumerator SignFlow()
    {
        Hide();

        if (signatureAnimator != null)
        {
            signatureAnimator.gameObject.SetActive(true);
            signatureAnimator.ResetTrigger(playTriggerName);
            signatureAnimator.SetTrigger(playTriggerName);

            if (signClipLength > 0f)
                yield return new WaitForSeconds(signClipLength);
        }

        if (extraHoldAfterSignature > 0f)
            yield return new WaitForSeconds(extraHoldAfterSignature);

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            fadeOverlay.interactable = false;
            yield return FadeCanvasGroup(fadeOverlay, fadeOverlay.alpha, 1f, fadeToBlackTime);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    public void OnDecline()
    {
        if (routine != null) return;
        routine = StartCoroutine(DeclineFlow());
    }

    IEnumerator DeclineFlow()
    {
        Hide();
        yield return null;
        // SceneManager.LoadScene("StartMenu");
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        cg.alpha = from;
        if (duration <= 0f)
        {
            cg.alpha = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        cg.alpha = to;
    }
}
