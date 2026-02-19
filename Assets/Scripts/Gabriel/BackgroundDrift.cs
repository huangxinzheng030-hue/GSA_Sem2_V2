using UnityEngine;

public class BackgroundDrift : MonoBehaviour
{
    [Header("Position Drift")]
    public Vector3 moveAxis = new Vector3(1, 0, 0); // 移动方向（世界/本地都行）
    public float moveAmplitude = 0.3f;              // 移动幅度（单位：米）
    public float moveSpeed = 0.2f;                  // 移动速度（越小越慢）

    [Header("Float (optional)")]
    public bool enableFloat = true;
    public float floatAmplitude = 0.08f;
    public float floatSpeed = 0.35f;

    [Header("Rotation (optional)")]
    public bool enableRotate = true;
    public Vector3 rotateAxis = new Vector3(0, 1, 0);
    public float rotateSpeedDeg = 2f;               // 每秒旋转角度（很小）

    [Header("Randomization")]
    public bool randomizePhase = true;
    public bool randomizeStartOffset = true;

    Vector3 startLocalPos;
    float phase;

    void Awake()
    {
        startLocalPos = transform.localPosition;

        if (randomizePhase)
            phase = Random.Range(0f, 1000f);

        if (randomizeStartOffset)
        {
            // 小幅随机初始位移，避免所有物体同步摆动
            startLocalPos += new Vector3(
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f)
            );
        }
    }

    void Update()
    {
        float t = Time.time + phase;

        // 来回平移（正弦）
        Vector3 drift = moveAxis.normalized * Mathf.Sin(t * moveSpeed) * moveAmplitude;

        // 上下漂浮（正弦）
        float y = 0f;
        if (enableFloat)
            y = Mathf.Sin(t * floatSpeed) * floatAmplitude;

        transform.localPosition = startLocalPos + drift + new Vector3(0f, y, 0f);

        // 缓慢自转
        if (enableRotate)
        {
            transform.Rotate(rotateAxis.normalized, rotateSpeedDeg * Time.deltaTime, Space.Self);
        }
    }
}
