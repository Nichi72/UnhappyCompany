using UnityEngine;

public class Flashlight : Item
{
    [Header("Flashlight Settings")]
    public Light flashlightLight;
    public bool isOn = false;
    
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

    public override void Use(Player player)
    {
        // Flashlight 토글 기능
        ToggleFlashlight();
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
    }
    
    // 상태 저장/로드 기능
    public override object SerializeState()
    {
        return isOn;
    }
    
    public override void DeserializeState(object state)
    {
        if (state is bool savedState)
        {
            isOn = savedState;
            SetFlashlightState(isOn);
        }
    }
}
