using UnityEngine;
using UnityEngine.UI; // If using standard UI
using TMPro;         // If using TextMeshPro

public class WinZone : MonoBehaviour
{
    [Header("UI & Audio")]
    public GameObject victoryPanel; // Drag your Win UI panel here
    public AudioSource winSound;    // Drag your "Click" sound source here

    private bool hasWon = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is the ball and we haven't won yet
        if (other.CompareTag("Player") && !hasWon)
        {
            hasWon = true;
            TriggerVictory();
        }
    }

    void TriggerVictory()
    {
        Debug.Log("Victory! Goal Reached.");

        // 1. Show the Victory UI
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // 2. Play the "Click" unlock sound
        if (winSound != null)
        {
            winSound.Play();
        }

        // 3. Optional: Stop the ball from moving
        Rigidbody rb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Freeze the ball
        }
    }
}