using UnityEngine;

public class MoveUp : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveDistance = 2f;   // 向上移动的距离
    public float moveSpeed = 2f;      // 移动速度

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isMoving = false;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.up * moveDistance;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isMoving = true;
        }
    }
}
