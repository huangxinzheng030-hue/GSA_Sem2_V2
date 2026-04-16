using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MonaLisaPuzzleController : MonoBehaviour
{
    [Header("Player")]
    public Camera playerCamera;
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public Transform interactTarget;   // 拖 Drawer

    [Header("UI")]
    public GameObject promptUI;
    public TMP_Text promptText;
    public string interactPrompt = "Press E to Interact";

    [Header("Drawer")]
    public Animator drawerAnimator;
    public string drawerOpenTrigger = "Open";
    public string drawerOpenedStateName = "OpenDrawer";
    public GameObject chessModel;

    [Header("Glass")]
    public GameObject glassObject;     // 直接拖玻璃物体，不再用 Animator

    [Header("Scene")]
    public string chessSceneName = "ChessPuzzle";

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        // 恢复抽屉状态
        if (PuzzleProgress.drawerOpened)
        {
            if (drawerAnimator != null)
                drawerAnimator.Play(drawerOpenedStateName, 0, 1f);

            if (chessModel != null)
                chessModel.SetActive(true);
        }
        else
        {
            if (chessModel != null)
                chessModel.SetActive(false);
        }

        // 玻璃状态恢复 / 处理
        if (glassObject != null)
        {
            // 如果谜题已经完成，回来后直接消失
            if (PuzzleProgress.chessCompleted)
            {
                glassObject.SetActive(false);
                PuzzleProgress.glassRemoved = true;
            }
            else
            {
                glassObject.SetActive(!PuzzleProgress.glassRemoved);
            }
        }
    }

    private void Update()
    {
        bool canInteract = CanInteractWithDrawer();

        // 谜题完成后不再显示这个抽屉交互提示
        if (promptUI != null)
            promptUI.SetActive(canInteract && !PuzzleProgress.chessCompleted);

        if (canInteract && promptText != null && !PuzzleProgress.chessCompleted)
        {
            promptText.text = interactPrompt;
        }

        if (canInteract && Input.GetKeyDown(KeyCode.E) && !PuzzleProgress.chessCompleted)
        {
            if (!PuzzleProgress.drawerOpened)
            {
                OpenDrawer();
            }
            else
            {
                EnterChessScene();
            }
        }
    }

    private bool CanInteractWithDrawer()
    {
        if (playerCamera == null || interactTarget == null)
            return false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            if (hit.transform == interactTarget || hit.transform.IsChildOf(interactTarget))
                return true;
        }

        return false;
    }

    private void OpenDrawer()
    {
        if (drawerAnimator != null)
            drawerAnimator.SetTrigger(drawerOpenTrigger);

        if (chessModel != null)
            chessModel.SetActive(true);

        PuzzleProgress.drawerOpened = true;
    }

    private void EnterChessScene()
    {
        SceneManager.LoadScene(chessSceneName);
    }
}