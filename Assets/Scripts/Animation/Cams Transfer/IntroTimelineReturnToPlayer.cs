using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class IntroTimelineReturnToPlayer : MonoBehaviour
{
    [Header("Scene Intro ID")]
    [SerializeField] private string introId = "LouvreIntro";

    [Header("Timeline")]
    [SerializeField] private PlayableDirector director;

    [Header("Camera Roots")]
    [SerializeField] private GameObject animationCameraRoot;
    [SerializeField] private GameObject playerCameraRoot;

    [Header("Player Control Scripts")]
    [SerializeField] private MonoBehaviour[] playerControlScripts;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorAfterIntro = true;
    [SerializeField] private bool hideCursorAfterIntro = true;

    private void Awake()
    {
        if (director != null)
        {
            director.playOnAwake = false;
            director.stopped += OnTimelineStopped;
        }
    }

    private void Start()
    {
        bool shouldSkipIntro = false;

        if (GameStateManager.Instance != null)
        {
            // 只要播过一次，就跳过
            if (GameStateManager.Instance.HasPlayedIntro(introId))
                shouldSkipIntro = true;

            // 如果你想“从谜题回来时必跳过”，也可以额外加这句
            if (GameStateManager.Instance.GetFlag("ReturningFromPuzzle"))
                shouldSkipIntro = true;
        }

        if (shouldSkipIntro)
        {
            SwitchToPlayerImmediately();
            return;
        }

        PlayIntro();
    }

    private void PlayIntro()
    {
        if (animationCameraRoot != null)
            animationCameraRoot.SetActive(true);

        if (playerCameraRoot != null)
            playerCameraRoot.SetActive(false);

        SetPlayerControl(false);

        if (director != null)
        {
            director.time = 0;
            director.Evaluate();
            director.Play();
        }
    }

    private void OnTimelineStopped(PlayableDirector pd)
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.MarkIntroPlayed(introId);
        }

        SwitchToPlayerImmediately();
    }

    private void SwitchToPlayerImmediately()
    {
        if (director != null && director.state == PlayState.Playing)
            director.Stop();

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
            director.stopped -= OnTimelineStopped;
    }
}