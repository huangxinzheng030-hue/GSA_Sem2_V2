using UnityEngine;

public class CloseUIOnClick : MonoBehaviour
{
    public GameObject targetToClose;
    public GameObject logoToShow;

    public void CloseTarget()
    {
        if (targetToClose != null)
        {
            targetToClose.SetActive(false);
        }

        if (logoToShow != null)
        {
            logoToShow.SetActive(true);
        }
    }
}