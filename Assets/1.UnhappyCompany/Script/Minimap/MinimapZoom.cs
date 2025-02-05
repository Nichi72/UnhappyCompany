using UnityEngine;

public class MinimapZoom : MonoBehaviour
{
    public RectTransform minimapArea; // 미니맵 영역
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 2f;

    void Update()
    {
        // 마우스 스크롤로 줌 조정
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newScale = Mathf.Clamp(minimapArea.localScale.x + scroll * zoomSpeed, minZoom, maxZoom);
            minimapArea.localScale = new Vector3(newScale, newScale, 1);
        }
    }
}
