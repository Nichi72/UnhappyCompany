using UnityEngine;
using UnityEngine.InputSystem;

public class MobileManager : MonoBehaviour
{
    public GameObject uiObjmobile;
    private Player player;
    void Start()
    {
        player = GameManager.instance.currentPlayer;
    }

    void Update()
    {
        // Debug.Log($"Cursor.visible = {Cursor.visible}");
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
