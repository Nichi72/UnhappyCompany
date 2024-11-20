using UnityEngine;

public class CCTV : CentralBatteryConsumerItem
{
    public Camera currentCamera;
    public bool isTurnOn = true;

    //public float BatteryDrainPerSecond { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    void Start()
    {
        CCTVManager.instance.cctvs.Add(this);
    }

    private void OnDestroy()
    {
        CCTVManager.instance.cctvs.Remove(this);
        CentralBatterySystem.Instance.UnregisterConsumer(this); // �߾� ���͸� �ý��ۿ��� ��� ����
    }
    

    // ���͸� �Ҹ� �޼��� (�⺻ ������ �����ϰų� ���� ����)
    public override void DrainBattery()
    {
        if(isTurnOn == true)
        {
            base.DrainBattery(); // �⺻ ���͸� �Ҹ� ��� ���
                                 // �߰����� �ൿ�� �ʿ��ϴٸ� ���⿡ �ۼ�
        }
    }

    public override void Use()
    {
        BuildSystem.instance.StartPlacing(itemData.prefab.gameObject); // ��ġ ��� ����
    }
}
