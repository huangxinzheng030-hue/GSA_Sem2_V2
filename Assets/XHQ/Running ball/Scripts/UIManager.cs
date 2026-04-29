using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI hintText;

    [Header("时间控制 (秒)")]
    public float initialShowDuration = 3.0f;

    void Start()
    {
        if (hintText != null)
        {
            hintText.text = "Use Arrow Keys to move the ball";
            hintText.color = Color.white;
            hintText.gameObject.SetActive(true);
            
            StartCoroutine(HideTextRoutine(initialShowDuration));
        }
    }

    IEnumerator HideTextRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        hintText.gameObject.SetActive(false);
    }
}