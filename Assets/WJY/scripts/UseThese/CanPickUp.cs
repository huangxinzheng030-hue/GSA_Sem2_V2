using UnityEngine;

public class CanPickUp : MonoBehaviour
{
    public string unlockId;
    public string collectibleTag = "Pickup";

    void Start()
    {
        if (AllOfTheGame.IsUnlocked(unlockId))
        {
            gameObject.tag = collectibleTag;
        }
        else
        {
            gameObject.tag = "Untagged";
        }
    }
}
