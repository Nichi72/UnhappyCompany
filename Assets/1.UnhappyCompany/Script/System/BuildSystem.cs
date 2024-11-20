using UnityEngine;
using System.Collections.Generic;

public class BuildSystem : MonoBehaviour
{
    public static BuildSystem instance = null;
    public GameObject objectToPlace; // 설치할 객체
    public LayerMask groundLayer; // 설치 가능한 레이어
    public Material previewMaterial; // 설치 미리보기용 반투명 재질

    private GameObject currentObject; // 현재 설치 중인 객체
    private bool isPlacing = false; // 설치 모드 여부
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>(); // 원래 재질 백업용
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    void Update()
    {
        if (isPlacing)
        {
            MoveObjectToMouse(); // 마우스를 따라 객체 이동
            RotateObject(); // 객체 회전

            if (Input.GetMouseButtonDown(0))
            {
                PlaceObject(); // 설치 확정
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement(); // 설치 취소
            }
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            //StartPlacing(); // 설치 모드 시작
        }
    }

    // 설치 모드 시작 - 객체를 생성하고 미리보기 재질 적용
    public void StartPlacing(GameObject objectToPlace)
    {
        if (objectToPlace != null)
        {
            currentObject = Instantiate(objectToPlace); // 설치할 객체 인스턴스화
            SetPreviewMaterial(currentObject); // 미리보기 재질 적용

            // 객체의 Rigidbody가 있다면 설치 모드 동안 비활성화
            Rigidbody rb = currentObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            isPlacing = true; // 설치 모드 활성화
        }
    }

    // 마우스를 따라 객체 이동 - Raycast를 사용하여 지면 위치 계산
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

    // 객체 설치 확정 - 미리보기 재질 제거 및 Rigidbody 설정
    void PlaceObject()
    {
        if (currentObject.GetComponent<Rigidbody>() != null)
        {
            Rigidbody rb = currentObject.GetComponent<Rigidbody>();
            rb.isKinematic = false; // Rigidbody 물리 활성화
            rb.useGravity = true; // 중력 활성화
        }
        RemovePreviewMaterial(currentObject); // 미리보기 재질 제거
        currentObject = null;
        isPlacing = false; // 설치 모드 비활성화
        QuickSlotSystem.instance.DestroyCurrentItem();
    }

    // 설치 취소 - 현재 객체 삭제
    void CancelPlacement()
    {
        Destroy(currentObject); // 현재 설치 중인 객체 삭제
        currentObject = null;
        isPlacing = false; // 설치 모드 비활성화
    }

    // 미리보기 재질 설정 - 설치할 객체에 반투명 재질 적용
    void SetPreviewMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(); // 객체의 모든 Renderer 가져오기
        foreach (Renderer renderer in renderers)
        {
            originalMaterials[renderer] = renderer.material; // 원래 재질 백업
            renderer.material = previewMaterial; // 각 Renderer에 미리보기 재질 적용
        }
    }

    // 미리보기 재질 제거 - 설치 확정 시 원래 재질로 복원
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
        originalMaterials.Clear(); // 백업 재질 정보 삭제
    }

    GameObject GetBuildObject()
    {
        return null;
    }
}
