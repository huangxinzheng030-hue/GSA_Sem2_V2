using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class RingPiece : MonoBehaviour
{
    [Header("Puzzle")]
    [Range(0f, 360f)]
    public float correctAngle = 0f;

    [Header("Highlight (darken, not transparent)")]
    [Range(0.3f, 1f)] public float dimBrightness = 0.75f;
    [Range(0.3f, 1f)] public float selectedBrightness = 1f;

    private RectTransform rect;
    private Image uiImage;

    private float currentZ;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        uiImage = GetComponent<Image>(); // optional for highlight

        if (rect == null)
        {
            Debug.LogError($"{name}: RingPiece must be on a UI Image (RectTransform).");
            enabled = false;
            return;
        }

        // Keep rotation centered
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);

        currentZ = Normalize(rect.localEulerAngles.z);
        ApplyRotation();
    }

    public void SetAngle(float angleDeg)
    {
        if (rect == null) return;
        currentZ = Normalize(angleDeg);
        ApplyRotation();
    }

    public void StepRotate(float stepDeg)
    {
        SetAngle(currentZ + stepDeg);
    }

    public float CurrentAngle()
    {
        return currentZ;
    }

    public void SetHighlighted(bool selected)
    {
        if (uiImage == null) return;
        float b = selected ? selectedBrightness : dimBrightness;
        uiImage.color = new Color(b, b, b, 1f);
    }

    private void ApplyRotation()
    {
        // ✅ UI-only: force pure 2D rotation (Z axis only)
        rect.localEulerAngles = new Vector3(0f, 0f, currentZ);
    }

    private static float Normalize(float a)
    {
        a = (a % 360f + 360f) % 360f;
        return a;
    }
}

