using UnityEngine;

public class ObejctFall : MonoBehaviour
{
    [Header("Delay before falling")]
    public float delay = 0f;   // 掉落延迟时间（0=立刻）

    private Rigidbody rb;
    private bool triggered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 初始状态不受重力
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (triggered) return;

    if (collision.gameObject.CompareTag("Player"))
    {
        if (collision.contacts[0].normal.y > 0.5f) // 从上方踩
        {
            triggered = true;
            Invoke("EnableGravity", delay);
        }
    }
    }

    void EnableGravity()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}
