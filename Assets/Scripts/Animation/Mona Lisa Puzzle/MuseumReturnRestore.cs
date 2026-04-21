using System.Collections;
using UnityEngine;

public class MuseumReturnRestore : MonoBehaviour
{
    public Transform playerRoot;
    public Transform returnPoint;

    [Header("Debug / Force Teleport")]
    public float delayBeforeTeleport = 0.15f;
    public int forceTeleportFrames = 5;

    private IEnumerator Start()
    {
        Debug.Log("MuseumReturnRestore Start");

        if (!PuzzleProgress.shouldReturnToMuseumPoint)
        {
            Debug.Log("MuseumReturnRestore: shouldReturnToMuseumPoint = false");
            yield break;
        }

        if (playerRoot == null)
        {
            Debug.LogWarning("MuseumReturnRestore: playerRoot is NULL");
            yield break;
        }

        if (returnPoint == null)
        {
            Debug.LogWarning("MuseumReturnRestore: returnPoint is NULL");
            yield break;
        }

        // 等待一会，避开其他出生点/控制器初始化
        yield return new WaitForSeconds(delayBeforeTeleport);

        CharacterController cc = playerRoot.GetComponent<CharacterController>();
        if (cc == null)
            cc = playerRoot.GetComponentInChildren<CharacterController>();

        Rigidbody[] rbs = playerRoot.GetComponentsInChildren<Rigidbody>(true);

        if (cc != null)
            cc.enabled = false;

        Debug.Log("MuseumReturnRestore: target pos = " + returnPoint.position);

        // 连续强制几帧，防止被别的脚本改回默认位置
        for (int i = 0; i < forceTeleportFrames; i++)
        {
            playerRoot.position = returnPoint.position;
            playerRoot.rotation = returnPoint.rotation;

            foreach (Rigidbody rb in rbs)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            yield return null;
        }

        if (cc != null)
            cc.enabled = true;

        Debug.Log("MuseumReturnRestore: final pos = " + playerRoot.position);

        PuzzleProgress.shouldReturnToMuseumPoint = false;
    }
}