using UnityEngine;

public class CloseTheDoor : MonoBehaviour
{
    public string portalId; // 必须与 TextMention 的 portalId 一致
    public GameObject objectToDeactivate; // 不填就默认关自己

    void Awake()
    {
        if (string.IsNullOrEmpty(portalId)) return;

        if (AllOfTheGame.IsUnlocked(portalId))
        {
            if (objectToDeactivate != null) objectToDeactivate.SetActive(false);
            else gameObject.SetActive(false);
        }
    }
}
