using UnityEngine;

public class MovingSD2 : MonoBehaviour
{
   public enum MoveAxis { X, Y, Z }

    [Header("Axis & Range")]
    public MoveAxis axis = MoveAxis.X;
    public float moveDistance = 5f;              // 单侧最大偏移（从初始位置到端点）
    public bool useLocalStartAsCenter = true;    // true: 以初始位置为中心来回；false: 以 startPoint 为中心

    [Header("Speed (Random Accelerating)")]
    public float minSpeed = 1f;
    public float maxSpeed = 6f;
    public float acceleration = 2f;             // 速度追向目标速度的“加速度”（越大变化越快）
    public float changeInterval = 2f;           // 每隔多久随机一个新的目标速度（秒）

    [Header("Pause At Ends")]
    public float minPauseTime = 0.0f;           // 到端点最短停顿
    public float maxPauseTime = 0.0f;           // 到端点最长停顿（>0 才会停）
    public bool pauseBothEnds = true;           // true: 两端都停；false: 只在正向端点停

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public bool drawOnlyWhenSelected = true;
    public float gizmoSphereRadius = 0.12f;

    [Tooltip("当 useLocalStartAsCenter=false 时使用")]
    public Transform startPoint;

    private Vector3 _centerPos;
    private int _direction = 1;

    private float _currentSpeed = 0f;
    private float _targetSpeed = 0f;
    private float _speedTimer = 0f;

    private bool _isPaused = false;
    private float _pauseTimer = 0f;

    void Start()
    {
        _centerPos = (useLocalStartAsCenter || startPoint == null) ? transform.position : startPoint.position;
        _targetSpeed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        if (_isPaused)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0f) _isPaused = false;
            return;
        }

        // 定时更新目标速度
        _speedTimer += Time.deltaTime;
        if (_speedTimer >= changeInterval)
        {
            _targetSpeed = Random.Range(minSpeed, maxSpeed);
            _speedTimer = 0f;
        }

        // 平滑追向目标速度（随机加减速）
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, acceleration * Time.deltaTime);

        // 世界坐标移动方向
        Vector3 axisDir = GetAxisDirection(axis);
        Vector3 delta = axisDir * (_direction * _currentSpeed * Time.deltaTime);
        Vector3 nextPos = transform.position + delta;

        // 计算下一步偏移（相对中心）
        float nextOffset = GetAxisValue(nextPos - _centerPos, axis);

        // 到端点：夹紧 + 反向 + 可能停顿
        if (Mathf.Abs(nextOffset) >= moveDistance)
        {
            float clampedOffset = Mathf.Sign(nextOffset) * moveDistance;
            nextPos = _centerPos + axisDir * clampedOffset;

            bool isPositiveEnd = clampedOffset > 0f;
            bool shouldPause = (maxPauseTime > 0f) && (pauseBothEnds || isPositiveEnd);

            transform.position = nextPos;
            _direction *= -1;

            if (shouldPause)
            {
                _isPaused = true;
                _pauseTimer = Random.Range(minPauseTime, Mathf.Max(minPauseTime, maxPauseTime));
            }

            return;
        }

        transform.position = nextPos;
    }

    private static Vector3 GetAxisDirection(MoveAxis a)
    {
        return a switch
        {
            MoveAxis.X => Vector3.right,
            MoveAxis.Y => Vector3.up,
            _ => Vector3.forward
        };
    }

    private static float GetAxisValue(Vector3 v, MoveAxis a)
    {
        return a switch
        {
            MoveAxis.X => v.x,
            MoveAxis.Y => v.y,
            _ => v.z
        };
    }

    // —— Gizmos 可视化（轨道 + 端点）——
    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (drawOnlyWhenSelected) return;
        DrawGizmosInternal();
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        if (!drawOnlyWhenSelected) return;
        DrawGizmosInternal();
    }

    private void DrawGizmosInternal()
    {
        Vector3 center = _centerPos;
        if (!Application.isPlaying)
        {
            // 编辑模式下用当前设置推一个中心点
            center = (useLocalStartAsCenter || startPoint == null) ? transform.position : startPoint.position;
        }

        Vector3 axisDir = GetAxisDirection(axis);
        Vector3 a = center - axisDir * moveDistance;
        Vector3 b = center + axisDir * moveDistance;

        Gizmos.DrawLine(a, b);
        Gizmos.DrawSphere(a, gizmoSphereRadius);
        Gizmos.DrawSphere(b, gizmoSphereRadius);
    }

}
