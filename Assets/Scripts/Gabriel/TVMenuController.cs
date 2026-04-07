using UnityEngine;

public class TVMenuController : MonoBehaviour
{
    [Header("Menu Items")]
    public TVMenuItemUI[] menuItems;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip moveClip;
    public AudioClip confirmClip;

    private int currentIndex = 0;

    private void Start()
    {
        currentIndex = 0;
        RefreshSelection();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            MoveUp();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            MoveDown();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            Confirm();
        }
    }

    void MoveUp()
    {
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = menuItems.Length - 1;

        RefreshSelection();
        PlaySound(moveClip);
    }

    void MoveDown()
    {
        currentIndex++;

        if (currentIndex >= menuItems.Length)
            currentIndex = 0;

        RefreshSelection();
        PlaySound(moveClip);
    }

    void Confirm()
    {
        PlaySound(confirmClip);

        switch (currentIndex)
        {
            case 0:
                Debug.Log("Start Game");
                break;

            case 1:
                Debug.Log("Options");
                break;

            case 2:
                Debug.Log("Quit");
                Application.Quit();
                break;
        }
    }

    void RefreshSelection()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i] != null)
                menuItems[i].SetSelected(i == currentIndex);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}