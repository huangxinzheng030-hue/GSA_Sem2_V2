using UnityEngine;

public class PointerStarDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Åöµ½£º" + other.name);

        if (other.CompareTag("Star"))
        {
            StarLight star = other.GetComponentInParent<StarLight>();

            if (star != null)
            {
                star.TurnOn();
            }
        }
    }
}