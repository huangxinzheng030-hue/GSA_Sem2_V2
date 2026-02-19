using UnityEngine;

public class TrackingTrap : MonoBehaviour
{
    [Header("Target")]
    public string playerTag = "Player";          // 玩家Tag
    public Transform target;                     // 可手动拖玩家；不拖就按Tag自动找

    [Header("Activation")]
    public bool onlyMoveWhenPlayerInside = true; // 玩家离开后是否停止
    public float startDelay = 0f;                // 进入后延迟启动（可选）

    [Header("Follow & Range")]
    public float minRadius = 1.5f;               // 与玩家最小距离
    public float maxRadius = 4.0f;               // 与玩家最大距离
    public float followSpeed = 6.0f;             // 跟随“目标点”的速度（越大越贴身）
    public float verticalOffset = 0f;            // 陷阱相对玩家的Y偏移（比如悬浮）

    [Header("Random Motion (Smooth)")]
    public float orbitSpeed = 1.2f;              // 围绕转动速度
    public float noiseStrength = 1.0f;           // 随机扰动强度
    public float noiseFrequency = 0.6f;          // 随机变化频率（越大变化越快）
    public float radiusLerpSpeed = 1.5f;         // 半径变化平滑速度

    [Header("Optional: Keep In Trigger Area")]
    public bool clampToTriggerBounds = false;    // 是否限制在触发器bounds内
    public Collider triggerArea;                 // 可拖入某个Trigger Collider作为范围（不拖则用本物体上的Trigger）

    private bool _active = false;
    private bool _playerInside = false;
    private float _startTimer = 0f;

    private float _angle = 0f;
    private float _radius = 0f;
    private float _radiusTarget = 0f;

    private float _noiseSeedA;
    private float _noiseSeedB;

    void Awake()
    {
        _noiseSeedA = Random.Range(0f, 1000f);
        _noiseSeedB = Random.Range(0f, 1000f);

        if (triggerArea == null)
            triggerArea = GetComponent<Collider>(); // 你可以把Trigger放在同一个物体上
    }

    void Start()
    {
        if (target == null && !string.IsNullOrEmpty(playerTag))
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) target = p.transform;
        }

        _radius = Mathf.Clamp((minRadius + maxRadius) * 0.5f, minRadius, maxRadius);
        _radiusTarget = _radius;
    }

    void Update()
    {
        if (target == null) return;

        if (!_active)
        {
            // 是否需要玩家在区域里才激活
            if (!onlyMoveWhenPlayerInside || _playerInside)
            {
                _startTimer += Time.deltaTime;
                if (_startTimer >= startDelay) _active = true;
            }
            else
            {
                _startTimer = 0f;
            }
        }

        if (!_active) return;

        // 如果要求“只在区域内移动”，玩家离开就停
        if (onlyMoveWhenPlayerInside && !_playerInside)
        {
            _active = false;
            _startTimer = 0f;
            return;
        }

        // 1) 平滑随机：Perlin 噪声生成扰动（-1..1）
        float t = Time.time * noiseFrequency;
        float nx = Mathf.PerlinNoise(_noiseSeedA, t) * 2f - 1f;
        float nz = Mathf.PerlinNoise(_noiseSeedB, t) * 2f - 1f;

        // 2) 半径也随机轻微变化，并平滑追随
        _radiusTarget = Mathf.Lerp(minRadius, maxRadius, Mathf.PerlinNoise(_noiseSeedA + 10f, t));
        _radius = Mathf.MoveTowards(_radius, _radiusTarget, radiusLerpSpeed * Time.deltaTime);

        // 3) 基础环绕角速度 + 随机扰动
        _angle += (orbitSpeed + nx * 0.3f) * Time.deltaTime;

        // 4) 计算围绕玩家的“目标点”（XZ平面环绕 + 噪声偏移）
        Vector3 baseOffset = new Vector3(Mathf.Cos(_angle), 0f, Mathf.Sin(_angle)) * _radius;
        Vector3 noiseOffset = new Vector3(nx, 0f, nz) * noiseStrength;

        Vector3 desired = target.position + new Vector3(0f, verticalOffset, 0f) + baseOffset + noiseOffset;

        // 5) 可选：限制在触发器bounds里（做“固定范围内移动”很实用）
        if (clampToTriggerBounds && triggerArea != null)
        {
            Bounds b = triggerArea.bounds;
            desired.x = Mathf.Clamp(desired.x, b.min.x, b.max.x);
            desired.y = Mathf.Clamp(desired.y, b.min.y, b.max.y);
            desired.z = Mathf.Clamp(desired.z, b.min.z, b.max.z);
        }

        // 6) 平滑移动到目标点（追随但随机）
        transform.position = Vector3.MoveTowards(transform.position, desired, followSpeed * Time.deltaTime);
    }

    // —— 触发区域：玩家进入开始、离开停止 ——
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        _playerInside = true;
        if (!onlyMoveWhenPlayerInside) return;

        // 进入就准备启动（实际启动受startDelay控制）
        _startTimer = 0f;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        _playerInside = false;
        if (onlyMoveWhenPlayerInside)
        {
            // 立即停止（如果你想“离开后缓慢停下”，我也可以给版本）
            _active = false;
            _startTimer = 0f;
        }
    }
}
