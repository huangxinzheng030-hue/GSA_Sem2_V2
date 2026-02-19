using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public void RestartLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
