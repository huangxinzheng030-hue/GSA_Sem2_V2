using TMPro;
using UnityEngine;

[ExecuteAlways]
public class AutoSizeBackground : MonoBehaviour
{
    public TMP_Text dialogueText;          
    public RectTransform background;      

    [Header("Padding")]
    public Vector2 padding = new Vector2(80f, 30f); 
    public float maxWidth = 1000f;                  
    public bool centerOnText = true;                 

    void Reset()
    {
        background = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (!dialogueText) return;
        if (!background) background = GetComponent<RectTransform>();

      
        dialogueText.ForceMeshUpdate();

      
        float w = Mathf.Min(dialogueText.preferredWidth, maxWidth);
        float h = dialogueText.preferredHeight;

     
        background.sizeDelta = new Vector2(w + padding.x, h + padding.y);

   
        if (centerOnText)
        {
            var textRect = dialogueText.rectTransform;
            background.anchoredPosition = textRect.anchoredPosition;
        }
    }
}
