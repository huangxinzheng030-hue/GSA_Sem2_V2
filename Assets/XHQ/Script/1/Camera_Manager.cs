using UnityEngine;
using Unity.Cinemachine; 
using System.Collections;

public class CameraAutoZoom : MonoBehaviour
{
    public CinemachineCamera farCam; 
    public CinemachineCamera closeCam;
    public float waitBeforeZoom = 1.0f; 

    void Start()
    {
        StartCoroutine(ExecuteZoom());
    }

    IEnumerator ExecuteZoom()
    {
        farCam.Priority = 20;
        closeCam.Priority = 10;

        yield return new WaitForSeconds(waitBeforeZoom);

        farCam.Priority = 10;
        closeCam.Priority = 20;
    }
}