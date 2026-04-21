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

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpening = false;
    private bool isOpen = false;

    void Awake()
    {
        Instance = this;
        closedPos = transform.localPosition;
        openPos = closedPos + openOffset;
        if (successUI != null)
            successUI.SetActive(false);
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
        if (!isOpen) isOpening = true;
    }
}