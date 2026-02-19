using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HintTextController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text hintText;

    [Header("Texts")]
    [TextArea(3, 10)]
    public string hiddenText;

    [TextArea(3, 10)]
    public string revealedText;

    [Header("Reveal Effect")]
    [Tooltip("Seconds between each revealed character.")]
    [Range(0.005f, 0.2f)]
    public float revealInterval = 0.02f;

    [Tooltip("Only '█' will be replaced progressively.")]
    public char maskChar = '█';

    [Header("Optional SFX")]
    public AudioSource sfxSource;
    public AudioClip revealTick;

    private Coroutine revealCo;

    void Awake()
    {
        if (!hintText) hintText = GetComponent<TMP_Text>();
    }

    public void ShowHidden()
    {
        StopReveal();
        if (hintText) hintText.text = hiddenText ?? "";
    }

    public void RevealProgressively()
    {
        StopReveal();
        revealCo = StartCoroutine(RevealCoroutine());
    }

    private void StopReveal()
    {
        if (revealCo != null)
        {
            StopCoroutine(revealCo);
            revealCo = null;
        }
    }

    private IEnumerator RevealCoroutine()
    {
        if (!hintText) yield break;

        string h = hiddenText ?? "";
        string r = revealedText ?? "";

        // If lengths differ, we still try best effort:
        // build a buffer that starts as hidden (or revealed if hidden empty).
        char[] buffer = h.Length > 0 ? h.ToCharArray() : r.ToCharArray();
        hintText.text = new string(buffer);

        int len = Mathf.Min(buffer.Length, r.Length);

        for (int i = 0; i < len; i++)
        {
            // Only replace masked chars, and only if target has a meaningful char.
            if (buffer[i] == maskChar)
            {
                buffer[i] = r[i];
                hintText.text = new string(buffer);

                if (sfxSource && revealTick) sfxSource.PlayOneShot(revealTick);
                yield return new WaitForSeconds(revealInterval);
            }
        }

        // Ensure final text is correct
        hintText.text = r;
        revealCo = null;
    }
}
