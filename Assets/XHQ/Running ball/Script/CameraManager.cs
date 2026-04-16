using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera farCam; 
    public CinemachineCamera closeCam;

    void Start()
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