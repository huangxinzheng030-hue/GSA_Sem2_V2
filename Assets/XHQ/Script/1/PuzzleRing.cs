using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))] // 确保物体上自动挂载 AudioSource 组件
public class PuzzleRing : MonoBehaviour
{
    [Header("旋转设置")]
    public float correctAngle = 0f;    // 胜利时的正确角度（通常蒙娜丽莎摆正是 0）
    public float rotateSpeed = 300f;   // 旋转动画的速度（数值越大转得越快）
    public float tolerance = 5f;       // 判定容错角度（正负5度内都算对齐）

    [Header("音效设置")]
    public AudioClip clickSound;       // 拖入“咔哒”音效
    
    // 内部运行状态变量
    private float targetAngle;         // 目标角度
    private bool isRotating = false;   // 是否正在执行旋转动画
    private bool isLocked = false;     // 胜利后是否已被锁死
    private AudioSource audioSource;   // 声音播放器

    void Start()
    {
        // 初始化音频组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false; // 强制关闭开局自动播放
        }

        // 记录初始的本地 Z 轴角度
        targetAngle = transform.localEulerAngles.z;
    }

    void Update()
    {
        // 如果触发了旋转操作，就在每一帧平滑过渡到目标角度
        if (isRotating)
        {
            float currentZ = transform.localEulerAngles.z;
            
            // 【核心修复】使用 MoveTowardsAngle 只计算 Z 轴的纯数字变化，绝对不碰 X 和 Y
            float nextZ = Mathf.MoveTowardsAngle(currentZ, targetAngle, rotateSpeed * Time.deltaTime);
            
            // 暴力重置：强制 X 和 Y 永远为 0
            transform.localRotation = Quaternion.Euler(0, 0, nextZ);
            
            // 检查是否已经转到了目标角度（误差小于 0.1 度即视为到达）
            if (Mathf.Abs(Mathf.DeltaAngle(currentZ, targetAngle)) < 0.1f)
            {
                // 强制对齐到绝对整数角度，并停止动画
                transform.localRotation = Quaternion.Euler(0, 0, targetAngle);
                isRotating = false;
            }
        }
    }

    void OnMouseDown()
    {
        // 如果正在旋转中，或者游戏已经胜利（被锁死），则不允许再次点击
        if (isRotating || isLocked) return;

        RotateRing();
    }

    // 执行旋转指令
    void RotateRing()
    {
        isRotating = true;
        targetAngle += 90f; // 每次顺时针旋转 90 度（如果要逆时针改成 -90f）
        
        // 播放点击音效
        if (clickSound != null && audioSource != null)
        {
            // 每次点击让音调产生微小的随机变化，听起来更生动真实
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clickSound);
        }
    }

    // 【供 PuzzleManager 调用】检查当前环是否对齐
    public bool IsCorrect()
    {
        float currentZ = transform.localEulerAngles.z;
        // DeltaAngle 会自动处理 360 度和 0 度是同一个位置的问题
        float difference = Mathf.DeltaAngle(currentZ, correctAngle);
        
        // 只要误差在容错范围内，就返回 true (对齐了)
        return Mathf.Abs(difference) < tolerance;
    }

    // 【供 PuzzleManager 调用】游戏胜利后锁定该环
    public void LockRing()
    {
        isLocked = true;
    }

    // 【供 PuzzleManager 调用】游戏开始时随机打乱角度
    public void RandomizeRotation()
    {
        // 准备三个错误的初始角度，故意避开 0 度
        float[] wrongAngles = { 90f, 180f, 270f };
        float randomZ = wrongAngles[Random.Range(0, wrongAngles.Length)];
        
        // 直接瞬间改变角度，不播放动画
        transform.localRotation = Quaternion.Euler(0, 0, randomZ);
        
        // 同步更新目标角度，防止它自己又转回去
        targetAngle = randomZ; 
        
        isRotating = false;
        isLocked = false;
    }

    internal string DissolveRoutine(float dissolveTime)
    {
        throw new System.NotImplementedException();
    }
    // 注意：文件顶部必须有 using System.Collections; 才能使用 IEnumerator

    // 提供给管理器调用的溶解/渐隐动画协程
    public IEnumerator Dissolve(float duration)
    {
        // 获取圆环上的 SpriteRenderer 组件
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 如果没有 SpriteRenderer，直接退出协程防报错
        if (spriteRenderer == null) yield break;

        // 获取初始颜色
        Color startColor = spriteRenderer.color;
        // 设置目标颜色（RGB不变，Alpha透明度变为0）
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float elapsedTime = 0f;

        // 在设定的时间（duration）内，平滑过渡透明度
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp 可以在两个值之间进行平滑插值
            spriteRenderer.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
            
            // 等待下一帧再继续循环
            yield return null; 
        }

        // 循环结束后，确保透明度彻底变成0
        spriteRenderer.color = targetColor;
        
        // 可选：溶解完成后隐藏物体，节省性能
        // gameObject.SetActive(false);
    }
}