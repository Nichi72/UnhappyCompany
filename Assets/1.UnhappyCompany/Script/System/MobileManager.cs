using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MobileManager : MonoBehaviour
{
    public GameObject uiObjmobile;
    public GameObject scanObj;
    private Player player;
    public Camera mobileCamera;
    public ScanningSystem multiRaycastOcclusionCheck;
    public static MobileManager instance;
    
    // UI 상태 추적을 위한 변수
    private bool previousMobileUIState;
    
    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        player = GameManager.instance.currentPlayer;
        CCTVManager.instance.mobileManager = this;
        mobileCamera.enabled = false;
        
        // 초기 상태 저장
        if (uiObjmobile != null)
        {
            previousMobileUIState = uiObjmobile.activeSelf;
        }
    }
    
    // UI 상태가 변경될 때 호출되는 빈 함수
    private void OnMobileUIStateChanged(bool isActive)
    {
        // 이 함수는 uiObjmobile.activeSelf 상태가 변경될 때마다 실행됩니다
        Debug.Log($"모바일 UI 상태 변경됨: {isActive}");
        if(isActive == true)
        {
            // CCTVManager.instance.TurnOnOffAllCCTVCamera(true);
            CCTVManager.instance.TurnOnOffCurrentCCTV(true);
        }
        else
        {
            // CCTVManager.instance.TurnOnOffAllCCTVCamera(false);
            CCTVManager.instance.TurnOnOffCurrentCCTV(false);
        }
    }
    
    void Update()
    {
        if (scanObj.activeSelf == true)
        {
            player.firstPersonController._input.SetCursorLock(true, false);
            Cursor.visible = false;
            mobileCamera.enabled = true;
        }
        else
        {
            if (mobileCamera.enabled == true)
            {
                mobileCamera.enabled = false;
            }
        }
        
        // UI 상태 변경 감지
        if (uiObjmobile != null && uiObjmobile.activeSelf != previousMobileUIState)
        {
            OnMobileUIStateChanged(uiObjmobile.activeSelf);
            previousMobileUIState = uiObjmobile.activeSelf;
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            uiObjmobile.SetActive(!uiObjmobile.activeSelf);
            if (CCTVManager.instance.IsAllCCTVsDisabled())
            {
                Debug.Log("모든 CCTV가 비활성화되어 있습니다.");
                if (CCTVManager.instance.cctvs.Count > 0)
                {
                    var cctv = CCTVManager.instance.cctvs[0];
                    if (cctv.currentCamera.enabled == false)
                    {
                        cctv.currentCamera.enabled = true;
                    }
                }
            }
            player.firstPersonController._input.SetCursorLock(!uiObjmobile.activeSelf);
        }
        else if (Input.GetKeyDown(KeyCode.Q) && scanObj.activeSelf == true)
        {
            multiRaycastOcclusionCheck.ScanForEnemies();
        }
        else if (Input.GetKeyDown(KeyCode.E) && scanObj.activeSelf == true)
        {
            // 스캔 종료
            player.firstPersonController._input.SetCursorLock(false, true);
            Cursor.visible = true;
            mobileCamera.enabled = false;
            scanObj.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.E) && uiObjmobile.activeSelf == true)
        {
            CCTVManager.instance.NextCCTV();
        }
        else if (Input.GetKeyDown(KeyCode.Q) && uiObjmobile.activeSelf == true)
        {
            CCTVManager.instance.BeforeCCTV();
        }
    }
}
