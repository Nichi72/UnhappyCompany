using UnityEngine;
using UnityEngine.UI;
public class MinimapUIBtnCCTV : MonoBehaviour
{
    public CCTV cctv;
    public Button btn;
    public Image img;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void BtnPressed()
    {
        CCTVManager.instance.PressedCCTV(cctv);
    }
}
