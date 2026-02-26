using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SignatureChoiceFlow : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject buttonsRoot;
    public Button signButton;
    public Button declineButton;

    [Header("Signature (Frame Animation)")]
    public Image signatureImage;
    public Sprite[] signatureFrames;
    public float fps = 24f;
    public bool hideSignatureOnStart = true;

    [Header("Timing (Sign -> Fade)")]
    [Tooltip("Wait after the signature animation finishes (stay on the scene).")]
    public float holdAfterSignature = 2f;

    [Tooltip("Optional extra delay before starting the fade.")]
    public float delayBeforeFade = 0f;

    [Header("Fade Overlay (Black Screen)")]
    public CanvasGroup fadeOverlay;
    public float fadeToBlackTime = 0.6f;

    [Header("Intertitle (Black Screen Title)")]
    public TMP_Text intertitleText;
    [TextArea(1, 2)] public string intertitle = "Mus¨¦e d'Orsay";
    public float intertitleHoldSeconds = 3.5f;

    [Header("Scenes")]
    public string nextSceneName = "S2";

    [Header("Decline -> Threat Flow")]
    public GameObject threatPanel;
    public TMP_Text threatText;
    [TextArea(1, 2)] public string threatLine = "You think you still have a way back?";
    public float threatCharInterval = 0.04f;
    public float threatTextHold = 1.2f;

    [Header("SFX (Signature Writing)")]
    public AudioSource writingSfxSource;
    public AudioClip writingClip;
    [Range(0f, 1f)] public float writingVolume = 0.8f;
    public bool loopWritingSfx = true;
    public float writingStartDelay = 0f;

    [Header("SFX (Threat / Chamber)")]
    public AudioSource threatSfxSource;
    public AudioClip chamberSfx;
    public float chamberDelay = 0.15f;

    Coroutine routine;
    Coroutine writingSfxRoutine;

    void Awake()
    {
        if (signButton != null) signButton.onClick.AddListener(OnSign);
        if (declineButton != null) declineButton.onClick.AddListener(OnDecline);
    }

    void Start()
    {
        // Buttons default: do NOT show unless you call ShowButtons/ShowSignOnly
        if (buttonsRoot != null) buttonsRoot.SetActive(false);

        // Signature defaults
        if (signatureImage != null)
        {
            signatureImage.color = Color.white;

            if (hideSignatureOnStart)
            {
                signatureImage.enabled = false;
                // Do NOT clear sprite here; keep your editor preview
            }
            else
            {
                signatureImage.enabled = true;
            }
        }

        // Fade defaults
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
            fadeOverlay.interactable = false;
            fadeOverlay.gameObject.SetActive(true);
        }

        // Intertitle defaults
        if (intertitleText != null)
        {
            intertitleText.gameObject.SetActive(false);
        }

        // Threat defaults
        if (threatPanel != null) threatPanel.SetActive(false);
        if (threatText != null) threatText.text = "";

        StopWritingSfx();
    }

    // Call this when camera switch finished (your DialogueTypewriter end)
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
            declineButton.gameObject.SetActive(true);
            declineButton.interactable = true;
        }
    }

    void ShowSignOnly()
    {
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

    void HideButtons()
    {
        if (signButton != null) signButton.interactable = false;
        if (declineButton != null) declineButton.interactable = false;

        if (buttonsRoot != null) buttonsRoot.SetActive(false);
    }

    public void OnSign()
    {
        if (routine != null) return;
        routine = StartCoroutine(SignFlow());
    }

    IEnumerator SignFlow()
    {
        HideButtons();

        if (threatPanel != null) threatPanel.SetActive(false);

        // Start writing sfx (optional)
        StartWritingSfx();

        // Play signature frames
        yield return PlaySignatureOnce();

        // Stop writing sfx when signature finishes
        StopWritingSfx();

        // Hold on scene after signature
        if (holdAfterSignature > 0f)
            yield return new WaitForSeconds(holdAfterSignature);

        // Optional delay before fade
        if (delayBeforeFade > 0f)
            yield return new WaitForSeconds(delayBeforeFade);

        // Fade + intertitle + load
        yield return FadeHoldAndLoad(nextSceneName, intertitle, intertitleHoldSeconds);
    }

    public void OnDecline()
    {
        if (routine != null) return;
        routine = StartCoroutine(DeclineThreatFlow());
    }

    IEnumerator DeclineThreatFlow()
    {
        HideButtons();

        // Show threat panel (black background)
        if (threatPanel != null) threatPanel.SetActive(true);

        // Typewriter
        if (threatText != null)
        {
            threatText.text = "";
            if (!string.IsNullOrEmpty(threatLine))
            {
                for (int i = 0; i < threatLine.Length; i++)
                {
                    threatText.text += threatLine[i];
                    if (threatCharInterval > 0f)
                        yield return new WaitForSeconds(threatCharInterval);
                    else
                        yield return null;
                }
            }
        }

        // Chamber SFX
        if (threatSfxSource != null && chamberSfx != null)
        {
            if (chamberDelay > 0f) yield return new WaitForSeconds(chamberDelay);
            threatSfxSource.PlayOneShot(chamberSfx);
        }

        if (threatTextHold > 0f)
            yield return new WaitForSeconds(threatTextHold);

        // Force only sign option
        ShowSignOnly();

        // Allow sign to be clicked now
        routine = null;
    }

    IEnumerator PlaySignatureOnce()
    {
        if (signatureImage == null) yield break;
        if (signatureFrames == null || signatureFrames.Length == 0) yield break;

        signatureImage.enabled = true;

        float frameTime = (fps <= 0f) ? 0.04f : (1f / fps);

        for (int i = 0; i < signatureFrames.Length; i++)
        {
            if (signatureFrames[i] != null)
                signatureImage.sprite = signatureFrames[i];

            if (frameTime > 0f)
                yield return new WaitForSeconds(frameTime);
            else
                yield return null;
        }
    }

    void StartWritingSfx()
    {
        // If no clip/source, do nothing
        if (writingSfxSource == null || writingClip == null) return;

        // Avoid stacking
        StopWritingSfx();

        writingSfxRoutine = StartCoroutine(WritingSfxRoutine());
    }

    IEnumerator WritingSfxRoutine()
    {
        if (writingStartDelay > 0f)
            yield return new WaitForSeconds(writingStartDelay);

        if (writingSfxSource == null || writingClip == null) yield break;

        writingSfxSource.loop = loopWritingSfx;
        writingSfxSource.volume = writingVolume;
        writingSfxSource.clip = writingClip;
        writingSfxSource.Play();
    }

    void StopWritingSfx()
    {
        if (writingSfxRoutine != null)
        {
            StopCoroutine(writingSfxRoutine);
            writingSfxRoutine = null;
        }

        if (writingSfxSource != null)
        {
            // Stop only if we are actually using the writing clip
            if (writingSfxSource.clip == writingClip)
            {
                writingSfxSource.Stop();
                writingSfxSource.clip = null;
            }
        }
    }

    IEnumerator FadeHoldAndLoad(string sceneName, string title, float holdSeconds)
    {
        // Fade to black
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
                    t += Time.deltaTime;
                    fadeOverlay.alpha = Mathf.Lerp(from, 1f, t / fadeToBlackTime);
                    yield return null;
                }
                fadeOverlay.alpha = 1f;
            }
        }

        // Intertitle on black
        if (intertitleText != null && holdSeconds > 0f && !string.IsNullOrEmpty(title))
        {
            intertitleText.text = title;
            intertitleText.gameObject.SetActive(true);
            yield return new WaitForSeconds(holdSeconds);
            intertitleText.gameObject.SetActive(false);
        }
        else if (holdSeconds > 0f)
        {
            yield return new WaitForSeconds(holdSeconds);
        }

        SceneManager.LoadScene(sceneName);
    }
}