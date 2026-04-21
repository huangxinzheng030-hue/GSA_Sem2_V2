using UnityEngine;

public class PuzzleClickHandler : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Debug.Log("鼠标点击了");

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit))
        {
            Debug.Log("射线没有打到任何物体");
            return;
        }

        Debug.Log("射线打到了：" + hit.collider.gameObject.name);

        PuzzleRing ring = hit.collider.GetComponent<PuzzleRing>();
        if (ring == null)
        {
            Debug.Log("打到的物体没有PuzzleRing组件");
            return;
        }

        Vector3 ringCenterScreen = mainCam.WorldToScreenPoint(ring.transform.position);
        Vector3 hitPointScreen = mainCam.WorldToScreenPoint(hit.point);

        if (hitPointScreen.x < ringCenterScreen.x)
        {
            Debug.Log("旋转左");
            ring.RotateLeft();
        }
        else
        {
            Debug.Log("旋转右");
            ring.RotateRight();
        }
    }
}