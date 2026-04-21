using UnityEngine;

public class TrackingAreaRelay : MonoBehaviour
{
    public TrackingTrap trackingTrap;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        trackingTrap.SetPlayerInside(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        trackingTrap.SetPlayerInside(false);
    }
}