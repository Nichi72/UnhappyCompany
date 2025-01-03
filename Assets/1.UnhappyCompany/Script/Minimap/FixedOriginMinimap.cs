using UnityEngine;

public class FixedOriginMinimap : MonoBehaviour
{
    public RectTransform minimapArea; // 미니맵 UI 영역
    public Transform originObject; // 원점 오브젝트
    public float minimapScale = 1f; // 월드 좌표 -> 미니맵 좌표로 변환하는 스케일

    // 월드 좌표를 미니맵 좌표로 변환
    public Vector2 WorldToMinimap(Vector3 worldPosition)
    {
        Vector3 relativePosition = worldPosition - originObject.position;

        // 월드 좌표를 미니맵 스케일로 변환
        float minimapX = relativePosition.x * minimapScale;
        float minimapY = relativePosition.z * minimapScale;

        return new Vector2(minimapX, minimapY);
    }
}
