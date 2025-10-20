using UnityEngine;

public class Flashlight : RechargeableItem
{
    [Header("Flashlight Settings")]
    public Light flashlightLight;
    public bool isOn = false;

    [Header("Flashlight Battery Drain")]
    [SerializeField] private float batteryDrainPerSecond = 5f; // 플래시라이트 사용 시 배터리 소모량

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

    // DrainBattery override - 플래시라이트 특화 로직 (배터리 없으면 자동 꺼짐)
    public override void DrainBattery(float amount)
    {
        base.DrainBattery(amount);

        // 배터리가 0이 되면 자동으로 끄기
        if (currentBatteryAmount <= 0 && isOn)
        {
            isOn = false;
            SetFlashlightState(false);
            Debug.Log("[FlahLight] 배터리 없어서 손전등꺼짐");
        }
    }

    // 상태 저장/로드 기능 (배터리 상태도 포함)
    public override object SerializeState()
    {
        return new FlashlightState 
        { 
            isOn = this.isOn, 
            currentBatteryAmount = this.currentBatteryAmount 
        };
    }

    public override void DeserializeState(object state)
    {
        if (state is FlashlightState savedState)
        {
            isOn = savedState.isOn;
            currentBatteryAmount = savedState.currentBatteryAmount;
            Debug.Log($"[FlashLight]: 현재 배터리 잔량: {currentBatteryAmount}");
            SetFlashlightState(isOn);
        }
        else if (state is bool savedIsOn) // 기존 호환성을 위해
        {
            isOn = savedIsOn;
            SetFlashlightState(isOn);
        }
    }

    // 배터리 상태를 위한 구조체
    [System.Serializable]
    private struct FlashlightState
    {
        public bool isOn;
        public float currentBatteryAmount;
    }
}
