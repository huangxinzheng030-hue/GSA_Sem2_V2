using UnityEngine;

public class StarLight : MonoBehaviour
{
    [Header("Glow Objects")]
    public GameObject glowCore;
    public GameObject glowSoft;

    [HideInInspector] public bool isLit = false;

    private void Awake()
    {
        SetUnlit();
    }

    public void LightUp()
    {
        if (isLit) return;

        isLit = true;

        if (glowCore != null)
            glowCore.SetActive(true);

        if (glowSoft != null)
            glowSoft.SetActive(true);

        PuzzleProgressManager manager = Object.FindFirstObjectByType<PuzzleProgressManager>();
        if (manager != null)
        {
            manager.OnStarLit();
        }
    }

    public void SetUnlit()
    {
        isLit = false;

        if (glowCore != null)
            glowCore.SetActive(false);

        if (glowSoft != null)
            glowSoft.SetActive(false);
    }
}