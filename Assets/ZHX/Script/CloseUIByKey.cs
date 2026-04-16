using UnityEngine;

public class CloseUIByKey : MonoBehaviour
{
    public KeyCode closeKey = KeyCode.Tab;

    [Header("UI To Close")]
    public GameObject targetToClose;

    [Header("Logo To Show Again")]
    public GameObject logoToShow;

    [Header("Dialogue")]
    public BGMDialogueSequence dialogueSequence;

    private bool hasTriggered = false;

    void Update()
    {
        if (!hasTriggered && Input.GetKeyDown(closeKey))
        {
            hasTriggered = true;

            if (targetToClose != null && targetToClose.activeSelf)
            {
                targetToClose.SetActive(false);
            }

            if (logoToShow != null)
            {
                logoToShow.SetActive(true);
            }

            if (dialogueSequence != null)
            {
                dialogueSequence.StartThirdDialogue();
            }
        }
    }
}