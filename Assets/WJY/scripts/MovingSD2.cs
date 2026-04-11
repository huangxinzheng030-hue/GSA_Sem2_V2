using UnityEngine;

public class MovingSD2 : MonoBehaviour
{
    public enum MoveAxis { X, Y, Z }

    [Header("Axis & Range")]
    public MoveAxis axis = MoveAxis.X;
    public bool startInNegativeDirection = false;
    public float moveDistance = 5f;
    public bool useLocalStartAsCenter = true;

    [Header("Speed (Random Accelerating)")]
    public float minSpeed = 1f;
    public float maxSpeed = 6f;
    public float acceleration = 2f;
    public float changeInterval = 2f;

    [Header("Pause At Ends")]
    public float minPauseTime = 0.0f;
    public float maxPauseTime = 0.0f;
    public bool pauseBothEnds = true;

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public bool drawOnlyWhenSelected = true;
    public float gizmoSphereRadius = 0.12f;

    [Tooltip("当 useLocalStartAsCenter=false 时使用")]
    public Transform startPoint;

    private Vector3 _centerPos;
    private int _direction;
    private Vector3 _axisDir;

    private float _currentSpeed = 0f;
    private float _targetSpeed = 0f;
    private float _speedTimer = 0f;

    private bool _isPaused = false;
    private float _pauseTimer = 0f;

    void Start()
    {
        _centerPos = (useLocalStartAsCenter || startPoint == null) ? transform.position : startPoint.position;
        _axisDir = GetAxisDirection(axis);
        _direction = startInNegativeDirection ? -1 : 1;

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

        _speedTimer += Time.deltaTime;
        if (_speedTimer >= changeInterval)
        {
            _targetSpeed = Random.Range(minSpeed, maxSpeed);
            _speedTimer = 0f;
        }

        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, acceleration * Time.deltaTime);

        Vector3 delta = _axisDir * (_direction * _currentSpeed * Time.deltaTime);
        Vector3 nextPos = transform.position + delta;

        float nextOffset = Vector3.Dot(nextPos - _centerPos, _axisDir);

        if (Mathf.Abs(nextOffset) >= moveDistance)
        {
            float clampedOffset = Mathf.Sign(nextOffset) * moveDistance;
            nextPos = _centerPos + _axisDir * clampedOffset;

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
        Vector3 center = Application.isPlaying
            ? _centerPos
            : (useLocalStartAsCenter || startPoint == null ? transform.position : startPoint.position);

        Vector3 axisDir = GetAxisDirection(axis);
        Vector3 a = center - axisDir * moveDistance;
        Vector3 b = center + axisDir * moveDistance;

        Gizmos.DrawLine(a, b);
        Gizmos.DrawSphere(a, gizmoSphereRadius);
        Gizmos.DrawSphere(b, gizmoSphereRadius);
    }
}