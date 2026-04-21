using UnityEngine;
using UnityEngine.Playables;

public class IntroTimelineReturnToPlayer : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector director;

    [Header("Camera Roots")]
    [SerializeField] private GameObject animationCameraRoot; // 你的 Animation Camera
    [SerializeField] private GameObject playerCameraRoot;    // 你的玩家相机（或玩家主相机物体）

    [Header("Player Control Scripts")]
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    // 例如：PlayerMovement、MouseLook、FirstPersonController 等

    [Header("Cursor After Intro")]
    [SerializeField] private bool lockCursorAfterIntro = true;
    [SerializeField] private bool hideCursorAfterIntro = true;

    private void Awake()
    {
        if (director != null)
        {
            director.stopped += OnTimelineStopped;
        }
    }

    private void Start()
    {
        // 开场时：动画相机开，玩家相机关
        if (animationCameraRoot != null)
            animationCameraRoot.SetActive(true);

        if (playerCameraRoot != null)
            playerCameraRoot.SetActive(false);

        SetPlayerControl(false);
    }

    private void OnTimelineStopped(PlayableDirector pd)
    {
        // Timeline播完后：关动画相机，开玩家相机
        if (animationCameraRoot != null)
            animationCameraRoot.SetActive(false);

        if (playerCameraRoot != null)
            playerCameraRoot.SetActive(true);

        SetPlayerControl(true);

        if (lockCursorAfterIntro)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        Cursor.visible = !hideCursorAfterIntro;
    }

    private void SetPlayerControl(bool enabledState)
    {
        if (playerControlScripts == null) return;

        foreach (var script in playerControlScripts)
        {
            if (script != null)
                script.enabled = enabledState;
        }
    }

    private void OnDestroy()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }
    }
}