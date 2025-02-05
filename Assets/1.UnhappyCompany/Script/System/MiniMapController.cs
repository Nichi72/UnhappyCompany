using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MiniMapController : MonoBehaviour
{
    public Camera miniMapCamera;
    public float zoomSpeed = 2f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float dragSpeed = 10f; // 드래그 속도 설정
    
   
    private void Update()
    {
        // 마우스 휠 입력에 따라 줌 조정
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // 마우스 위치를 기준으로 줌 인/아웃
        // 마우스 드래그로 미니맵 이동
        if (Input.GetMouseButton(0)) // 마우스 좌클릭
        {
            Vector3 mouseDelta = new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"));
            miniMapCamera.transform.position -= mouseDelta * miniMapCamera.orthographicSize * dragSpeed / 5f;
        }

        // 마우스 휠로 줌 조절
        if (scroll != 0.0f)
        {
            // 새로운 카메라 크기 계산 및 적용
            float newSize = miniMapCamera.orthographicSize - scroll * zoomSpeed;
            miniMapCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
        // 스페이스바를 누르면 플레이어 위치로 카메라 이동
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 playerPos = player.transform.position;
                miniMapCamera.transform.position = new Vector3(playerPos.x, miniMapCamera.transform.position.y, playerPos.z);
            }
        }

        UpdateCCTVIconPositions();
    }
        
    public void OnCCTVButtonClick()
    {
        
    }
    private void UpdateCCTVIconPositions()
    {
        var cctvs = CCTVManager.instance.cctvs;
        var cctvsBtn = UIManager.instance.cctvButtons;
        
        for (int i = 0; i < cctvs.Count; i++)
        {
            if (cctvs[i] != null && cctvsBtn[i] != null)
            {
                // CCTV 오브젝트의 월드 위치를 미니맵 상의 위치로 변환
                Vector3 cctvPosition = cctvs[i].transform.position;
                Vector3 iconPosition = miniMapCamera.WorldToScreenPoint(cctvPosition);
                
                // UI 버튼의 위치를 CCTV 위치와 동기화
                cctvsBtn[i].transform.position = iconPosition;
            }
        }
    }
}

   

   