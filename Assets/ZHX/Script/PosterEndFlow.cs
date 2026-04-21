using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PosterEndFlow : MonoBehaviour
{
    public Animator posterAnimator;   
    public string animationStateName; 
    public GameObject buttonObject;   
    public float waitAfterAnimation = 2f;
    public string menuSceneName = "Menu"; 

    private void Start()
    {
        StartCoroutine(WaitAndShowButton());
    }

    private IEnumerator WaitAndShowButton()
    {
   
        yield return null; 

        AnimatorStateInfo stateInfo = posterAnimator.GetCurrentAnimatorStateInfo(0);

       
        while (!stateInfo.IsName(animationStateName))
        {
            yield return null;
            stateInfo = posterAnimator.GetCurrentAnimatorStateInfo(0);
        }

        
        while (stateInfo.normalizedTime < 1f)
        {
            yield return null;
            stateInfo = posterAnimator.GetCurrentAnimatorStateInfo(0);
        }

        yield return new WaitForSeconds(waitAfterAnimation);

        
        buttonObject.SetActive(true);
    }

    
    public void GoToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}