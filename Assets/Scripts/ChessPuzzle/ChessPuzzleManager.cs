using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ChessPuzzleManager : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public TMP_Text infoText;

    [Header("Layer Masks")]
    public LayerMask pieceLayer;
    public LayerMask squareLayer;

    [Header("Puzzle Answer")]
    public ChessPiece correctPiece;
    public BoardSquare correctTargetSquare;

    [Header("Success Knockdown")]
    public ChessPiece successTargetPiece;
    public float knockDownDelay = 0.08f;
    public float knockDownDuration = 0.6f;
    public Vector3 knockDownEuler = new Vector3(0f, 0f, -90f);
    public float knockDownPushDistance = 0.12f;
    public float knockDownDropDistance = 0.02f;
    public bool disableTargetAfterKnockdown = false;

    [Header("Level Transition")]
    public bool loadNextLevelOnSuccess = true;
    public string nextSceneName;
    public float nextSceneDelay = 2f;

    [Header("Options")]
    public bool resetOnWrongMove = true;
    public bool forceShowCursorOnStart = true;

    [Header("Persistent IDs")]
    public string puzzleId = "MonaLisaChess";
    public string successFlagId = "MonaLisa.glassRemoved";

    private ChessPiece selectedPiece;
    private BoardSquare originalSquare;
    private Vector3 originalWorldPosition;
    private Quaternion originalWorldRotation;

    private bool solved = false;
    private bool isBusy = false;

    private void Start()
    {
        if (forceShowCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (infoText != null)
            infoText.text = "White to move. Checkmate in one.";
    }

    private void Update()
    {
        if (solved || isBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedPiece == null)
            {
                SelectPiece();
            }
            else
            {
                TryDeselectOrChooseTarget();
            }
        }
    }

    private void SelectPiece()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, pieceLayer))
        {
            ChessPiece piece = hit.collider.GetComponentInParent<ChessPiece>();

            if (piece != null && piece.color == PieceColor.White)
            {
                ClearCurrentSelectionImmediate();

                selectedPiece = piece;
                originalSquare = piece.currentSquare;
                originalWorldPosition = piece.transform.position;
                originalWorldRotation = piece.transform.rotation;

                selectedPiece.LiftUp();
                selectedPiece.SetSelectionEffects(true);

                if (SFXPlayer.Instance != null)
                    SFXPlayer.Instance.PlaySelect();

                if (infoText != null)
                    infoText.text = "Selected: " + piece.type + ". Choose a target square.";
            }
        }
    }

    private void TryDeselectOrChooseTarget()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit pieceHit, 100f, pieceLayer))
        {
            ChessPiece clickedPiece = pieceHit.collider.GetComponentInParent<ChessPiece>();

            if (clickedPiece != null)
            {
                // 再点同一个白棋：取消选择
                if (clickedPiece == selectedPiece)
                {
                    selectedPiece.SetSelectionEffects(false);
                    selectedPiece.PutDown();
                    selectedPiece = null;

                    if (infoText != null)
                        infoText.text = "White to move. Checkmate in one.";

                    return;
                }

                // 点另一个白棋：切换选择
                if (clickedPiece.color == PieceColor.White)
                {
                    ClearCurrentSelectionImmediate();

                    selectedPiece = clickedPiece;
                    originalSquare = clickedPiece.currentSquare;
                    originalWorldPosition = clickedPiece.transform.position;
                    originalWorldRotation = clickedPiece.transform.rotation;

                    selectedPiece.LiftUp();
                    selectedPiece.SetSelectionEffects(true);

                    if (SFXPlayer.Instance != null)
                        SFXPlayer.Instance.PlaySelect();

                    if (infoText != null)
                        infoText.text = "Selected: " + clickedPiece.type + ". Choose a target square.";

                    return;
                }
            }
        }

        SelectTargetSquare();
    }

    private void SelectTargetSquare()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, squareLayer))
        {
            BoardSquare square = hit.collider.GetComponent<BoardSquare>();

            if (square != null && selectedPiece != null)
            {
                StartCoroutine(ResolveMove(selectedPiece, square));
            }
        }
    }

    private IEnumerator ResolveMove(ChessPiece piece, BoardSquare targetSquare)
    {
        isBusy = true;

        Vector3 attackerStartPosition = originalWorldPosition;

        piece.SetSelectionEffects(false);

        if (SFXPlayer.Instance != null)
            SFXPlayer.Instance.PlayMove();

        // 缓慢移动到目标格
        yield return StartCoroutine(piece.SmoothSnapToSquareRoutine(targetSquare));

        // 正确答案
        if (piece == correctPiece && targetSquare == correctTargetSquare)
        {
            solved = true;

            if (infoText != null)
                infoText.text = "Success! Checkmate in one.";

            if (SFXPlayer.Instance != null)
                SFXPlayer.Instance.PlaySuccess();

            if (successTargetPiece != null)
            {
                yield return StartCoroutine(
                    KnockDownCapturedPiece(piece, successTargetPiece, attackerStartPosition)
                );
            }

            selectedPiece = null;
            isBusy = false;

            if (loadNextLevelOnSuccess && !string.IsNullOrEmpty(nextSceneName))
            {
                yield return StartCoroutine(LoadNextSceneAfterDelay());
            }

            yield break;
        }

        // 错误答案
        if (infoText != null)
            infoText.text = "Wrong move. Try again.";

        if (SFXPlayer.Instance != null)
            SFXPlayer.Instance.PlayWrong();

        if (resetOnWrongMove)
        {
            yield return StartCoroutine(
                piece.SmoothRestoreStateRoutine(originalSquare, originalWorldPosition, originalWorldRotation)
            );
        }

        selectedPiece = null;
        isBusy = false;
    }

    private IEnumerator KnockDownCapturedPiece(ChessPiece attacker, ChessPiece target, Vector3 attackerStartPosition)
    {
        yield return new WaitForSeconds(knockDownDelay);

        if (target == null) yield break;

        if (SFXPlayer.Instance != null)
            SFXPlayer.Instance.PlayKnockdown();

        Vector3 startPosition = target.transform.position;
        Quaternion startRotation = target.transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(knockDownEuler);

        Vector3 pushDirection = startPosition - attackerStartPosition;
        pushDirection.y = 0f;

        if (pushDirection.sqrMagnitude < 0.0001f && attacker != null)
        {
            pushDirection = attacker.transform.forward;
            pushDirection.y = 0f;
        }

        if (pushDirection.sqrMagnitude < 0.0001f)
            pushDirection = Vector3.right;

        pushDirection.Normalize();

        Vector3 endPosition = startPosition
                            + pushDirection * knockDownPushDistance
                            + Vector3.down * knockDownDropDistance;

        float time = 0f;
        while (time < knockDownDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / knockDownDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            target.transform.position = Vector3.Lerp(startPosition, endPosition, easedT);
            target.transform.rotation = Quaternion.Slerp(startRotation, endRotation, easedT);

            yield return null;
        }

        target.transform.position = endPosition;
        target.transform.rotation = endRotation;

        if (disableTargetAfterKnockdown)
        {
            target.gameObject.SetActive(false);
        }
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(nextSceneDelay);

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.MarkPuzzleCompleted(puzzleId);

            if (!string.IsNullOrWhiteSpace(successFlagId))
                GameStateManager.Instance.SetFlag(successFlagId, true);

            GameStateManager.Instance.ReturnFromPuzzle();
            yield break;
        }

        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void ClearCurrentSelectionImmediate()
    {
        if (selectedPiece == null) return;

        selectedPiece.SetSelectionEffects(false);
        selectedPiece.RestoreState(originalSquare, originalWorldPosition, originalWorldRotation);
        selectedPiece = null;
    }
}