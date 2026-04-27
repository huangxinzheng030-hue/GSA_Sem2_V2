using UnityEngine;

public class PuzzleReturn : MonoBehaviour
{
    [Header("Optional")]
    public string puzzleId;
    public string successFlagId;

    public void ReturnToPreviousScene()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("PuzzleReturn: GameStateManager.Instance ²»´æÔÚ¡£");
            return;
        }

        ISceneFlowService flow = GameStateManager.Instance;

        if (!string.IsNullOrWhiteSpace(puzzleId))
            flow.MarkPuzzleCompleted(puzzleId);

        if (!string.IsNullOrWhiteSpace(successFlagId))
            flow.SetFlag(successFlagId, true);

        flow.ReturnFromPuzzle();
    }
}