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
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        player = GameManager.instance.currentPlayer;
        CCTVManager.instance.mobileManager = this;
    }

    void Update()
    {
        if(scanObj.activeSelf == true)
        {
            player.firstPersonController._input.SetCursorLock(true,false);
            Cursor.visible = false;
            mobileCamera.enabled = true;
        }
        else
        {
            if(mobileCamera.enabled == true)
            {
                mobileCamera.enabled = false;
            }
        }
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            uiObjmobile.SetActive(!uiObjmobile.activeSelf);
            player.firstPersonController._input.SetCursorLock(!uiObjmobile.activeSelf);
        }
        if(Input.GetKeyDown(KeyCode.Q) && scanObj.activeSelf == true)
        {
            multiRaycastOcclusionCheck.ScanForEnemies();
        }
        if(Input.GetKeyDown(KeyCode.E) && scanObj.activeSelf == true)
        {
            // 스캔 종료
            player.firstPersonController._input.SetCursorLock(false,true);
            Cursor.visible = true;
            mobileCamera.enabled = false;
            scanObj.SetActive(false);
        }
    

        if(Input.GetKeyDown(KeyCode.E) && uiObjmobile.activeSelf == true)
        {
            CCTVManager.instance.NextCCTV();
        }
        if (Input.GetKeyDown(KeyCode.Q) && uiObjmobile.activeSelf == true)
        {
            CCTVManager.instance.BeforeCCTV();
        }
    }

    
}
