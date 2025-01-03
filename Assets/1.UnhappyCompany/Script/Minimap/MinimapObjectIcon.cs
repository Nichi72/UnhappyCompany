using UnityEngine;

public class MinimapObjectIcon : MonoBehaviour
{
    public RectTransform icon; // 미니맵 아이콘
    public FixedOriginMinimap minimap; // 미니맵 클래스
    public Transform worldObject; // 월드 오브젝트 Transform

    void Update()
    {
        // 월드 좌표를 미니맵 좌표로 변환
        Vector2 minimapPosition = minimap.WorldToMinimap(worldObject.position);
        icon.anchoredPosition = minimapPosition;
    }
}
