using UnityEngine;

public class CentralBatteryConsumer : MonoBehaviour, ICentralBatteryConsumer
{
    [SerializeField] protected string id;
    
    [Tooltip("This sets the amount of battery consumed per second.")]
    protected float batteryDrainPerSecond = 0.05f;

    public string ID => id;

    public virtual float BatteryDrainPerSecond
    {
        get => batteryDrainPerSecond;
        set => batteryDrainPerSecond = value;
    }

    protected virtual void Start()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString();
        }
        CentralBatterySystem.Instance.RegisterConsumer(this); // 중앙 배터리 시스템에 소비자 등록
    }

    void OnDestroy()
    {
        CentralBatterySystem.Instance.UnregisterConsumer(this); // 중앙 배터리 시스템에서 소비자 제거
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
        DrainBattery(); 
    }

    public virtual string GetConsumerName()
    {
        return gameObject.name;
    }
}
