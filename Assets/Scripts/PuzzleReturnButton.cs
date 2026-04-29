using UnityEngine;

public class PuzzleReturnButton : MonoBehaviour
{
    [Header("Puzzle Result IDs")]
    public string puzzleId = "StarPuzzle";
    public string successFlagId = "StarPuzzle.completed";

    [Header("Options")]
    public bool markPuzzleCompleted = true;
    public bool setSuccessFlag = true;

    public void ReturnToMainScene()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("PuzzleReturnButton: GameStateManager.Instance 不存在，无法返回主场景。");
            return;
        }

        if (markPuzzleCompleted && !string.IsNullOrWhiteSpace(puzzleId))
        {
            GameStateManager.Instance.MarkPuzzleCompleted(puzzleId);
        }

        if (setSuccessFlag && !string.IsNullOrWhiteSpace(successFlagId))
        {
            GameStateManager.Instance.SetFlag(successFlagId, true);
        }

        GameStateManager.Instance.ReturnFromPuzzle();
    }
}