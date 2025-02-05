using UnityEngine;
using System.Collections.Generic;

public class CentralBatterySystem : MonoBehaviour
{
    public static CentralBatterySystem Instance; // 싱글톤 패턴으로 중앙 배터리 시스템 관리
    public float MaxBatteryLevel = 1000.0f; // 중앙 배터리의 총량
    [ReadOnly] public float currentBatteryLevel = 1000.0f; // 중앙 배터리의 총량
    [SerializeField]
    private List<ICentralBatteryConsumer> batteryConsumers = new List<ICentralBatteryConsumer>();

    public bool isStop = false;
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
        currentBatteryLevel = MaxBatteryLevel;
        UIManager.instance.UpdateTotalConsumers(batteryConsumers.Count);
        UIManager.instance.UpdateTotalBatteryLevel(currentBatteryLevel);
    }

    // 중앙 배터리에서 전력을 소비하는 메서드
    public void DrainBattery(ICentralBatteryConsumer consumer, float amount)
    {
        if(isStop)
        {
            return;
        }
        if (currentBatteryLevel >= amount)
        {
            currentBatteryLevel -= amount;
            UpdateConsumerInfo();
            UIManager.instance.UpdateTotalBatteryLevel(currentBatteryLevel);
            // return true;
        }
        else
        {
            Debug.Log("Not enough battery in the central system!");
        }
    }

    // 소비자를 추가하는 메서드
    public void RegisterConsumer(ICentralBatteryConsumer consumer)
    {
        if (!batteryConsumers.Contains(consumer))
        {
            batteryConsumers.Add(consumer);
            UpdateConsumerInfo();
            UIManager.instance.UpdateTotalConsumers(batteryConsumers.Count);
        }
    }

    // 소비자를 제거하는 메서드 (객체가 파괴될 때 사용)
    public void UnregisterConsumer(ICentralBatteryConsumer consumer)
    {
        if (batteryConsumers.Contains(consumer))
        {
            batteryConsumers.Remove(consumer);
            UpdateConsumerInfo();
            UIManager.instance.UpdateTotalConsumers(batteryConsumers.Count);
        }
    }

    private void UpdateConsumerInfo()
    {
        UIManager.instance.ClearConsumerInfo(); // 기존 텍스트 초기화

        foreach (var consumer in batteryConsumers)
        {
            UIManager.instance.AddConsumerInfo($"{consumer.GetConsumerName()} - Battery drained: {consumer.BatteryDrainPerSecond:F2}");
        }
    }
}
