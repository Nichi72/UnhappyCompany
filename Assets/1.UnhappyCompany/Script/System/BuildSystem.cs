using UnityEngine;
using System.Collections.Generic;

public class BuildSystem : MonoBehaviour
{
    // public static BuildSystem instance = null;
    public GameObject objectToPlace; // 배치할 객체
    public LayerMask groundLayer; // 배치 가능한 레이어
    public Material previewMaterial; // 배치 미리보기용 재질
    public Player currentPlayer;

    private GameObject currentObject; // 현재 배치 중인 객체
    private bool isPlacing = false; // 배치 모드 여부
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>(); // 원래 재질 저장소
    private void Awake()
    {
        // if(instance == null)
        // {
        //     instance = this;
        // }
    }
    void Update()
    {
        if (isPlacing)
        {
            MoveObjectToMouse(); // 마우스로 객체 이동
            RotateObject(); // 객체 회전

            if (Input.GetMouseButtonDown(0))
            {
                PlaceObject(); // 배치 확정
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement(); // 배치 취소
            }
        }
    }

    // 배치 모드 시작 - 객체를 생성하고 미리보기 재질 적용
    public void StartPlacing(GameObject objectToPlace)
    {
        if (objectToPlace != null)
        {
            currentObject = Instantiate(objectToPlace); // 배치할 객체 인스턴스화
            SetPreviewMaterial(currentObject); // 미리보기 재질 적용

            // 객체에 Rigidbody가 있다면 배치 모드 동안 비활성화
            Rigidbody rb = currentObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            isPlacing = true; // 배치 모드 활성화
        }
    }

    // 마우스로 객체 이동 - Raycast를 사용하여 위치 결정
    void MoveObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 마우스 위치에서 Ray 생성
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            currentObject.transform.position = hit.point; // 객체 위치를 Ray 충돌 지점으로 설정
        }
    }

    // 객체 회전 - R 키를 눌러 객체를 회전시킴
    void RotateObject()
    {
        if (Input.GetKey(KeyCode.R))
        {
            currentObject.transform.Rotate(Vector3.up, 100 * Time.deltaTime); // Y축 기준으로 회전
        }
    }

    // 객체 배치 확정 - 미리보기 재질 제거 및 Rigidbody 설정
    void PlaceObject()
    {
        if (currentObject.GetComponent<Rigidbody>() != null)
        {
            Rigidbody rb = currentObject.GetComponent<Rigidbody>();
            rb.isKinematic = true; // Rigidbody 설정 활성화
            rb.useGravity = false; // 중력 활성화
        }
        RemovePreviewMaterial(currentObject); // 미리보기 재질 제거
        currentObject = null;
        isPlacing = false; // 배치 모드 비활성화
        currentPlayer.quickSlotSystem.DestroyCurrentItem();
    }

    // 배치 취소 - 현재 객체 제거
    void CancelPlacement()
    {
        Destroy(currentObject); // 현재 배치 중인 객체 제거
        currentObject = null;
        isPlacing = false; // 배치 모드 비활성화
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
