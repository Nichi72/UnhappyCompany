// �߾� ���͸��� ����ϴ� ��ü���� Ŭ���� 1: ��ġ�� ����
using UnityEngine;

public class CentralLight : CentralBatteryConsumer
{
    //// �ʴ� �Ҹ�Ǵ� ���͸� ��
    //protected override float BatteryDrainPerSecond => 10.0f;

    // ���͸� �Ҹ� �޼��� (�⺻ ������ �����ϰų� ���� ����)
    public override void DrainBattery()
    {
        base.DrainBattery(); // �⺻ ���͸� �Ҹ� ��� ���
        // �߰����� �ൿ�� �ʿ��ϴٸ� ���⿡ �ۼ�
    }
}