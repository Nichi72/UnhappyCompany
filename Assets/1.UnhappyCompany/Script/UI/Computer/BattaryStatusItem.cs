using TMPro;
using UnityEngine;

public class BattaryStatusItem : MonoBehaviour
{
    
    public TMP_Text batteryDrainPerSecond;
    public TMP_Text nameText;
    public ICentralBatteryConsumer batteryConsumer;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Init(ICentralBatteryConsumer batteryConsumer)
    {
        this.batteryConsumer = batteryConsumer;
        batteryDrainPerSecond.text = batteryConsumer.BatteryDrainPerSecond.ToString();
        nameText.text = batteryConsumer.GetConsumerName();
    }
}
