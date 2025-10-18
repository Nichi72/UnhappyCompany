using UnityEngine;
using System.Collections.Generic;

public class CentralBatterySystem : MonoBehaviour
{
    public static CentralBatterySystem Instance; // 싱글톤 패턴으로 중앙 배터리 시스템 관리
    public float MaxBatteryLevel = 1000.0f; // 중앙 배터리의 총량
    [ReadOnly] public float currentBatteryLevel = 1000.0f; // 중앙 배터리의 총량
    [SerializeField]
    private Dictionary<string, ICentralBatteryConsumer> batteryConsumers = new Dictionary<string, ICentralBatteryConsumer>();
    
    [SerializeField]
    private Dictionary<string, ICentralBatteryRechargeable> rechargeableItems = new Dictionary<string, ICentralBatteryRechargeable>();

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

    public float CalculateTotalBatteryDrainPerSecond()
    {
        float total = 0;
        foreach(var consumer in batteryConsumers.Values)
        {
            total += consumer.BatteryDrainPerSecond;
        }
        return total;
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
        if (!batteryConsumers.ContainsValue(consumer))
        {
            batteryConsumers.Add(consumer.ID, consumer);
            UIManager.instance.UpdateTotalConsumers(batteryConsumers.Count);
            UIManager.instance.InitBatteryStatusItem(consumer);
        }
    }

    // 소비자를 제거하는 메서드 (객체가 파괴될 때 사용)
    public void UnregisterConsumer(ICentralBatteryConsumer consumer)
    {
        if (batteryConsumers.ContainsValue(consumer))
        {
            batteryConsumers.Remove(consumer.ID);
            UIManager.instance.UpdateTotalConsumers(batteryConsumers.Count);
            UIManager.instance.RemoveBatteryStatusItem(consumer);
        }
    }

    // ID로 소비자 찾기
    public ICentralBatteryConsumer GetConsumerByID(string id)
    {
        if (batteryConsumers.TryGetValue(id, out ICentralBatteryConsumer consumer))
        {
            return consumer;
        }
        return null;
    }
    
    // 충전 가능한 아이템을 등록하는 메서드
    public void RegisterRechargeable(ICentralBatteryRechargeable rechargeableItem)
    {
        if (!rechargeableItems.ContainsValue(rechargeableItem))
        {
            rechargeableItems.Add(rechargeableItem.ID, rechargeableItem);
        }
    }

    // 충전 가능한 아이템을 제거하는 메서드
    public void UnregisterRechargeable(ICentralBatteryRechargeable rechargeableItem)
    {
        if (rechargeableItems.ContainsValue(rechargeableItem))
        {
            rechargeableItems.Remove(rechargeableItem.ID);
        }
    }

    // 전력을 요청하는 메서드 (실제 공급 가능한 양을 반환)
    public float RequestPower(float requestedAmount)
    {
        Debug.Log($"[CenterBattry, 충전 요구] 현재 남은 배터리 양: {currentBatteryLevel}");

        if (isStop || currentBatteryLevel <= 0 || requestedAmount <= 0)
        {
            return 0f;
        }

        float actualAmount = Mathf.Min(requestedAmount, currentBatteryLevel);
        currentBatteryLevel -= actualAmount;

        Debug.Log($"[CenterBattry] 센터 배터리를 통해 아이템이 충전될 배터리 양: {actualAmount}");
        Debug.Log($"[CenterBattry, 충전 후] 현재 남은 배터리 양: {currentBatteryLevel}");


        UIManager.instance.UpdateTotalBatteryLevel(currentBatteryLevel);
        return actualAmount;
    }

    [ContextMenu("DebugBatteryConsumers")]
    public void DebugBatteryConsumers()
    {
        Debug.Log($"===== 배터리 소비자 목록 (총 {batteryConsumers.Count}개) =====");
        
        foreach (var pair in batteryConsumers)
        {
            string id = pair.Key;
            ICentralBatteryConsumer consumer = pair.Value;
            
            Debug.Log($"ID: {id} | 이름: {consumer.GetConsumerName()} | 소모량: {consumer.BatteryDrainPerSecond}/초 | 타입: {consumer.GetType().Name}");
        }
        
        Debug.Log("=======================================");
    }

    [ContextMenu("DebugRechargeableItems")]
    public void DebugRechargeableItems()
    {
        Debug.Log($"===== 충전 가능한 아이템 목록 (총 {rechargeableItems.Count}개) =====");
        
        foreach (var pair in rechargeableItems)
        {
            string id = pair.Key;
            ICentralBatteryRechargeable rechargeableItem = pair.Value;
            
            Debug.Log($"ID: {id} | 이름: {rechargeableItem.GetItemName()} | " +
                     $"배터리: {rechargeableItem.CurrentBatteryAmount:F1}/{rechargeableItem.MaxBatteryCapacity:F1} " +
                     $"({(rechargeableItem.CurrentBatteryAmount/rechargeableItem.MaxBatteryCapacity*100):F1}%) | " +
                     $"충전속도: {rechargeableItem.RechargeRatePerSecond:F1}/초 | " +
                     $"완전충전: {rechargeableItem.IsFullyCharged}");
        }
        
        Debug.Log("=======================================");
    }

    [ContextMenu("TestChargeAllItems")]
    public void TestChargeAllItems()
    {
        Debug.Log($"===== 모든 아이템 충전 테스트 =====");
        
        foreach (var pair in rechargeableItems)
        {
            ICentralBatteryRechargeable rechargeableItem = pair.Value;
            
            var result = rechargeableItem.TryChargeFromCentralBattery();
            Debug.Log($"{rechargeableItem.GetItemName()} 충전 결과: {result}");
        }
        
        Debug.Log("=======================================");
    }
}
