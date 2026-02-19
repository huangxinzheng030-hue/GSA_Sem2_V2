using UnityEngine;

public class BallMovementArrows : MonoBehaviour
{
    public float moveSpeed = 20f;
    public bool invertHorizontal = false;
    public bool invertVertical = true; 

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = 2f; 
        rb.angularDamping = 2f;
    }

    void FixedUpdate()
    {
        if (rb != null) MoveBall();
    }

    void MoveBall()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h == 0 && v == 0) return;
        if (invertHorizontal) h *= -1f;
        if (invertVertical) v *= -1f;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        float camYRotation = mainCam.transform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, camYRotation, 0);

        Vector3 inputDir = new Vector3(h, 0, v);
        Vector3 moveDir = rotation * inputDir;

        rb.AddForce(moveDir.normalized * moveSpeed, ForceMode.Force);
    }
}