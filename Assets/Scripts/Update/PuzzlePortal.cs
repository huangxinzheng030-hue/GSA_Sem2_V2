using UnityEngine;

public class PuzzlePortal : MonoBehaviour
{
    [Header("Puzzle")]
    public string puzzleSceneName;
    public string returnSpawnPointId;

    public void EnterPuzzle()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("PuzzlePortal: GameStateManager.Instance ²»´æÔÚ¡£");
            return;
        }

        ISceneFlowService flow = GameStateManager.Instance;
        flow.EnterPuzzle(puzzleSceneName, returnSpawnPointId);
    }
}