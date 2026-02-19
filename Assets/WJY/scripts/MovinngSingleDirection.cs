using UnityEngine;

public class MovinngSingleDirection : MonoBehaviour
{
    public float moveDistance = 5f;
    public float minSpeed = 1f;
    public float maxSpeed = 5f;
    public float speedChangeRate = 1f;

    private Vector3 startPos;
    private int direction = 1;
    private float noiseOffset;

    void Start()
    {
        startPos = transform.position;
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * speedChangeRate, noiseOffset);
        float currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, noise);

        // ⭐ 直接改Y坐标（世界坐标）
        transform.position += new Vector3(direction * currentSpeed * Time.deltaTime, 0, 0);

        float offset = transform.position.x - startPos.x;

        if (Mathf.Abs(offset) >= moveDistance)
        {
            direction *= -1;
        }
    }
}
