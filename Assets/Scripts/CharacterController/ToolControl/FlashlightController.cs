using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlight;   // ÍÏ Spot Light
    private bool isOn = false;

    void Update()
    {
        // °´ F ¿ª¹Ø
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleLight();
        }
    }

    void ToggleLight()
    {
        isOn = !isOn;

        if (flashlight != null)
        {
            flashlight.enabled = isOn;
        }
    }
}