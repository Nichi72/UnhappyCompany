using UnityEngine;
using System.Collections.Generic;

public class BuildSystem : MonoBehaviour
{
    // public static BuildSystem instance = null;
    public GameObject objectToPlace; // 배치할 객체
    public LayerMask groundLayer; // 배치 가능한 레이어
    public LayerMask wallLayer; // 벽 레이어
    public Material previewMaterial; // 배치 미리보기용 재질
    public Material validPlacementMaterial; // 설치 가능한 상태의 재질
    public Material invalidPlacementMaterial; // 설치 불가능한 상태의 재질
    public Player currentPlayer;
    public float wallCheckDistance = 0.5f; // 벽 감지 거리
    public float wallOffset = 0.05f; // 벽으로부터의 거리 오프셋
    public LayerMask playerLayer; // 플레이어 레이어

    private GameObject currentObject; // 현재 배치 중인 객체
    public bool isPlacing = false; // 배치 모드 여부
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>(); // 원래 재질 저장소
    [SerializeField] [ReadOnly] private GameObject currentItem;
    private bool canPlaceOnWall = false; // 벽에 설치 가능 여부
    private bool isWallMountable = false; // 벽에만 설치 가능한 아이템인지 여부
    
    private void Awake()
    {
        currentPlayer = GameManager.instance.currentPlayer;
    }
    
    void Update()
    {
        if(currentItem != null)
        {
            if(currentItem != GameManager.instance.currentPlayer.quickSlotSystem.currentItemObject)
            {
                // 빌드 시스템을 시작한 아이템과 현재 들고 있는 오브젝트의 정보가 다름. 즉 다른 오브젝트로 변경한것임.
                CancelPlacement(); // 배치 취소
            }
        }
        else
        {
            CancelPlacement();
        }
        
        if (isPlacing)
        {
            if (isWallMountable)
            {
                MoveObjectToWall(); // 벽에 객체 이동
            }
            else
            {
                MoveObjectToMouse(); // 마우스로 객체 이동
            }
            
            RotateObject(); // 객체 회전

            // 벽에만 설치 가능한 객체는 벽에 있을 때만 배치 가능
            bool canPlace = !isWallMountable || (isWallMountable && canPlaceOnWall);
            Debug.Log("canPlace: " + canPlace);
            
            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                Debug.Log("배치 확정: " + canPlace);
                PlaceObject(); // 배치 확정
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement(); // 배치 취소
            }
        }
    }

    // 배치 모드 시작 - 객체를 생성하고 미리보기 재질 적용
    public void StartPlacing(GameObject objectToPlace, GameObject currentItem, bool wallMountable = false, System.Action<GameObject> onPlacingStarted = null)
    {
        InteractionSystem.instance.isInteraction = false;
        this.currentItem = currentItem;
        this.isWallMountable = wallMountable; // 벽에만 설치 가능한지 설정
        
        if (objectToPlace != null)
        {
            currentObject = Instantiate(objectToPlace); // 배치할 객체 인스턴스화
            currentObject.name = objectToPlace.name + " (임시)";
            
            // 콜라이더 일시적으로 비활성화
            Collider[] colliders = currentObject.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
            
            SetPreviewMaterial(currentObject); // 미리보기 재질 적용

            // 객체에 Rigidbody가 있다면 배치 모드 동안 비활성화
            Rigidbody rb = currentObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            
            isPlacing = true; // 배치 모드 활성화
            
            // 콜백 함수 실행 (null이 아닐 경우)
            onPlacingStarted?.Invoke(currentObject);
        }
    }

    // 마우스로 객체 이동 - Raycast를 사용하여 위치 결정
    void MoveObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 마우스 위치에서 Ray 생성
        RaycastHit hit;

        // 플레이어와 Item 레이어를 제외한 레이어 마스크 생성
        int itemLayer = LayerMask.GetMask("Item");
        int layerMask = groundLayer & ~playerLayer & ~itemLayer;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            currentObject.transform.position = hit.point; // 객체 위치를 Ray 충돌 지점으로 설정
        }
    }
    
    // 벽에 객체 이동 - Raycast를 사용하여 벽에 위치 결정
    void MoveObjectToWall()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 마우스 위치에서 Ray 생성
        RaycastHit hit;

        // 플레이어와 Item 레이어를 제외한 레이어 마스크 생성
        int itemLayer = LayerMask.GetMask("Item");
        int layerMask = Physics.DefaultRaycastLayers & ~playerLayer & ~itemLayer;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            // 벽 레이어에 충돌했는지 확인
            canPlaceOnWall = (wallLayer == (wallLayer | (1 << hit.collider.gameObject.layer)));
            
            // 위치 설정 - 모든 표면에 미리보기 표시
            if (canPlaceOnWall)
            {
                // 벽의 법선 방향으로 오프셋을 적용하여 위치 설정
                Vector3 offsetPosition = hit.point + (hit.normal * wallOffset);
                currentObject.transform.position = offsetPosition;
                
                // 벽에 대해 객체 방향 조정 (벽에 수직으로)
                currentObject.transform.forward = hit.normal;
                UpdatePlacementMaterial(true); // 설치 가능 상태로 표시
            }
            else
            {
                // 벽이 아닌 곳에도 미리보기는 표시
                Vector3 offsetPosition = hit.point + (hit.normal * wallOffset);
                currentObject.transform.position = offsetPosition;
                currentObject.transform.forward = hit.normal;
                UpdatePlacementMaterial(false); // 설치 불가능 상태로 표시
            }
        }
        else
        {
            // 레이캐스트가 실패한 경우 카메라 전방에 위치시킴
            currentObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 3f;
            canPlaceOnWall = false;
            UpdatePlacementMaterial(false); // 설치 불가능 상태로 표시
        }
    }

    // 객체 회전 - R 키를 눌러 객체를 회전시킴
    void RotateObject()
    {
        if (Input.GetKey(KeyCode.R) && (!isWallMountable || !canPlaceOnWall)) // 벽에 설치 중일 때는 회전 제한
        {
            currentObject.transform.Rotate(Vector3.up, 100 * Time.deltaTime); // Y축 기준으로 회전
        }
    }

    // 객체 배치 확정 - 미리보기 재질 제거 및 Rigidbody 설정
    void PlaceObject()
    {
        // 벽에만 설치 가능한 객체인데 벽에 설치되지 않으면 리턴
        if (isWallMountable && !canPlaceOnWall)
        {
            Debug.LogWarning("벽에만 설치 가능한 객체는 벽에서만 설치할 수 있습니다.");
            return;
        }
            
        if (currentObject.GetComponent<Rigidbody>() != null)
        {
            Rigidbody rb = currentObject.GetComponent<Rigidbody>();
            rb.isKinematic = true; // Rigidbody 설정 활성화
            rb.useGravity = false; // 중력 활성화
        }
        
        // 콜라이더 다시 활성화
        Collider[] colliders = currentObject.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }
        
        currentObject.layer = LayerMask.NameToLayer(ETag.Item.ToString());
        RemovePreviewMaterial(currentObject); // 미리보기 재질 제거
        
        // ItemCushion 설치 콜백 호출 (Phase 1)
        var cushion = currentObject.GetComponent<ItemCushion>();
        if (cushion != null)
        {
            cushion.OnPlaced();
        }
        
        Destroy(currentItem);
        currentObject = null;
        isPlacing = false; // 배치 모드 비활성화
        InteractionSystem.instance.isInteraction = true;
        currentPlayer.quickSlotSystem.DestroyCurrentItem();
    }

    // 배치 취소 - 현재 객체 제거
    void CancelPlacement()
    {
        Destroy(currentObject); // 현재 배치 중인 객체 제거
        currentObject = null;
        isPlacing = false; // 배치 모드 비활성화
        InteractionSystem.instance.isInteraction = true;
    }

    // 미리보기 재질 적용 - 배치할 객체에 재질 적용
    void SetPreviewMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(); // 객체의 모든 Renderer 가져오기
        foreach (Renderer renderer in renderers)
        {
            originalMaterials[renderer] = renderer.material; // 원래 재질 저장
            renderer.material = previewMaterial; // 각 Renderer에 미리보기 재질 적용
        }
    }

    // 설치 가능 여부에 따른 재질 업데이트
    void UpdatePlacementMaterial(bool valid)
    {
        if (currentObject == null) return;
        
        Renderer[] renderers = currentObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = valid ? validPlacementMaterial : invalidPlacementMaterial;
        }
    }

    // 미리보기 재질 제거 - 배치 확정 후 원래 재질로 복원
    void RemovePreviewMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(); // 객체의 모든 Renderer 가져오기
        foreach (Renderer renderer in renderers)
        {
            if (originalMaterials.ContainsKey(renderer))
            {
                renderer.material = originalMaterials[renderer]; // 원래 재질로 복원
            }
        }
        originalMaterials.Clear(); // 재질 저장소 초기화
    }

    GameObject GetBuildObject()
    {
        return null;
    }
}
