using UnityEngine;

public class RotateWheelByMouse : MonoBehaviour
{
    public Camera mainCamera;

    [Header("真正要旋转的物体")]
    public Transform rotationTarget;

    public enum Axis { X, Y, Z }

    [Header("旋转轴")]
    public Axis rotateAxis = Axis.X;

    [Header("设置")]
    public float rotationMultiplier = 1f;
    public bool invert = false;

    [Header("Audio")]
    public AudioSource rotateAudio;

    private bool dragging = false;
    private float startMouseAngle;
    private Vector3 startEuler;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (rotateAudio != null)
        {
            rotateAudio.loop = true;
            rotateAudio.playOnAwake = false;
            rotateAudio.Stop();
        }
    }

    private void OnMouseDown()
    {
        if (mainCamera == null || rotationTarget == null) return;

        dragging = true;
        startMouseAngle = GetMouseAngleFromTargetCenter();
        startEuler = rotationTarget.localEulerAngles;

        if (rotateAudio != null && !rotateAudio.isPlaying)
        {
            rotateAudio.Play();
        }
    }

    private void OnMouseUp()
    {
        dragging = false;

        if (rotateAudio != null && rotateAudio.isPlaying)
        {
            rotateAudio.Stop();
        }
    }

    private void Update()
    {
        if (!dragging || mainCamera == null || rotationTarget == null) return;

        float currentMouseAngle = GetMouseAngleFromTargetCenter();
        float delta = Mathf.DeltaAngle(startMouseAngle, currentMouseAngle);

        if (invert)
            delta = -delta;

        Vector3 euler = startEuler;

        switch (rotateAxis)
        {
            case Axis.X:
                euler.x = startEuler.x + delta * rotationMultiplier;
                break;
            case Axis.Y:
                euler.y = startEuler.y + delta * rotationMultiplier;
                break;
            case Axis.Z:
                euler.z = startEuler.z + delta * rotationMultiplier;
                break;
        }

        rotationTarget.localEulerAngles = euler;
    }

    private float GetMouseAngleFromTargetCenter()
    {
        Vector3 screenCenter = mainCamera.WorldToScreenPoint(rotationTarget.position);
        Vector2 dir = (Vector2)Input.mousePosition - (Vector2)screenCenter;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
}