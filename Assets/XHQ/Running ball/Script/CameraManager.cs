using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera farCam;    // 远景
    public CinemachineCamera closeCam;  // 近景

    void Start()
    {
        // 核心修改：游戏一开始就自动拉近
        ZoomIn(); 
    }

    public void ZoomIn()
    {
        closeCam.Priority = 20;
        farCam.Priority = 10;
    }

    public void ZoomOut()
    {
        farCam.Priority = 20;
        closeCam.Priority = 10;
    }
}