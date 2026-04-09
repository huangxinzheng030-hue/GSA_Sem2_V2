using UnityEngine;

public class Esc : MonoBehaviour
{
    public GameObject EscMenu;

    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            EscPressDown();
        }
    }
    void EscPressDown(){
        bool isActive = EscMenu.activeSelf;
        EscMenu.SetActive(!isActive);

        Time.timeScale = isActive ? 1f : 0f;
    }
}
