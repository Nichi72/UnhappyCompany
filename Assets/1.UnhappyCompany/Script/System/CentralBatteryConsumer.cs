using UnityEngine;
public class CentralBatteryConsumer : MonoBehaviour, ICentralBatteryConsumer
{
    // 1초당 소모되는 배터리 양
    [Tooltip("This sets the amount of battery consumed per second.")]
    [SerializeField] public float BatteryDrainPerSecond { get; set; } = 0.05f;

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

    // MonoBehaviour Update 메서드
    void Update()
    {
        DrainBattery(); // 매 프레임마다 배터리 소모
    }

    // 소비자의 이름을 반환하는 메서드
    public virtual string GetConsumerName()
    {
        return gameObject.name;
    }
}
