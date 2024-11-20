using UnityEngine;

public class CentralBatteryConsumer : MonoBehaviour, ICentralBatteryConsumer
{
    // ���͸� �Ҹ��� �����ϴ� ��ŷ �ʵ�
    [Tooltip("This sets the amount of battery consumed per second.")]
    protected float batteryDrainPerSecond = 0.05f;

    // ���͸� �Ҹ� ������Ƽ
    public virtual float BatteryDrainPerSecond
    {
        get => batteryDrainPerSecond;
        set => batteryDrainPerSecond = value;
    }

    void Start()
    {
        CentralBatterySystem.Instance.RegisterConsumer(this); // �߾� ���͸� �ý��ۿ� ���
    }

    void OnDestroy()
    {
        CentralBatterySystem.Instance.UnregisterConsumer(this); // �߾� ���͸� �ý��ۿ��� ��� ����
    }

    // ���͸� �Ҹ� �޼���
    public virtual void DrainBattery()
    {
        if (CentralBatterySystem.Instance != null)
        {
            CentralBatterySystem.Instance.DrainBattery(this, BatteryDrainPerSecond * Time.deltaTime);
        }
    }

    void Update()
    {
        DrainBattery(); // �� �����Ӹ��� ���͸� �Ҹ�
    }

    public virtual string GetConsumerName()
    {
        return gameObject.name;
    }
}
