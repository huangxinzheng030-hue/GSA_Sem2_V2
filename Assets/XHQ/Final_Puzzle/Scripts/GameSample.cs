using UnityEngine;

public class GameSample : MonoBehaviour
{
    public CirclePuzzle circlePuzzle;
    public GameObject showUI;

    [Header("提示面板（可选）")]
    public HintPanel hintPanel;

    void Start()
    {
        circlePuzzle.onComplete += OnComplete;
    }

    private void OnComplete()
    {
        hintPanel?.HideHint();
        showUI.SetActive(true);
        circlePuzzle.gamePause = true;
    }

    /// <summary>
    /// 点击"Steal It"按钮：关闭 Complete 面板
    /// </summary>
    public void StealIt()
    {
        showUI.SetActive(false);
    }
}
