using UnityEngine;

public class MobileManager : MonoBehaviour
{
    public GameObject uiObjmobile;
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            uiObjmobile.SetActive(!uiObjmobile.activeSelf);
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
