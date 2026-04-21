using UnityEngine;

public class CameraRelay : MonoBehaviour
{
    public CameraManager camManager;
    public UIManager uiManager;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.name + " with Tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            if (camManager != null) camManager.ZoomOut();
            
            if (uiManager != null) uiManager.ShowWin();
            
            Debug.Log("<color=green>Success!</color> Camera and UI Updated.");
        }
    }
}