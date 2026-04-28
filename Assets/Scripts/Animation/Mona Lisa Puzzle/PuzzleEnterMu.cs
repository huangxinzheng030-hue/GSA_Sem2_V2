using System.Collections;
using UnityEngine;
using TMPro;

public class PuzzleEnterMu : MonoBehaviour
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
    public GameObject blockObject; // 例如玻璃

    [Header("After Puzzle Return")]
    public GameObject postReturnColliderObject; // 关卡完成后显示的隐藏碰撞体
    public bool startDialogueWhenTouchCollider = true;
    public string dialoguePlayedFlag = ""; // 可以留空，系统会自动生成
    public string playerTag = "Player";

    [Header("Post Return Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [TextArea(2, 4)]
    public string[] dialogueLines;

    public float typeSpeed = 0.04f;
    public float lineHoldTime = 1.2f;

    [Header("Evacuation Teleport")]
    public GameObject evacuationTeleportPoint; // 对话结束后显示的撤离传送点

    private bool dialogueStarted = false;

    private bool IsOpened =>
        GameStateManager.Instance != null &&
        !string.IsNullOrWhiteSpace(openedFlag) &&
        GameStateManager.Instance.GetFlag(openedFlag);

    private bool IsCompleted =>
        GameStateManager.Instance != null &&
        !string.IsNullOrWhiteSpace(puzzleId) &&
        GameStateManager.Instance.IsPuzzleCompleted(puzzleId);

    private bool IsBlockRemoved =>
        GameStateManager.Instance != null &&
        !string.IsNullOrWhiteSpace(completedFlag) &&
        GameStateManager.Instance.GetFlag(completedFlag);

    private bool HasDialoguePlayed
    {
        get
        {
            if (GameStateManager.Instance == null) return false;
            return GameStateManager.Instance.GetFlag(GetDialoguePlayedFlag());
        }
    }

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (evacuationTeleportPoint != null)
            evacuationTeleportPoint.SetActive(false);

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

        SetupPostReturnObjects();
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
            // 如果 openedFlag 没填，说明这个谜题不需要“先打开一步”，直接进入关卡
            if (string.IsNullOrWhiteSpace(openedFlag))
            {
                EnterPuzzle();
                return;
            }

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

    private void SetupPostReturnObjects()
    {
        bool puzzleFinished = IsCompleted || IsBlockRemoved;

        if (postReturnColliderObject != null)
        {
            postReturnColliderObject.SetActive(puzzleFinished);

            if (puzzleFinished && startDialogueWhenTouchCollider)
            {
                PostReturnDialogueTrigger trigger =
                    postReturnColliderObject.GetComponent<PostReturnDialogueTrigger>();

                if (trigger == null)
                    trigger = postReturnColliderObject.AddComponent<PostReturnDialogueTrigger>();

                trigger.owner = this;
                trigger.playerTag = playerTag;
            }
        }

        if (!puzzleFinished)
            return;

        if (HasDialoguePlayed)
        {
            if (evacuationTeleportPoint != null)
                evacuationTeleportPoint.SetActive(true);

            return;
        }

        if (!startDialogueWhenTouchCollider)
        {
            BeginPostReturnDialogue();
        }
    }

    public void BeginPostReturnDialogue()
    {
        if (dialogueStarted) return;
        if (HasDialoguePlayed) return;

        dialogueStarted = true;
        StartCoroutine(TypeDialogueRoutine());
    }

    private IEnumerator TypeDialogueRoutine()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = "";

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            FinishPostReturnDialogue();
            yield break;
        }

        foreach (string line in dialogueLines)
        {
            if (dialogueText != null)
                dialogueText.text = "";

            foreach (char c in line)
            {
                if (dialogueText != null)
                    dialogueText.text += c;

                yield return new WaitForSeconds(typeSpeed);
            }

            yield return new WaitForSeconds(lineHoldTime);
        }

        FinishPostReturnDialogue();
    }

    private void FinishPostReturnDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetFlag(GetDialoguePlayedFlag(), true);

        if (evacuationTeleportPoint != null)
            evacuationTeleportPoint.SetActive(true);

        Debug.Log("Post return dialogue finished. Evacuation teleport point activated.");
    }

    private string GetDialoguePlayedFlag()
    {
        if (!string.IsNullOrWhiteSpace(dialoguePlayedFlag))
            return dialoguePlayedFlag;

        if (!string.IsNullOrWhiteSpace(puzzleId))
            return puzzleId + ".postReturnDialoguePlayed";

        return gameObject.name + ".postReturnDialoguePlayed";
    }
}

public class PostReturnDialogueTrigger : MonoBehaviour
{
    public PuzzleEnterMu owner;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (owner == null) return;

        if (other.CompareTag(playerTag))
        {
            owner.BeginPostReturnDialogue();
        }
    }
}