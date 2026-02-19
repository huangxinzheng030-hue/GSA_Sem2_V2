using UnityEngine;

public class BallMovementArrows : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Set high drag to prevent the ball from rolling uncontrollably
        rb.linearDamping = 2f;
        rb.angularDamping = 2f;
    }

    void FixedUpdate()
    {
        MoveBall();
    }

    void MoveBall()
    {
        // Using "Raw" axis for snappy, instant response
        float moveHorizontal = Input.GetAxisRaw("Horizontal"); // Left/Right Arrows
        float moveVertical = Input.GetAxisRaw("Vertical");     // Up/Down Arrows

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        if (movement != Vector3.zero)
        {
            // Apply force normalized to ensure consistent speed diagonally
            rb.AddForce(movement.normalized * moveSpeed);
        }
    }
}