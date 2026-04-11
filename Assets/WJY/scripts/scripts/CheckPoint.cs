using UnityEngine;

[DisallowMultipleComponent]
public class CheckPoint : MonoBehaviour
{
    [Header("Trigger")]
    public string playerTag = "Player";

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public bool useThisTransformIfPointIsNull = true;
    public bool updateRotationToo = true;

    [Header("Options")]
    public bool triggerOnlyOnce = false;

    private bool hasTriggered = false;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("碰到东西了: " + other.name);

        if (!other.CompareTag(playerTag))
        {
            Debug.Log("不是玩家，tag = " + other.tag);
            return;
        }

        if (triggerOnlyOnce && hasTriggered) return;
        if (GameController2.Instance == null)
        {
            Debug.LogWarning("GameController.Instance is null");
            return;
        }

        Transform pointToUse = respawnPoint != null ? respawnPoint : transform;

        if (updateRotationToo)
        {
            GameController2.Instance.SetRespawnPoint(pointToUse.position, pointToUse.eulerAngles);
        }
        else
        {
            GameController2.Instance.SetRespawnPoint(pointToUse.position);
        }

        Debug.Log("检查点已更新到: " + pointToUse.position);

        hasTriggered = true;
    }
}
