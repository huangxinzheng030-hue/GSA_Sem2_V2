using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SignatureChoiceFlow : MonoBehaviour
{
    [Header("UI")]
    public GameObject buttonsRoot;
    public Button signButton;
    public Button declineButton;

    [Header("Signature Image (UI)")]
    public Image signatureImage;
    public Sprite[] signatureFrames;
    public float fps = 24f;
    public bool hideSignatureOnStart = true;

    [Header("Timing")]
    public float delayBeforeFade = 2.0f;
    public float holdAfterSignature = 0.0f;

    [Header("Fade & Scene")]
    public CanvasGroup fadeOverlay;
    public float fadeToBlackTime = 0.6f;

    [Header("Intertitle (Black Screen Title)")]
    public TMP_Text intertitleText;
    [TextArea(1, 2)] public string intertitle = "Mus¨¦e d'Orsay";
    public float intertitleHoldSeconds = 3.5f;

    [Header("Scenes")]
    public string nextSceneName = "S2";

    [Header("Decline -> Threat Flow")]
    public TMP_Text threatText;
    [TextArea(1, 2)] public string threatLine = "You think you still have a way back?";
    public float threatTextHold = 1.2f;

    public AudioSource sfxSource;
    public AudioClip chamberSfx;
    public float chamberDelay = 0.15f;

    Coroutine routine;
    bool forcedSignOnly = false;

    void Awake()
    {
        if (signButton != null) signButton.onClick.AddListener(OnSign);
        if (declineButton != null) declineButton.onClick.AddListener(OnDecline);
    }

    void Start()
    {
        if (signatureImage != null)
        {
            signatureImage.color = Color.white;
            signatureImage.enabled = !hideSignatureOnStart;
            if (hideSignatureOnStart) signatureImage.sprite = null;
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
            fadeOverlay.interactable = false;
        }

        if (intertitleText != null) intertitleText.gameObject.SetActive(false);

        if (threatText != null)
        {
            threatText.gameObject.SetActive(false);
            threatText.text = "";
        }
    }

    public void ShowButtons()
    {
        if (buttonsRoot != null) buttonsRoot.SetActive(true);

        if (signButton != null)
        {
            signButton.gameObject.SetActive(true);
            signButton.interactable = true;
        }

        if (declineButton != null)
        {
            declineButton.gameObject.SetActive(!forcedSignOnly);
            declineButton.interactable = !forcedSignOnly;
        }
    }

    void HideAllButtons()
    {
        if (signButton != null) signButton.interactable = false;
        if (declineButton != null) declineButton.interactable = false;

        if (buttonsRoot != null) buttonsRoot.SetActive(false);
    }

    void ShowSignOnly()
    {
        forcedSignOnly = true;

        if (buttonsRoot != null) buttonsRoot.SetActive(true);

        if (signButton != null)
        {
            signButton.gameObject.SetActive(true);
            signButton.interactable = true;
        }

        if (declineButton != null)
        {
            declineButton.interactable = false;
            declineButton.gameObject.SetActive(false);
        }
    }

    public void OnSign()
    {
        if (routine != null) return;
        routine = StartCoroutine(SignFlow());
    }

    IEnumerator SignFlow()
    {
        HideAllButtons();

        yield return PlaySignatureOnce();

        if (holdAfterSignature > 0f)
            yield return new WaitForSecondsRealtime(holdAfterSignature);

        if (delayBeforeFade > 0f)
            yield return new WaitForSecondsRealtime(delayBeforeFade);

        yield return FadeHoldAndLoad(nextSceneName, intertitle, intertitleHoldSeconds);
    }

    public void OnDecline()
    {
        if (routine != null) return;
        routine = StartCoroutine(DeclineThreatFlow());
    }

    IEnumerator DeclineThreatFlow()
    {
        HideAllButtons();

        if (threatText != null)
        {
            threatText.text = threatLine;
            threatText.gameObject.SetActive(true);
        }

        if (chamberDelay > 0f)
            yield return new WaitForSecondsRealtime(chamberDelay);

        if (sfxSource != null && chamberSfx != null)
            sfxSource.PlayOneShot(chamberSfx);

        if (threatTextHold > 0f)
            yield return new WaitForSecondsRealtime(threatTextHold);

        if (threatText != null)
        {
            threatText.gameObject.SetActive(false);
            threatText.text = "";
        }

        ShowSignOnly();
        routine = null;
    }

    IEnumerator PlaySignatureOnce()
    {
        if (signatureImage == null || signatureFrames == null || signatureFrames.Length == 0)
            yield break;

        signatureImage.enabled = true;

        float frameTime = (fps <= 0f) ? 0.04f : (1f / fps);

        for (int i = 0; i < signatureFrames.Length; i++)
        {
            signatureImage.sprite = signatureFrames[i];
            yield return new WaitForSecondsRealtime(frameTime);
        }
    }

    IEnumerator FadeHoldAndLoad(string sceneName, string title, float holdSeconds)
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            fadeOverlay.interactable = false;

            float from = fadeOverlay.alpha;
            float t = 0f;

            if (fadeToBlackTime <= 0f)
            {
                fadeOverlay.alpha = 1f;
            }
            else
            {
                while (t < fadeToBlackTime)
                {
                    t += Time.unscaledDeltaTime;
                    fadeOverlay.alpha = Mathf.Lerp(from, 1f, t / fadeToBlackTime);
                    yield return null;
                }
                fadeOverlay.alpha = 1f;
            }
        }

        if (intertitleText != null && holdSeconds > 0f && !string.IsNullOrEmpty(title))
        {
            intertitleText.text = title;
            intertitleText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(holdSeconds);
            intertitleText.gameObject.SetActive(false);
        }
        else if (holdSeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(holdSeconds);
        }

        SceneManager.LoadScene(sceneName);
    }
}
