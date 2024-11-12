using UnityEngine;

public class CCTV : MonoBehaviour
{
    public Camera currentCamera;
    void Start()
    {
        CCTVManager.instance.cctvs.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        CCTVManager.instance.cctvs.Remove(this);
    }
}
