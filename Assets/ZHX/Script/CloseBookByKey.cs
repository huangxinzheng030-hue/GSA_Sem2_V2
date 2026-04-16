using UnityEngine;

public class CloseBookByKey : MonoBehaviour
{
    public KeyCode closeKey = KeyCode.T;

    [Header("Book UI / Object")]
    public GameObject bookToClose;

    [Header("Book Logo To Show Again")]
    public GameObject bookLogoToShow;

    void Update()
    {
        if (Input.GetKeyDown(closeKey))
        {
            if (bookToClose != null && bookToClose.activeSelf)
            {
                bookToClose.SetActive(false);
            }

            if (bookLogoToShow != null)
            {
                bookLogoToShow.SetActive(true);
            }
        }
    }
}