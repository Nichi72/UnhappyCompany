using UnityEngine;
using System.Collections.Generic;

public class CentralBatterySystem : MonoBehaviour
{
    public static CentralBatterySystem Instance; // �̱��� �������� �߾� ���͸� �ý��� ����

    public float totalBatteryLevel = 1000.0f; // �߾� ���͸��� �ѷ�
    [SerializeField]
    private List<ICentralBatteryConsumer> batteryConsumers = new List<ICentralBatteryConsumer>();

    void Awake()
    {
        // �̱��� �ν��Ͻ� ����
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

    // �߾� ���͸����� ������ �Һ��ϴ� �޼���
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

    // �Һ��ڸ� �߰��ϴ� �޼���
    public void RegisterConsumer(ICentralBatteryConsumer consumer)
    {
        if (!batteryConsumers.Contains(consumer))
        {
            batteryConsumers.Add(consumer);
            UpdateConsumerInfo();
            UIManager.Instance.UpdateTotalConsumers(batteryConsumers.Count);
        }
    }

    // �Һ��ڸ� �����ϴ� �޼��� (��ü�� �ı��� �� ���)
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
        UIManager.Instance.ClearConsumerInfo(); // ���� �ؽ�Ʈ �ʱ�ȭ

        foreach (var consumer in batteryConsumers)
        {
            UIManager.Instance.AddConsumerInfo($"{consumer.GetConsumerName()} - Battery drained: {consumer.BatteryDrainPerSecond:F2}");
        }
    }
}
