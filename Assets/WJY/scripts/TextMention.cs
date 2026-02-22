using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TextMention : MonoBehaviour
{
    public GameObject promptUI;
    public GameObject objectToDeactivate; // 在按 F 时要关闭的空物体
    public string sceneToLoad;   // 要跳转的场景名

    private bool playerInRange = false;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            // 先关闭目标物体（如果有），再切换场景
            if (objectToDeactivate != null)
                objectToDeactivate.SetActive(false);

            SceneManager.LoadScene(sceneToLoad);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (promptUI != null)
                promptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }
}
