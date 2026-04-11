using UnityEngine;
using System.Collections;
public class ObjectMoveOntrigger : MonoBehaviour
{
    [System.Serializable]
    public class ControlledObject
    {
        public Transform target;

        [Header("Gravity")]
        public bool useGravityControl = false;

        [Header("Move")]
        public bool moveControl = false;
        public float moveDistance = 2f;
        public float moveSpeed = 2f;

        [HideInInspector] public Vector3 startPos;
        [HideInInspector] public Vector3 targetPos;
        [HideInInspector] public Rigidbody rb;
    }

    [Header("Controlled Objects")]
    public ControlledObject[] objects;

    [Header("Trigger Settings")]
    public float delay = 0f;
    public bool triggerOnce = true;

    private bool triggered = false;

    void Start()
    {
        foreach (var obj in objects)
        {
            if (obj.target == null) continue;

            obj.startPos = obj.target.position;
            obj.targetPos = obj.startPos + Vector3.up * obj.moveDistance;

            obj.rb = obj.target.GetComponent<Rigidbody>();

            if (obj.useGravityControl && obj.rb != null)
            {
                obj.rb.useGravity = false;
                obj.rb.isKinematic = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered && triggerOnce) return;
        Debug.Log("触发到了: " + other.name);

        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(TriggerAction());
            Debug.Log("玩家进入 Trigger");
        }
    }

    IEnumerator TriggerAction()
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        foreach (var obj in objects)
        {
            if (obj.target == null) continue;

            // 开启重力
            if (obj.useGravityControl && obj.rb != null)
            {
                obj.rb.isKinematic = false;
                obj.rb.useGravity = true;
            }

            // 移动
            if (obj.moveControl)
            {
                StartCoroutine(MoveObject(obj));
            }
        }
    }

    IEnumerator MoveObject(ControlledObject obj)
    {
        while (Vector3.Distance(obj.target.position, obj.targetPos) > 0.01f)
        {
            obj.target.position = Vector3.MoveTowards(
                obj.target.position,
                obj.targetPos,
                obj.moveSpeed * Time.deltaTime
            );
            yield return null;
        }
    }
}
