using UnityEngine;

public class CenterBattery : MonoBehaviour
{
    CentralBatterySystem centralBatterySystem;
    public Transform battery100;
    public Transform battery0;
    public Transform battery;

    void Awake()
    {
        
    }
    void Start()
    {
        centralBatterySystem = CentralBatterySystem.Instance;
        // 배터리 레벨에 따라 배터리 위치 보간
        UpdateBatteryPosition();
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBatteryPosition();
    }

    void UpdateBatteryPosition()
    {
        float t = centralBatterySystem.currentBatteryLevel / centralBatterySystem.MaxBatteryLevel; // 1000이 최대값이므로 0~1 사이 값으로 정규화
        t = Mathf.Clamp01(t); // 0~1 사이로 제한
        battery.position = Vector3.Lerp(battery0.position, battery100.position, t);
    }
}
