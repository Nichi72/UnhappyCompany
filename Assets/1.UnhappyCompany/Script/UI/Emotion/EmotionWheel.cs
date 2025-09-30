using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EmotionWheel : MonoBehaviour
{
    // 감정 아이콘 정보를 담은 구조체
    [System.Serializable]
    public class EmotionData {
        public Sprite icon;  // 사용할 이미지
        public int order;    // 배치 순서 (낮은 숫자부터)
    }
    
    // Inspector에 설정할 데이터들
    public List<EmotionData> emotionDataList = new List<EmotionData>();
    public GameObject iconPrefab;      // 감정 아이콘 프리팹 (Image 컴포넌트 포함)
    public RectTransform wheelRect;    // 원형 휠이 들어있는 패널의 RectTransform
    public float radius = 100f;        // 아이콘을 배치할 반지름
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    
    // 선택 영역의 보정용 오프셋 (degrees 단위)
    public float selectionAngleOffset = 0f;
    
    // 내부에서 관리할 리스트와 변수
    private List<Image> emotionIcons = new List<Image>();
    private int selectedIndex = -1;
    
    void Start()
    {
        // order 기준 정렬
        emotionDataList.Sort((a, b) => a.order.CompareTo(b.order));
        GenerateEmotionWheel();
    }
    
    // 아이콘들을 원형으로 배치하는 함수
    void GenerateEmotionWheel()
    {
        // 기존 자식 제거 (에디터 테스트 시 중복 방지)
        foreach (Transform child in wheelRect)
        {
            Destroy(child.gameObject);
        }
        emotionIcons.Clear();
        
        int count = emotionDataList.Count;
        for (int i = 0; i < count; i++)
        {
            // 아이콘의 중심 각도 (0, 360 등으로 균등 배치)
            float angle = i * (360f / count);
            float rad = angle * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            
            // 프리팹 인스턴스 생성 후 배치
            GameObject iconGO = Instantiate(iconPrefab, wheelRect);
            iconGO.name = "EmotionIcon_" + i;
            RectTransform rt = iconGO.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            
            Image img = iconGO.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = emotionDataList[i].icon;
                img.color = normalColor;
            }
            emotionIcons.Add(img);
        }
    }
    
    void Update()
    {
        // 마우스 포인터의 스크린 좌표를 휠의 로컬 좌표로 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(wheelRect, Input.mousePosition, null, out localPoint);
        
        // 중앙을 기준으로 마우스 방향 각도 계산 (오른쪽이 0°)
        float angle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle += 360f;
        
        int count = emotionIcons.Count;
        if (count == 0)
            return;
        
        // 각 영역의 크기 (예: 360°/count)
        float segmentAngle = 360f / count;
        // 마우스 각도에서 섹터의 절반 각도를 빼고, 오프셋을 추가하여 보정
        float adjustedAngle = angle - (segmentAngle / 2f) + selectionAngleOffset;
        // 보정 값이 음수이면 360을 더하고, 360 이상이면 360을 빼서 0~360 범위로 맞춤
        if (adjustedAngle < 0)
            adjustedAngle += 360f;
        if (adjustedAngle >= 360f)
            adjustedAngle -= 360f;
        
        int newIndex = Mathf.FloorToInt(adjustedAngle / segmentAngle);
        
        // 선택된 아이콘 색상 업데이트
        if (newIndex != selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < emotionIcons.Count)
                emotionIcons[selectedIndex].color = normalColor;
            
            selectedIndex = newIndex;
            emotionIcons[selectedIndex].color = selectedColor;
        }
        
        // 마우스 좌클릭 시 감정 선택 처리
        if (Input.GetMouseButtonDown(0))
        {
            OnEmotionSelected(selectedIndex);
        }
    }
    
    // 감정 선택 후 처리 함수
    void OnEmotionSelected(int index)
    {
        Debug.Log("선택된 감정 인덱스: " + index);
        PlaySelectionAnimation(index);
    }
    
    // 추후 애니메이션 추가를 위한 임시 함수
    void PlaySelectionAnimation(int index)
    {
        Debug.Log("애니메이션 재생: 감정 인덱스 " + index);
        // TODO: 선택 시 애니메이션 코드를 추가하세요.
    }
}
