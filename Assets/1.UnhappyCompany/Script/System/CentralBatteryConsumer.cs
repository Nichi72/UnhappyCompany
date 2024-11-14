using UnityEngine;
public class CentralBatteryConsumer : MonoBehaviour, ICentralBatteryConsumer
{
    // 1�ʴ� �Ҹ�Ǵ� ���͸� ��
    [Tooltip("This sets the amount of battery consumed per second.")]
    [SerializeField] public float BatteryDrainPerSecond { get; set; } = 0.05f;

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

    // MonoBehaviour Update �޼���
    void Update()
    {
        DrainBattery(); // �� �����Ӹ��� ���͸� �Ҹ�
    }

    // �Һ����� �̸��� ��ȯ�ϴ� �޼���
    public virtual string GetConsumerName()
    {
        return gameObject.name;
    }
}
