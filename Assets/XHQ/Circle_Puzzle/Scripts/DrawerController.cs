using UnityEngine;

public class DrawerController : MonoBehaviour
{
    public static DrawerController Instance;

    [Header("抽屉弹出偏移（本地坐标）")]
    public Vector3 openOffset = new Vector3(2f, 0, 0);

    [Header("动画速度")]
    public float openSpeed = 1.5f;

    [Header("解锁后显示的UI文字")]
    public GameObject successUI;

    [Header("音效")]
    public AudioClip unlockSound;          // 解锁音效，拖入 Inspector
    [Range(0f, 1f)]
    public float unlockSoundVolume = 1f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpening = false;
    private bool isOpen = false;
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        closedPos = transform.localPosition;
        openPos = closedPos + openOffset;
        if (successUI != null)
            successUI.SetActive(false);

        // 自动添加 AudioSource（如果没有）
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (isOpening && !isOpen)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition, openPos, openSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.localPosition, openPos) < 0.001f)
            {
                transform.localPosition = openPos;
                isOpen = true;
                isOpening = false;
                if (successUI != null)
                    successUI.SetActive(true);
            }
        }
    }

    public void OpenDrawer()
    {
        if (!isOpen)
        {
            isOpening = true;
            PlayUnlockSound();
        }
    }

    private void PlayUnlockSound()
    {
        if (unlockSound != null && audioSource != null)
            audioSource.PlayOneShot(unlockSound, unlockSoundVolume);
    }
}
