using UnityEngine;
using System.Collections;

public class MazeFinalController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject mazeObject;   
    public GameObject painting;     
    public AudioSource unlockSound; 

    [Header("Settings")]
    public float pauseDelay = 1.5f;   
    public float slideSpeed = 10f;   
    public float fallDistance = 25f;  

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(ExecuteMazeSequence(other.gameObject));
        }
    }

    IEnumerator ExecuteMazeSequence(GameObject ball)
    {
        if (unlockSound != null) unlockSound.Play();

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.isKinematic = true;
            ballRb.linearVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(pauseDelay);

        if (painting != null) painting.SetActive(true);

        Rigidbody mazeRb = mazeObject.GetComponent<Rigidbody>();
        if (mazeRb != null)
        {
            mazeRb.isKinematic = true; 
            mazeRb.detectCollisions = false; 
        }

        Animator mazeAnim = mazeObject.GetComponent<Animator>();
        if (mazeAnim == null) mazeAnim = mazeObject.GetComponentInChildren<Animator>();
        if (mazeAnim != null) mazeAnim.enabled = false;

        ball.transform.SetParent(mazeObject.transform);

        Vector3 startPos = mazeObject.transform.position;
        float traveled = 0f;

        while (traveled < fallDistance)
        {
            float step = slideSpeed * Time.deltaTime;
            traveled += step;

            mazeObject.transform.position = new Vector3(
                startPos.x,
                startPos.y,
                startPos.z + traveled
            );

            yield return null; 
        }

        Destroy(mazeObject);
        Debug.Log("Maze Sequence Completed.");
    }
}