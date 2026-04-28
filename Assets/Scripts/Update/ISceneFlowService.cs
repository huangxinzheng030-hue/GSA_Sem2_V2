public interface ISceneFlowService
{
    void EnterPuzzle(string puzzleSceneName, string returnSpawnPointId);
    void ReturnFromPuzzle();

    bool HasPlayedIntro(string introId);
    void MarkIntroPlayed(string introId);

    void SetFlag(string flagId, bool value);
    bool GetFlag(string flagId);

    void MarkPuzzleCompleted(string puzzleId);
    bool IsPuzzleCompleted(string puzzleId);
}