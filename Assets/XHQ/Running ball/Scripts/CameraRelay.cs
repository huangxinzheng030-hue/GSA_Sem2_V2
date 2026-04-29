using UnityEngine;

public class CameraRelay : MonoBehaviour
{
    public CameraManager camManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (camManager != null) camManager.ZoomOut();
            Debug.Log("<color=green>Success!</color> Camera Updated.");
        }
    }
}