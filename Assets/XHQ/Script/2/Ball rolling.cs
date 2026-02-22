using UnityEngine;

public class BallRollingSound : MonoBehaviour
{
    public AudioSource rollingSource;
    public Rigidbody rb;
    
    public float maxSpeed = 10f; 
    public float minPitch = 0.5f;
    public float maxPitch = 1.5f;

    void Start()
    {
        if (rollingSource != null)
        {
            rollingSource.loop = true;
            rollingSource.Stop(); 
            rollingSource.volume = 0;
        }
    }

    void Update()
    {
        if (rollingSource == null || rb == null) return;

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float currentSpeed = horizontalVel.magnitude;

        if (currentSpeed > 0.2f && IsGrounded())
        {
            if (!rollingSource.isPlaying) 
            {
                rollingSource.Play();
            }

            float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
            rollingSource.volume = Mathf.Lerp(rollingSource.volume, speedRatio, Time.deltaTime * 5f);
            rollingSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);
        }
        else
        {
            rollingSource.volume = Mathf.MoveTowards(rollingSource.volume, 0, Time.deltaTime * 2f);
            
            if (rollingSource.volume <= 0 && rollingSource.isPlaying)
            {
                rollingSource.Stop();
            }
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y / 2) + 0.1f);
    }
}