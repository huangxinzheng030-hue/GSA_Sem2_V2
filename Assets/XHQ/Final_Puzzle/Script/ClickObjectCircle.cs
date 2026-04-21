using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickObjectCircle : MonoBehaviour
{
    public int clickIndex = 0;
    public CirclePuzzle circlePuzzle;
    private int saveRotation = 0;
    private float dragAngle = 0;

    [SerializeField]
    public LinkToIndex[] linkTo;

    private int lastSaveForPlaySound = 0;

    void OnMouseDown()
    {
        if (!circlePuzzle.gamePause)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
            pos = Input.mousePosition - pos;
            dragAngle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;

            saveRotation = circlePuzzle.circleRotation[clickIndex];
            lastSaveForPlaySound = saveRotation;

            for (int i = 0; i < linkTo.Length; i++)
            {
                linkTo[i].saveRotation = circlePuzzle.circleRotation[linkTo[i].index];
            }
        }
    }

    private void OnMouseDrag()
    {
        if (!circlePuzzle.gamePause)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
            pos = Input.mousePosition - pos;
            int rotation = (int)((Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg) - dragAngle);

            circlePuzzle.circleRotation[clickIndex] = (-rotation / 18) + saveRotation;

            if (lastSaveForPlaySound != circlePuzzle.circleRotation[clickIndex])
            {
                lastSaveForPlaySound = circlePuzzle.circleRotation[clickIndex];
                circlePuzzle.PlayDragSound(clickIndex);
            }

            for (int i = 0; i < linkTo.Length; i++)
            {
                circlePuzzle.circleRotation[linkTo[i].index] = (-(rotation * linkTo[i].dir) / 18) + linkTo[i].saveRotation;
            }
        }
    }
}

[Serializable]
public class LinkToIndex
{
    [HideInInspector]
    public int saveRotation = 0;
    public int index = 0;
    public int dir = 1;
}
