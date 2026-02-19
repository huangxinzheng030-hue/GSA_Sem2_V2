using UnityEngine;

[RequireComponent(typeof(AudioSource))] // 自动添加 AudioSource 组件
public class PuzzleRing : MonoBehaviour
{
    [Header("设置")]
    public float correctAngle = 0f;    // 正确的角度（通常是0）
    public float rotateSpeed = 200f;   // 旋转动画速度
    public float tolerance = 5f;       // 判定容错角度（建议 3-5 度）

    [Header("音效")]
    public AudioClip clickSound;       // 拖入“咔哒”音效
    
    // 内部变量
    private float targetAngle;
    private bool isRotating = false;
    private bool isLocked = false;     // 胜利后锁定，不可再转
    private AudioSource audioSource;

    void Start()
    {
        // 初始化当前角度
        targetAngle = transform.eulerAngles.z;
        audioSource = GetComponent<AudioSource>();
        
        // 初始时不播放声音
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        // 平滑旋转处理
        if (transform.eulerAngles.z != targetAngle)
        {
            Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            
            // 检查是否旋转到位
            if (Quaternion.Angle(transform.rotation, targetRot) < 0.1f)
            {
                transform.rotation = targetRot; // 强制归位
                isRotating = false;
            }
        }
    }

    void OnMouseDown()
    {
        // 如果正在旋转 或者 游戏已经结束，就不允许点击
        if (isRotating || isLocked) return;

        RotateRing();
    }

    void RotateRing()
    {
        isRotating = true;
        targetAngle += 90f; // 每次点击转 90 度
        
        // 播放音效（带一点随机音调，听起来更自然）
        if (clickSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clickSound);
        }
    }

    // 提供给管理器调用的判定方法
    public bool IsCorrect()
    {
        float currentZ = transform.eulerAngles.z;
        // 使用 DeltaAngle 处理 0 和 360 的问题
        float difference = Mathf.DeltaAngle(currentZ, correctAngle);
        return Mathf.Abs(difference) < tolerance;
    }

    // 胜利后锁定圆环
    public void LockRing()
    {
        isLocked = true;
    }
}