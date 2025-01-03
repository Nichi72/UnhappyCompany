using UnityEngine;

public class PlayerMinimapIcon : MonoBehaviour
{
    public RectTransform playerIcon; // 플레이어 아이콘
    public FixedOriginMinimap minimap; // 미니맵 클래스
    public Transform player; // 플레이어 Transform

    void Update()
    {
        // 플레이어의 월드 좌표를 미니맵 좌표로 변환
        Vector2 minimapPosition = minimap.WorldToMinimap(player.position);
        playerIcon.anchoredPosition = minimapPosition;
    }
}
