using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class LoadingVideoController : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;

    [Header("Next Scene")]
    public string nextSceneName = "S2";

    [Header("Options")]
    public bool allowSkip = false;
    public KeyCode skipKey = KeyCode.Space;

    bool hasLoaded = false;

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            Debug.LogError("LoadingVideoController: No VideoPlayer found.");
            return;
        }

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    void Update()
    {
        if (!allowSkip || hasLoaded) return;

        if (Input.GetKeyDown(skipKey))
        {
            LoadNextScene();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (hasLoaded) return;
        hasLoaded = true;

        SceneManager.LoadScene(nextSceneName);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}