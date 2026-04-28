using UnityEngine;
using TMPro;

public class PuzzleEnter : MonoBehaviour
{
    [Header("Player")]
    public Camera playerCamera;
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public Transform interactTarget;

    [Header("UI")]
    public GameObject promptUI;
    public TMP_Text promptText;
    public string interactPrompt = "Press E to Interact";

    [Header("Optional Animation / Object")]
    public Animator interactAnimator;
    public string openTrigger = "Open";
    public string openedStateName = "OpenDrawer";
    public GameObject revealObject;

    [Header("Puzzle Routing")]
    public string puzzleSceneName = "ChessPuzzle";
    public string returnSpawnPointId = "From_Chess";
    public string puzzleId = "MonaLisaChess";

    [Header("State Flags")]
    public string openedFlag = "MonaLisa.drawerOpened";
    public string completedFlag = "MonaLisa.glassRemoved";

    [Header("Optional Block Object")]
    public GameObject blockObject; // ÀýÈç²£Á§

    private bool IsOpened =>
        GameStateManager.Instance != null && GameStateManager.Instance.GetFlag(openedFlag);

    private bool IsCompleted =>
        GameStateManager.Instance != null && GameStateManager.Instance.IsPuzzleCompleted(puzzleId);

    private bool IsBlockRemoved =>
        GameStateManager.Instance != null && GameStateManager.Instance.GetFlag(completedFlag);

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        if (IsOpened)
        {
            if (interactAnimator != null && !string.IsNullOrWhiteSpace(openedStateName))
                interactAnimator.Play(openedStateName, 0, 1f);

            if (revealObject != null)
                revealObject.SetActive(true);
        }
        else
        {
            if (revealObject != null)
                revealObject.SetActive(false);
        }

        if (blockObject != null)
        {
            if (IsCompleted || IsBlockRemoved)
                blockObject.SetActive(false);
            else
                blockObject.SetActive(true);
        }
    }

    private void Update()
    {
        bool canInteract = CanInteract();

        if (promptUI != null)
            promptUI.SetActive(canInteract && !IsCompleted);

        if (promptText != null && canInteract && !IsCompleted)
            promptText.text = interactPrompt;

        if (canInteract && Input.GetKeyDown(KeyCode.E) && !IsCompleted)
        {
            if (!IsOpened)
                OpenStep();
            else
                EnterPuzzle();
        }
    }

    private bool CanInteract()
    {
        if (playerCamera == null || interactTarget == null)
            return false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            if (hit.transform == interactTarget || hit.transform.IsChildOf(interactTarget))
                return true;
        }

        return false;
    }

    private void OpenStep()
    {
        if (interactAnimator != null && !string.IsNullOrWhiteSpace(openTrigger))
            interactAnimator.SetTrigger(openTrigger);

        if (revealObject != null)
            revealObject.SetActive(true);

        if (GameStateManager.Instance != null && !string.IsNullOrWhiteSpace(openedFlag))
            GameStateManager.Instance.SetFlag(openedFlag, true);
    }

    private void EnterPuzzle()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.EnterPuzzle(puzzleSceneName, returnSpawnPointId);
    }
}