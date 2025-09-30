using UnityEngine;

public class Flashlight : Item, ICentralBatteryRechargeable
{
    [Header("Flashlight Settings")]
    public Light flashlightLight;
    public bool isOn = false;
    
    [Header("Battery Settings")]
    [SerializeField] private float maxBatteryCapacity = 100f;
    [SerializeField] private float currentBatteryAmount = 100f;
    [SerializeField] private float rechargeRatePerSecond = 10f;
    [SerializeField] private float batteryDrainPerSecond = 5f; // 플래시라이트 사용 시 배터리 소모량
    
    // ICentralBatteryRechargeable 구현
    public string ID => GetUniqueInstanceID();
    public float MaxBatteryCapacity { get => maxBatteryCapacity; set => maxBatteryCapacity = value; }
    public float CurrentBatteryAmount { get => currentBatteryAmount; set => currentBatteryAmount = Mathf.Clamp(value, 0f, maxBatteryCapacity); }
    public float RechargeRatePerSecond { get => rechargeRatePerSecond; set => rechargeRatePerSecond = value; }
    public bool IsFullyCharged => currentBatteryAmount >= maxBatteryCapacity;
    
    void Start()
    {
        // Flashlight Light 컴포넌트가 없으면 자동으로 추가
        if (flashlightLight == null)
        {
            flashlightLight = GetComponent<Light>();
            if (flashlightLight == null)
            {
                flashlightLight = gameObject.AddComponent<Light>();
            }
        }
        
        // 초기 상태 설정
        SetFlashlightState(isOn);
    }
    
    void Update()
    {
        // 플래시라이트가 켜져있으면 배터리 소모
        if (isOn)
        {
            DrainBattery(batteryDrainPerSecond * Time.deltaTime);
        }
    }

    public override void Use(Player player)
    {
        // 배터리가 있으면 Flashlight 토글 기능
        if (currentBatteryAmount > 0)
        {
            ToggleFlashlight();
        }
        else
        {
            Debug.Log("Flashlight battery is empty!");
        }
    }
    
    private void ToggleFlashlight()
    {
        isOn = !isOn;
        SetFlashlightState(isOn);
        
        Debug.Log($"Flashlight turned {(isOn ? "ON" : "OFF")}");
    }
    
    private void SetFlashlightState(bool state)
    {
        if (flashlightLight != null)
        {
            flashlightLight.enabled = state;
        }
        
        // 배터리가 없으면 자동으로 끄기
        if (state && currentBatteryAmount <= 0)
        {
            isOn = false;
            flashlightLight.enabled = false;
        }
    }
    
    // ICentralBatteryRechargeable 메서드 구현
    public void RechargeFromCentralBattery()
    {
        if (!IsFullyCharged)
        {
            float rechargeAmount = rechargeRatePerSecond * Time.deltaTime;
            CurrentBatteryAmount += rechargeAmount;
            
            Debug.Log($"Flashlight recharging... Current battery: {currentBatteryAmount:F1}/{maxBatteryCapacity}");
        }
    }
    
    public void DrainBattery(float amount)
    {
        CurrentBatteryAmount -= amount;
        
        // 배터리가 0이 되면 자동으로 끄기
        if (currentBatteryAmount <= 0 && isOn)
        {
            isOn = false;
            SetFlashlightState(false);
            Debug.Log("Flashlight turned off due to low battery!");
        }
    }
    
    public string GetItemName()
    {
        return itemData != null ? itemData.itemName : "Flashlight";
    }
    
    // 상태 저장/로드 기능 (배터리 상태도 포함)
    public override object SerializeState()
    {
        return this.isOn;

        // return new FlashlightState 
        // { 
        //     isOn = this.isOn, 
        //     currentBatteryAmount = this.currentBatteryAmount 
        // };
    }
    
    public override void DeserializeState(object state)
    {
        // if (state is FlashlightState savedState)
        // {
        //     isOn = savedState.isOn;
        //     currentBatteryAmount = savedState.currentBatteryAmount;
        //     SetFlashlightState(isOn);
        // }
        // else if (state is bool savedIsOn) // 기존 호환성을 위해
        // {
        //     isOn = savedIsOn;
        //     SetFlashlightState(isOn);
        // }
    }
    
    // 배터리 상태를 위한 구조체
    [System.Serializable]
    private struct FlashlightState
    {
        public bool isOn;
        public float currentBatteryAmount;
    }
}
