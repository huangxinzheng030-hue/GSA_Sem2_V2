using UnityEngine;

public class HideBookAndStartDialogue : MonoBehaviour
{
    public KeyCode hideKey = KeyCode.T;

    [Header("Book")]
    public GameObject bookToHide;

    [Header("Book Logo")]
    public GameObject bookLogoToShow;

    [Header("Dialogue")]
    public BGMDialogueSequence dialogueSequence;

    private bool hasTriggered = false;

    void Update()
    {
        if (!hasTriggered && Input.GetKeyDown(hideKey))
        {
            hasTriggered = true;

            if (bookToHide != null)
            {
                bookToHide.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }

            if (bookLogoToShow != null)
            {
                bookLogoToShow.SetActive(true);
            }

            if (dialogueSequence != null)
            {
                dialogueSequence.StartSecondDialogue();
            }
        }
    }
}