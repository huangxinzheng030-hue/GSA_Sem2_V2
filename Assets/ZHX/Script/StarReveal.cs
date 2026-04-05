using UnityEngine;

public class StarReveal : MonoBehaviour
{
    public GameObject starVisual;   
    public bool IsRevealed { get; private set; }

    public void Reveal()
    {
        if (IsRevealed) return;
        IsRevealed = true;

        if (starVisual != null)
            starVisual.SetActive(true);
    }
}