using UnityEngine;

/// <summary>
/// 중앙 배터리로 충전 가능한 아이템의 기본 구현을 제공하는 추상 클래스
/// </summary>
public abstract class RechargeableItem : Item, ICentralBatteryRechargeable
{
    [Header("Battery Settings")]
    [SerializeField] protected float maxBatteryCapacity = 100f;
    [SerializeField] protected float currentBatteryAmount = 100f;
    [SerializeField] protected float rechargeRatePerSecond = 10f;

    // ICentralBatteryRechargeable 구현
    public string ID => GetUniqueInstanceID();
    public float MaxBatteryCapacity { get => maxBatteryCapacity; set => maxBatteryCapacity = value; }
    public float CurrentBatteryAmount 
    { 
        get => currentBatteryAmount; 
        set => currentBatteryAmount = Mathf.Clamp(value, 0f, maxBatteryCapacity); 
    }
    public float RechargeRatePerSecond { get => rechargeRatePerSecond; set => rechargeRatePerSecond = value; }
    public bool IsFullyCharged => currentBatteryAmount >= maxBatteryCapacity;

    /// <summary>
    /// 중앙 배터리로부터 충전을 시도합니다. (공통 구현)
    /// 특별한 충전 로직이 필요하면 override 가능합니다.
    /// </summary>
    public virtual ChargeResult TryChargeFromCentralBattery()
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

    /// <summary>
    /// 배터리를 소모합니다.
    /// </summary>
    public virtual void DrainBattery(float amount)
    {
        CurrentBatteryAmount -= amount;
    }

    public virtual string GetItemName()
    {
        return itemData != null ? itemData.itemName : GetType().Name;
    }

    public override void Mount(Player player, object state = null)
    {
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

        base.UnMount();
    }
}

