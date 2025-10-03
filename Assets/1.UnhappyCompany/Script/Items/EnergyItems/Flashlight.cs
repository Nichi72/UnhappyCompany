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
        if (currentBatteryAmount <= 0f)
        {
            Debug.Log("[FlahLight] 손전등의 배터리가 없습니다.");
            return;
        }

        ToggleFlashlight();
    }

    private void ToggleFlashlight()
    {
        isOn = !isOn;
        SetFlashlightState(isOn);
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
    public ChargeResult TryChargeFromCentralBattery()
    {
        if (CentralBatterySystem.Instance == null)
        {
            return ChargeResult.SystemUnavailable;
        }

        // 내가 필요한 만큼 계산 (완전 충전까지)
        float neededAmount = maxBatteryCapacity - currentBatteryAmount;
        if (neededAmount <= 0f)
        {
            return ChargeResult.AlreadyFull;
        }

        // 중앙 배터리에서 전력 요청
        float receivedPower = CentralBatterySystem.Instance.RequestPower(neededAmount);
        if (receivedPower <= 0)
        {
            return ChargeResult.CentralBatteryEmpty;
        }

        CurrentBatteryAmount += receivedPower;
        return ChargeResult.Success;
    }

    public void DrainBattery(float amount)
    {
        CurrentBatteryAmount -= amount;

        // 배터리가 0이 되면 자동으로 끄기
        if (currentBatteryAmount <= 0 && isOn)
        {
            isOn = false;
            SetFlashlightState(false);
            Debug.Log("[FlahLight] 배터리 없어서 손전등꺼짐");
        }
    }

    public string GetItemName()
    {
        return itemData != null ? itemData.itemName : "Flashlight";
    }

    public override void Mount(Player player, object state = null)
    {
        // 부모 클래스의 Mount 메서드 호출
        base.Mount(player, state);

        // 중앙 배터리 시스템에 충전 가능한 아이템으로 등록
        if (CentralBatterySystem.Instance != null)
        {
            CentralBatterySystem.Instance.RegisterRechargeable(this);
        }
    }

    public override void UnMount()
    {
        // 중앙 배터리 시스템에서 등록 해제
        if (CentralBatterySystem.Instance != null)
        {
            CentralBatterySystem.Instance.UnregisterRechargeable(this);
        }

        // 부모 클래스의 UnMount 메서드 호출
        base.UnMount();
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
