using UnityEngine;

public class CameraRelay : MonoBehaviour
{
    public CameraManager camManager;

    private void OnTriggerEnter(Collider other)
    {
        // 这里的 Tag 必须和 Drop 脚本里判断的一样（Player）
        if (other.CompareTag("Player"))
        {
            if (camManager != null)
            {
                camManager.ZoomOut();
                Debug.Log("球到达终点：自动拉远。");
            }
        }
    }
}