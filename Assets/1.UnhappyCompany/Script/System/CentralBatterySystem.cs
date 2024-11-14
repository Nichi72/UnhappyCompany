using UnityEngine;
using System.Collections.Generic;

public class CentralBatterySystem : MonoBehaviour
{
    public static CentralBatterySystem Instance; // 싱글톤 패턴으로 중앙 배터리 시스템 관리

    public float totalBatteryLevel = 1000.0f; // 중앙 배터리의 총량
    [SerializeField]
    private List<ICentralBatteryConsumer> batteryConsumers = new List<ICentralBatteryConsumer>();

    void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UIManager.Instance.UpdateTotalConsumers(batteryConsumers.Count);
        UIManager.Instance.UpdateTotalBatteryLevel(totalBatteryLevel);
    }

    // 중앙 배터리에서 전력을 소비하는 메서드
    public bool DrainBattery(ICentralBatteryConsumer consumer, float amount)
    {
        if (totalBatteryLevel >= amount)
        {
            totalBatteryLevel -= amount;
            UpdateConsumerInfo();
            UIManager.Instance.UpdateTotalBatteryLevel(totalBatteryLevel);
            return true;
        }
        else
        {
            Debug.Log("Not enough battery in the central system!");
            return false;
        }
    }

    // 소비자를 추가하는 메서드
    public void RegisterConsumer(ICentralBatteryConsumer consumer)
    {
        if (!batteryConsumers.Contains(consumer))
        {
            batteryConsumers.Add(consumer);
            UpdateConsumerInfo();
            UIManager.Instance.UpdateTotalConsumers(batteryConsumers.Count);
        }
    }

    // 소비자를 제거하는 메서드 (개체가 파괴될 때 사용)
    public void UnregisterConsumer(ICentralBatteryConsumer consumer)
    {
        if (batteryConsumers.Contains(consumer))
        {
            batteryConsumers.Remove(consumer);
            UpdateConsumerInfo();
            UIManager.Instance.UpdateTotalConsumers(batteryConsumers.Count);
        }
    }

    private void UpdateConsumerInfo()
    {
        UIManager.Instance.ClearConsumerInfo(); // 이전 텍스트 초기화

        foreach (var consumer in batteryConsumers)
        {
            UIManager.Instance.AddConsumerInfo($"{consumer.GetConsumerName()} - Battery drained: {consumer.BatteryDrainPerSecond:F2}");
        }
    }
}
