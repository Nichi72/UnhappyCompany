using UnityEngine;
using UnityEngine.InputSystem;

public class MobileManager : MonoBehaviour
{
    public GameObject uiObjmobile;
    public Player player;
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
            player.firstPersonController._input.cursorLocked = !uiObjmobile.activeSelf;
            player.firstPersonController._input.cursorInputForLook = !uiObjmobile.activeSelf;
            Cursor.visible = uiObjmobile.activeSelf;
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Mouse.current.WarpCursorPosition(screenCenter);
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
