using System;
using UnityEngine;

public class ClickObjectCircle : MonoBehaviour
{
    public int clickIndex = 0;
    public CirclePuzzle circlePuzzle;

    // 鼠标按下时记录的初始角度与旋转步数
    private float dragAngle = 0f;
    private int saveRotation = 0;
    private int lastRotationForSound = 0;

    // 与本圆圈联动的其他圆圈
    [SerializeField]
    public LinkToIndex[] linkTo;

    // ─────────────────────────────────────────────

    void OnMouseDown()
    {
        if (circlePuzzle.gamePause) return;

        // 计算鼠标相对圆圈中心的角度
        Vector3 offset = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        dragAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;

        // 记录初始旋转步数
        saveRotation = circlePuzzle.circleRotation[clickIndex];
        lastRotationForSound = saveRotation;

        // 记录所有联动圆圈的初始旋转步数
        for (int i = 0; i < linkTo.Length; i++)
        {
            linkTo[i].saveRotation = circlePuzzle.circleRotation[linkTo[i].index];
        }
    }

    void OnMouseDrag()
    {
        if (circlePuzzle.gamePause) return;

        // 当前角度与初始角度的差值（单位：度）
        Vector3 offset = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        int deltaAngle = (int)(Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg - dragAngle);

        // 更新主圆圈旋转（每步 18 度）
        circlePuzzle.circleRotation[clickIndex] = (-deltaAngle / 18) + saveRotation;

        // 旋转步数变化时播放音效
        if (lastRotationForSound != circlePuzzle.circleRotation[clickIndex])
        {
            lastRotationForSound = circlePuzzle.circleRotation[clickIndex];
            circlePuzzle.PlayDragSound(clickIndex);
        }

        // 更新所有联动圆圈（dir 控制同向/反向）
        for (int i = 0; i < linkTo.Length; i++)
        {
            circlePuzzle.circleRotation[linkTo[i].index] =
                (-(deltaAngle * linkTo[i].dir) / 18) + linkTo[i].saveRotation;
        }
    }
}

/// <summary>
/// 描述一个与当前圆圈联动的圆圈索引及旋转方向
/// </summary>
[Serializable]
public class LinkToIndex
{
    [HideInInspector]
    public int saveRotation = 0;

    public int index = 0;

    // 1 = 反向旋转，-1 = 同向旋转
    public int dir = 1;
}
