using UnityEngine;

public class PuzzleBackButton : MonoBehaviour
{
    [Header("Optional Save State")]
    public string puzzleId;
    public string successFlagId;
    public bool markCompletedWhenBack = false;

    public void BackToPreviousScene()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("PuzzleBackButton: GameStateManager.Instance 不存在，无法返回。");
            return;
        }

        // 如果这个按钮只是“退出谜题”，一般不要标记完成
        // 如果你想按这个按钮也算完成，可以勾 markCompletedWhenBack
        if (markCompletedWhenBack)
        {
            if (!string.IsNullOrWhiteSpace(puzzleId))
                GameStateManager.Instance.MarkPuzzleCompleted(puzzleId);

            if (!string.IsNullOrWhiteSpace(successFlagId))
                GameStateManager.Instance.SetFlag(successFlagId, true);
        }

        GameStateManager.Instance.ReturnFromPuzzle();
    }
}