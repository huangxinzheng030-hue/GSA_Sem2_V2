using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public void RestartLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

     public string unlockId;
     public void CompletedAndReturn(string levelName)
    {
        // 标记挑战完成
        AllOfTheGame.Unlock(unlockId);

        // 返回主场景
        SceneManager.LoadScene(levelName);
    }
}
