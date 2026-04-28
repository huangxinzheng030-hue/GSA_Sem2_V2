using UnityEngine;

public class StarLight : MonoBehaviour
{
    public GameObject glowCore;
    public GameObject glowSoft;

    [Header("Audio")]
    public AudioSource lightAudio;

    [Header("Puzzle")]
    public StarPuzzleManager puzzleManager;

    private bool isLit = false;

    private void Start()
    {
        if (glowCore != null)
            glowCore.SetActive(false);

        if (glowSoft != null)
            glowSoft.SetActive(false);
    }

    public void TurnOn()
    {
        if (isLit) return;

        isLit = true;

        if (glowCore != null)
            glowCore.SetActive(true);

        if (glowSoft != null)
            glowSoft.SetActive(true);

        if (lightAudio != null)
            lightAudio.Play();

        if (puzzleManager != null)
            puzzleManager.StarLit();

        Debug.Log(gameObject.name + " µ„¡¡¡À");
    }
}