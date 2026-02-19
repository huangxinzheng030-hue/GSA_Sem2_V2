using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContractSignFlow : MonoBehaviour
{
    [Header("UI")]
    public GameObject choicePanel;   
    public Button signButton;
    public Button declineButton;

    [Header("Signature Animation")]
    public Animator signatureAnimator; 
    public string signTriggerName = "Play";
    public float signClipLength = 1.0f;     
    public float extraHoldAfterSign = 1.2f;

    [Header("Fade & Scene")]
    public CanvasGroup fadeOverlay;    
    public float fadeToBlackTime = 0.6f;
    public string nextSceneName = "HeistTutorial";

    Coroutine routine;

    public void OnClickSign()
    {
        if (routine != null) return;
        routine = StartCoroutine(SignFlow());
    }

    IEnumerator SignFlow()
    {
        if (signButton != null) signButton.interactable = false;
        if (declineButton != null) declineButton.interactable = false;

        if (choicePanel != null) choicePanel.SetActive(false);

        if (signatureAnimator != null)
        {
            signatureAnimator.gameObject.SetActive(true);
            signatureAnimator.ResetTrigger(signTriggerName);
            signatureAnimator.SetTrigger(signTriggerName);
            yield return new WaitForSeconds(Mathf.Max(0.01f, signClipLength));
        }

        if (extraHoldAfterSign > 0f)
            yield return new WaitForSeconds(extraHoldAfterSign);

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            yield return FadeCanvasGroup(fadeOverlay, fadeOverlay.alpha, 1f, fadeToBlackTime);
        }

        SceneManager.LoadScene(nextSceneName);
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

    public void OnClickDecline()
    {
      
        // SceneManager.LoadScene("StartMenu");
    }
}
