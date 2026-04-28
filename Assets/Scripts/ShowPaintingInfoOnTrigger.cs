using UnityEngine;

public class ShowPaintingInfoOnTrigger : MonoBehaviour
{
    [Header("要显示/隐藏的3D简介面板")]
    public GameObject infoCanvas;

    [Header("玩家Tag")]
    public string playerTag = "Player";

    private void Start()
    {
        if (infoCanvas != null)
        {
            infoCanvas.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            infoCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            infoCanvas.SetActive(false);
        }
    }
}