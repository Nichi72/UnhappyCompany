using UnityEngine;

public class CentralBatteryConsumerItem : Item , ICentralBatteryConsumer
{
    [SerializeField] public float BatteryDrainPerSecond { get; set; } = 0.05f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CentralBatterySystem.Instance.RegisterConsumer(this); // 중앙 배터리 시스템에 등록
    }
    private void OnDestroy()
    {
        CentralBatterySystem.Instance.UnregisterConsumer(this); // 중앙 배터리 시스템에서 등록 해제
    }
    // Update is called once per frame
 

    public virtual void DrainBattery()
    {
        if (CentralBatterySystem.Instance != null)
        {
            CentralBatterySystem.Instance.DrainBattery(this, BatteryDrainPerSecond * Time.deltaTime);
        }

    }

    //public override void Use()
    //{
    //    //throw new System.NotImplementedException();
    //}

    public string GetConsumerName()
    {
        return itemData.itemName;
    }
}
