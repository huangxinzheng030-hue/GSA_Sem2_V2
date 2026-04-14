using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSample : MonoBehaviour
{
    public CirclePuzzle circlePuzzle;

    public GameObject showUI;

    void Start ()
    {
        circlePuzzle.onComplete += OnComplete;
    }

    private void OnComplete()
    {
        showUI.SetActive(true);
        circlePuzzle.gamePause = true;
    }

    public void CloseUI ()
    {
        showUI.SetActive(false);
        circlePuzzle.gamePause = false;
        circlePuzzle.Setup();
    }
}
