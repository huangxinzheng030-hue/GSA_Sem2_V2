using UnityEngine;

public class PaintingTriggerShow : MonoBehaviour
{
    public GameObject targetPanel;

    private void Start()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetPanel != null)
            {
                targetPanel.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetPanel != null)
            {
                targetPanel.SetActive(false);
            }
        }
    }
}