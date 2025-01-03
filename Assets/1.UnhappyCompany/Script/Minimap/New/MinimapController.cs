using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour
{
    // 매니저 스크립트로
    public static MinimapController instance;
   

    [Header("Minimap Camera 세팅")]
    [SerializeField] private Camera minimapCamera;

    [Header("미니맵 이동 & 줌 세팅")]
    [SerializeField] private float dragSpeed = 50f;
    [SerializeField] private float zoomSpeed = 200f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 50f;

    [Header("UI RectTransform")]
    [SerializeField] private RectTransform minimapRect;

    [Header("아이콘 오프셋")]
    [Tooltip("미니맵 UI 상에서 아이콘이 실제 위치보다 약간 움직이게 하고 싶을 때 사용.")]
    public Vector2 iconOffset = Vector2.zero;

    private Vector3 lastMousePos;

    [Header("플레이어 설정")]
    private Transform playerTransform; // 플레이어의 Transform

    [Header("아이콘 프리팹")]
    [SerializeField] private GameObject playerIconPrefab; // UI 아이콘 프리팹

    [System.Serializable]
    public class MinimapIcon
    {
        public Transform worldTarget;    // 추적할 월드 오브젝트
        public RectTransform iconUI;     // 미니맵 위에 표시할 아이콘(UI Image 등)
    }

    [Header("아이콘 추적 리스트")]
    public List<MinimapIcon> minimapIcons;
     private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("MinimapController 중복 생성 방지");
        }
    }
    private void Start()
    {
        playerTransform = GameManager.instance.currentPlayer.transform;
        // 플레이어의 위치로 미니맵 카메라 초기 위치 설정
        if (playerTransform != null)
        {
            Vector3 playerPosition = playerTransform.position;
            minimapCamera.transform.position = new Vector3(playerPosition.x, minimapCamera.transform.position.y, playerPosition.z);
            AddMinimapIcon(playerTransform, playerIconPrefab);
        }
    }

    private void Update()
    {
        HandleMinimapPan();
        HandleMinimapZoom();
        UpdateIconsPosition();
    }

    private void HandleMinimapPan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;

            float moveX = -delta.x * dragSpeed * Time.deltaTime;
            float moveZ = -delta.y * dragSpeed * Time.deltaTime;

            minimapCamera.transform.Translate(moveX, 0f, moveZ, Space.World);

            lastMousePos = Input.mousePosition;
        }
    }

    private void HandleMinimapZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = minimapCamera.orthographicSize - (scroll * zoomSpeed * Time.deltaTime);
            minimapCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    private void UpdateIconsPosition()
    {
        Vector2 mapSize = minimapRect.sizeDelta;

        foreach (var icon in minimapIcons)
        {
            if (icon.worldTarget == null || icon.iconUI == null) 
                continue;

            // 월드 좌표 → 뷰포트 좌표 (0~1 범위)
            Vector3 worldPos = icon.worldTarget.position;
            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldPos);

            // 뷰포트 (0..1)를 미니맵 UI 좌표로 변환 (Pivot = 0.5, 0.5 기준)
            float uiX = (viewportPos.x - 0.5f) * mapSize.x;
            float uiY = (viewportPos.y - 0.5f) * mapSize.y;

            // 오프셋 적용
            uiX += iconOffset.x;
            uiY += iconOffset.y;

            icon.iconUI.anchoredPosition = new Vector2(uiX, uiY);

            // 뷰포트 범위를 벗어나면 숨기거나 처리
            bool isOutOfRange = (viewportPos.z < 0f ||
                                 viewportPos.x < 0f || viewportPos.x > 1f ||
                                 viewportPos.y < 0f || viewportPos.y > 1f);

            icon.iconUI.gameObject.SetActive(!isOutOfRange);
        }
    }

    public GameObject AddMinimapIcon(Transform worldTarget, GameObject iconPrefab)
    {
        if (iconPrefab == null || worldTarget == null)
        {
            Debug.LogError("아이콘 프리팹이 없거나 월드 타켓이 없습니다.");
            return null;
        }

        GameObject iconObject = Instantiate(iconPrefab, minimapRect);
        RectTransform iconUI = iconObject.GetComponent<RectTransform>();

        MinimapIcon newIcon = new MinimapIcon
        {
            worldTarget = worldTarget,
            iconUI = iconUI
        };

        minimapIcons.Add(newIcon);
        return iconObject;
    }

    public void RemoveMinimapIcon(Transform worldTarget)
    {
        MinimapIcon iconToRemove = minimapIcons.Find(icon => icon.worldTarget == worldTarget);
        if (iconToRemove != null)
        {
            Destroy(iconToRemove.iconUI.gameObject);
            minimapIcons.Remove(iconToRemove);
        }
    }
}
