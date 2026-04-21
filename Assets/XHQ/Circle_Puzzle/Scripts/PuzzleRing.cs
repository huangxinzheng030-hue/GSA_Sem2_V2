using UnityEngine;
using System.Collections;

public class PuzzleRing : MonoBehaviour
{
    [Header("拼图设置")]
    public float stepAngle = 30f;
    public float[] correctAngles = new float[] { 0f };  
    public float tolerance = 10f;

    [Header("动画")]
    public float rotateSpeed = 300f;

    private float targetAngle;
    private bool isRotating = false;
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend) originalColor = rend.material.color;
    }

    void Update()
    {
        if (isRotating)
        {
            float current = transform.localEulerAngles.y;
            float next = Mathf.MoveTowardsAngle(current, targetAngle, rotateSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0, next, 0);

            if (Mathf.Abs(Mathf.DeltaAngle(next, targetAngle)) < 0.1f)
            {
                transform.localEulerAngles = new Vector3(0, targetAngle, 0);
                isRotating = false;
                PuzzleManager.Instance.CheckSolved();
            }
        }
    }

    public void RotateLeft()
    {
        if (isRotating) return;
        targetAngle -= stepAngle;
        isRotating = true;
        StartCoroutine(Flash());
    }

    public void RotateRight()
    {
        if (isRotating) return;
        targetAngle += stepAngle;
        isRotating = true;
        StartCoroutine(Flash());
    }

    public bool IsSolved()
    {
        float current = transform.localEulerAngles.y;
        foreach (float angle in correctAngles)
        {
            float correct = ((angle % 360f) + 360f) % 360f;
            float diff = Mathf.Abs(Mathf.DeltaAngle(current, correct));
            Debug.Log($"{gameObject.name} 当前: {current}, 正确角度之一: {correct}, 差值: {diff}");
            if (diff <= tolerance) return true;
        }
        return false;
    }

    public void Randomize()
    {
        float baseAngle = correctAngles.Length > 0 ? correctAngles[0] : 0f;
        int steps = Random.Range(1, 8);
        float randomAngle = (baseAngle + steps * stepAngle) % 360f;
        transform.localEulerAngles = new Vector3(0, randomAngle, 0);
        targetAngle = randomAngle;
    }

    private IEnumerator Flash()
    {
        if (rend) rend.material.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        if (rend) rend.material.color = originalColor;
    }
}