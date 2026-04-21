using UnityEngine;

public class HideBookByKey : MonoBehaviour
{
    public KeyCode hideKey = KeyCode.T;

    void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(hideKey))
        {
            gameObject.SetActive(false);
        }
    }
}