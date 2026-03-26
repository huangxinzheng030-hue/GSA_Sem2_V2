using UnityEngine;
using System.Collections;

// 自动添加 MeshCollider 或 BoxCollider，确保能被点到
[RequireComponent(typeof(Collider))] 
public class PuzzleRing : MonoBehaviour
{
    [Header("判定设置")]
    public float[] correctAngles = { 0f }; 
    public float tolerance = 10.0f;        

    [Header("旋转控制")]
    public float rotateStep = 30f; // 每次点击转多少度

    private Material dissolveMat;
    private bool isLocked = false;

    void Awake()
    {
        // 1. 获取材质
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // 使用 .material 会创建材质副本，保证圆环溶解互不干扰
            dissolveMat = rend.material; 
            dissolveMat.SetFloat("_DissolveAmount", 0);
        }

        // 2. 检查碰撞体：OnMouseDown 必须有 Collider 才能生效
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning(gameObject.name + " 缺少 Collider，已自动添加 BoxCollider");
            gameObject.AddComponent<BoxCollider>();
        }
    }

    // 判断是否对齐 (供 Manager 调用)
    public bool IsCorrect()
    {
        float currentZ = transform.localEulerAngles.z;
        foreach (float target in correctAngles)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(currentZ, target)) <= tolerance)
            {
                return true;
            }
        }
        return false;
    }

    // 锁定方法 (供 Manager 调用)
    public void LockRing()
    {
        isLocked = true;
        Debug.Log(gameObject.name + " 已锁定，无法再转动");
    }

    // 鼠标点击旋转
    void OnMouseDown()
    {
        if (!isLocked)
        {
            // 绕 Z 轴旋转
            transform.Rotate(0, 0, rotateStep);
            Debug.Log(gameObject.name + " 旋转至: " + transform.localEulerAngles.z);
        }
    }

    // 溶解协程 (供 Manager 调用)
    public IEnumerator DissolveRoutine(float duration)
    {
        if (dissolveMat == null) yield break;

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            dissolveMat.SetFloat("_DissolveAmount", elapsed / duration);
            yield return null;
        }
        gameObject.SetActive(false); 
    }
}