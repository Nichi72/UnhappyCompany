using UnityEngine;
/// <summary>
/// �޴�� ���͸� ����
/// </summary>
public abstract class BatteryConsumerItem : MonoBehaviour
{
    // ���͸� �ѷ�
    protected float batteryLTotalAmount = 100.0f;

    /// 1�ʴ� �Ҹ�Ǵ� ���͸� ��
    protected abstract float BatteryDrainPerSecond { get; }

    // ���͸� �Ҹ� �޼��� (�� ���� Ŭ�������� �ʴ� ���͸� �Ҹ� ó��)
    protected virtual void DrainBattery()
    {
        batteryLTotalAmount -= BatteryDrainPerSecond * Time.deltaTime;
        batteryLTotalAmount = Mathf.Max(batteryLTotalAmount, 0); // ���͸� �ܷ��� ������ ���� �ʵ��� ����
        Debug.Log($"Battery Level: {batteryLTotalAmount}");
    }

    // MonoBehaviour Update �޼���
    void Update()
    {
        if (batteryLTotalAmount > 0)
        {
            DrainBattery(); // �� �����Ӹ��� ���͸� �Ҹ�
        }
    }
}
