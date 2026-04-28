using UnityEngine;

public class CirclePuzzle : MonoBehaviour
{
    // 完成时触发的回调
    public delegate void OnComplete();
    public OnComplete onComplete;

    // 各圆圈的 Transform
    public Transform[] circleList;

    // 各圆圈当前旋转步数（每步 18 度，共 20 步 = 360 度）
    [HideInInspector]
    public int[] circleRotation;

    // 音效
    private AudioSource audioSource;
    public AudioClip drag_sound;
    public AudioClip complete_sound;

    // 摄像机震动
    public bool shakeCamera = true;
    [HideInInspector]
    public Vector3 saveRotation;

    [HideInInspector]
    public bool gamePause = false;

    // 拖动音效与震动的计时器
    private float playSoundDragTime = 0f;
    private float shakeDragTime = 0f;

    // ─────────────────────────────────────────────

    void Start()
    {
        circleRotation = new int[circleList.Length];
        audioSource = GetComponent<AudioSource>();
        saveRotation = Camera.main.transform.localRotation.eulerAngles;
        Setup();
    }

    /// <summary>
    /// 随机初始化每个圆圈的旋转步数（3 ~ 19）
    /// </summary>
    public void Setup()
    {
        for (int i = 0; i < circleList.Length; i++)
        {
            circleRotation[i] = Random.Range(3, 20);
        }
    }

    // ─────────────────────────────────────────────

    void Update()
    {
        UpdateSoundTimer();
        UpdateCameraShake();
        UpdateCircleRotations();
    }

    /// <summary>
    /// 递减拖动音效冷却计时器
    /// </summary>
    private void UpdateSoundTimer()
    {
        if (playSoundDragTime > 0f)
            playSoundDragTime -= Time.deltaTime;
    }

    /// <summary>
    /// 处理摄像机震动，震动结束时检查胜利
    /// </summary>
    private void UpdateCameraShake()
    {
        if (shakeDragTime <= 0f) return;

        shakeDragTime -= Time.deltaTime;

        if (shakeDragTime <= 0f)
        {
            CheckWin();
            return;
        }

        if (shakeCamera)
        {
            Vector3 shake = new Vector3(
                Random.Range(-0.2f, 0.2f) * shakeDragTime + saveRotation.x,
                Random.Range(-0.2f, 0.2f) * shakeDragTime + saveRotation.y,
                Random.Range(-0.2f, 0.2f) * shakeDragTime + saveRotation.z
            );
            Camera.main.transform.rotation = Quaternion.Euler(shake);
        }
    }

    /// <summary>
    /// 平滑插值每个圆圈到目标旋转角度
    /// </summary>
    private void UpdateCircleRotations()
    {
        for (int i = 0; i < circleList.Length; i++)
        {
            Quaternion target = Quaternion.Euler(-90f, 0f, circleRotation[i] * 18f);
            circleList[i].localRotation = Quaternion.Lerp(
                circleList[i].localRotation,
                target,
                0.15f - (i * 0.025f)
            );
        }
    }

    // ─────────────────────────────────────────────

    /// <summary>
    /// 播放拖动音效，带冷却防止过于频繁触发
    /// </summary>
    public void PlayDragSound(int circleIndex)
    {
        if (playSoundDragTime > 0f) return;

        audioSource.pitch = 1.25f - (circleIndex * 0.15f);
        audioSource.PlayOneShot(drag_sound);

        playSoundDragTime = 0.08f + (circleIndex * 0.025f);
        shakeDragTime = (0.1f + (circleIndex * 0.015f)) * 5f;
    }

    /// <summary>
    /// 检查所有圆圈是否对齐到同一旋转格（模 20 相等即视为胜利）
    /// </summary>
    public void CheckWin()
    {
        int first = circleRotation[0] % 20;
        for (int i = 1; i < circleRotation.Length; i++)
        {
            if (circleRotation[i] % 20 != first) return;
        }

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(complete_sound);
        onComplete();
    }
}
