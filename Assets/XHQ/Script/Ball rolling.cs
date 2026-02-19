using UnityEngine;

public class BallRollingSound : MonoBehaviour
{
    public AudioSource rollingSource;
    public Rigidbody rb;
    
    public float maxSpeed = 10f; // Speed at which pitch/volume is maximum
    public float minPitch = 0.5f;
    public float maxPitch = 1.5f;

    void Start()
    {
        if (rollingSource != null)
        {
            rollingSource.loop = true;
            rollingSource.Play();
        }
    }

    void Update()
    {
        // Calculate current speed
        float currentSpeed = rb.linearVelocity.magnitude;

        if (currentSpeed > 0.1f && IsGrounded())
        {
            if (!rollingSource.isPlaying) rollingSource.Play();

            // Adjust volume based on speed (faster = louder)
            rollingSource.volume = Mathf.Lerp(0, 1, currentSpeed / maxSpeed);

            // Adjust pitch based on speed (faster = higher pitch)
            rollingSource.pitch = Mathf.Lerp(minPitch, maxPitch, currentSpeed / maxSpeed);
        }
        else
        {
            // Fade out or stop when ball is still or in the air
            rollingSource.volume = Mathf.Lerp(rollingSource.volume, 0, Time.deltaTime * 5f);
        }
    }

    // Simple check to ensure ball is touching the ground
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, transform.localScale.y / 2 + 0.1f);
    }
}