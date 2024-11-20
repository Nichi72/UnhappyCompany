using UnityEngine;

public class CentralBatteryConsumer : MonoBehaviour, ICentralBatteryConsumer
{
    // 배터리 소모량을 저장하는 백킹 필드
    [Tooltip("This sets the amount of battery consumed per second.")]
    protected float batteryDrainPerSecond = 0.05f;

    // 배터리 소모량 프로퍼티
    public virtual float BatteryDrainPerSecond
    {
        get => batteryDrainPerSecond;
        set => batteryDrainPerSecond = value;
    }

    void Start()
    {
        CentralBatterySystem.Instance.RegisterConsumer(this); // 중앙 배터리 시스템에 등록
    }

    void OnDestroy()
    {
        CentralBatterySystem.Instance.UnregisterConsumer(this); // 중앙 배터리 시스템에서 등록 해제
    }

    // 배터리 소모 메서드
    public virtual void DrainBattery()
    {
        if (CentralBatterySystem.Instance != null)
        {
            CentralBatterySystem.Instance.DrainBattery(this, BatteryDrainPerSecond * Time.deltaTime);
        }
    }

    void Update()
    {
        DrainBattery(); // 매 프레임마다 배터리 소모
    }

    public virtual string GetConsumerName()
    {
        return gameObject.name;
    }
}
