using UnityEngine;

public class MinimapBoundaryHandler : MonoBehaviour
{
    public RectTransform icon; // 미니맵 아이콘
    public RectTransform minimapArea; // 미니맵 UI 영역
    public FixedOriginMinimap minimap; // 미니맵 클래스
    public Transform worldObject; // 월드 오브젝트 Transform

    void Update()
    {
        Vector2 minimapPosition = minimap.WorldToMinimap(worldObject.position);

        // 미니맵 경계 계산
        float halfWidth = minimapArea.sizeDelta.x / 2;
        float halfHeight = minimapArea.sizeDelta.y / 2;

        // 경계 내에 있는지 확인
        if (minimapPosition.x >= -halfWidth && minimapPosition.x <= halfWidth &&
            minimapPosition.y >= -halfHeight && minimapPosition.y <= halfHeight)
        {
            // 경계 안에 있을 경우 아이콘 활성화 및 위치 설정
            icon.gameObject.SetActive(true);
            icon.anchoredPosition = minimapPosition;
        }
        else
        {
            // 경계 밖으로 나가면 아이콘 비활성화
            icon.gameObject.SetActive(false);
        }
    }
}