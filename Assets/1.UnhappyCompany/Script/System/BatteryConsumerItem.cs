using UnityEngine;
/// <summary>
/// 휴대용 배터리 전용
/// </summary>
public abstract class BatteryConsumerItem : MonoBehaviour
{
    // 배터리 총량
    protected float batteryLTotalAmount = 100.0f;

    /// 1초당 소모되는 배터리 양
    protected abstract float BatteryDrainPerSecond { get; }

    // 배터리 소모 메서드 (각 하위 클래스에서 초당 배터리 소모를 처리)
    protected virtual void DrainBattery()
    {
        batteryLTotalAmount -= BatteryDrainPerSecond * Time.deltaTime;
        batteryLTotalAmount = Mathf.Max(batteryLTotalAmount, 0); // 배터리 잔량이 음수가 되지 않도록 설정
        Debug.Log($"Battery Level: {batteryLTotalAmount}");
    }

    // MonoBehaviour Update 메서드
    void Update()
    {
        if (batteryLTotalAmount > 0)
        {
            DrainBattery(); // 매 프레임마다 배터리 소모
        }
    }
}
