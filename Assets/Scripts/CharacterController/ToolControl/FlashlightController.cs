using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlight;   // ÍÏ Spot Light
    private bool isOn = true;
    public GameObject text;

    void Update()
    {
        // °´ F ¿ª¹Ø
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleLight();
            text.SetActive(false);
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