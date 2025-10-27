using UnityEngine;

public class CenterBattery : BuiltInFacilityBase
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

    #region IScannable Override
    
    /// <summary>
    /// 센터 배터리 스캔 시 이름 반환
    /// </summary>
    public override string GetScanName()
    {
        return "Center Battery";
    }

    /// <summary>
    /// 센터 배터리 스캔 시 상세 정보 반환 (배터리 레벨 포함)
    /// </summary>
    public override string GetScanDescription()
    {
        if (centralBatterySystem != null)
        {
            float batteryPercent = (centralBatterySystem.currentBatteryLevel / centralBatterySystem.MaxBatteryLevel) * 100f;
            string batteryInfo = $"Battery: {centralBatterySystem.currentBatteryLevel:F0}/{centralBatterySystem.MaxBatteryLevel:F0} ({batteryPercent:F0}%)";
            
            int consumerCount = CentralBatterySystem.Instance?.CalculateTotalBatteryDrainPerSecond() > 0 ? 1 : 0;
            string statusInfo = centralBatterySystem.isStop ? "Status: STOPPED" : "Status: Active";
            
            return $"{batteryInfo} | {statusInfo}";
        }
        return "Center Battery System";
    }

    /// <summary>
    /// 센터 배터리가 스캔되었을 때 호출
    /// </summary>
    public override void OnScanned()
    {
        base.OnScanned();
        Debug.Log($"[CenterBattery] 현재 배터리: {centralBatterySystem?.currentBatteryLevel:F0}/{centralBatterySystem?.MaxBatteryLevel:F0}");
    }
    
    #endregion
}
