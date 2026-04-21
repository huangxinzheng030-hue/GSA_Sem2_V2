using UnityEngine;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("圆环列表（从内到外）")]
    public PuzzleRing[] rings;

    [Header("UI 提示（可选）")]
    public TMP_Text ringIndexText;

    private bool isSolved = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (var ring in rings)
            ring.Randomize();
    }

    public void CheckSolved()
    {
        if (isSolved) return;

        foreach (var ring in rings)
        {
            if (!ring.IsSolved()) return;
        }

        isSolved = true;
        Debug.Log("The puzzle is complete!");
        DrawerController.Instance.OpenDrawer();
    }
}