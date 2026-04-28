using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 提示面板：显示正确图案的图片和说明文字
/// 用法：将此脚本挂到任意 GameObject，在 Inspector 中绑定对应 UI 组件
/// </summary>
public class HintPanel : MonoBehaviour
{
    [Header("提示面板")]
    // 整个提示面板的根节点（拖入 Panel GameObject）
    public GameObject hintPanel;

    // 显示正确图案的图片
    public Image hintImage;

    // 显示提示文字
    public Text hintText;

    [Header("提示内容")]
    // 在 Inspector 中设置正确图案的图片
    public Sprite correctPatternSprite;

    // 在 Inspector 中设置提示文字
    [TextArea]
    public string hintMessage = "将所有圆圈对齐到相同的图案！";

    [Header("关联")]
    public CirclePuzzle circlePuzzle;

    // ─────────────────────────────────────────────

    void Start()
    {
        // 初始化内容
        if (hintImage != null && correctPatternSprite != null)
            hintImage.sprite = correctPatternSprite;

        if (hintText != null)
            hintText.text = hintMessage;

        // 默认隐藏
        SetPanelVisible(false);
    }

    /// <summary>
    /// 打开提示面板（绑定到"提示"按钮的 OnClick）
    /// </summary>
    public void ShowHint()
    {
        if (circlePuzzle != null && circlePuzzle.gamePause) return;
        SetPanelVisible(true);
    }

    /// <summary>
    /// 关闭提示面板（绑定到面板内"关闭"按钮的 OnClick）
    /// </summary>
    public void HideHint()
    {
        SetPanelVisible(false);
    }

    private void SetPanelVisible(bool visible)
    {
        if (hintPanel != null)
            hintPanel.SetActive(visible);
    }
}
