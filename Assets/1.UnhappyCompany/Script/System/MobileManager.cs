using UnityEngine;
using UnityEngine.InputSystem;

public class MobileManager : MonoBehaviour
{
    public GameObject uiObjmobile;
    public GameObject scanObj;
    private Player player;
    public Camera mobileCamera;
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
        if(Input.GetKeyDown(KeyCode.E))
        {
            CCTVManager.instance.NextCCTV();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CCTVManager.instance.BeforeCCTV();
        }
    }
}
