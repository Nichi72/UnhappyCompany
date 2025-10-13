using UnityEngine;

/// <summary>
/// CentralBatterySystem의 currentBatteryLevel을 충전하는 아이템
/// </summary>
public class BatteryChargeItem : Item
{
    [Header("Charge Settings")]
    [Tooltip("이 아이템이 충전하는 배터리 양")]
    public float chargeAmount = 100f;

    [Header("Visual Settings")]
    [SerializeField] private ParticleSystem chargeEffect; // 충전 효과 (옵션)

    public override string ToolTipText2 
    { 
        get => $"충전량: {chargeAmount}"; 
        set => ToolTipText2 = value; 
    }

    private void Start()
    {
        // 충전 효과가 있다면 비활성화 상태로 시작
        if (chargeEffect != null)
        {
            chargeEffect.Stop();
        }
    }

    public override void Use(Player player)
    {
        // 이 아이템은 BatteryChargeSlot과의 상호작용으로만 사용됨
        Debug.Log("[BatteryChargeItem] 이 아이템은 배터리 충전 슬롯에서만 사용할 수 있습니다.");
    }

    public override void Mount(Player player, object state = null)
    {
        base.Mount(player, state);
        Debug.Log($"[BatteryChargeItem] 배터리 충전 아이템 장착 (충전량: {chargeAmount})");
    }

    public override void UnMount()
    {
        base.UnMount();
        Debug.Log("[BatteryChargeItem] 배터리 충전 아이템 해제");
    }

    // 상태 저장 (충전량 정보 포함)
    public override object SerializeState()
    {
        return new BatteryChargeItemState
        {
            chargeAmount = this.chargeAmount
        };
    }

    // 상태 복원
    public override void DeserializeState(object state)
    {
        if (state is BatteryChargeItemState savedState)
        {
            this.chargeAmount = savedState.chargeAmount;
        }
    }

    // 충전 효과 재생 (옵션)
    public void PlayChargeEffect()
    {
        if (chargeEffect != null)
        {
            chargeEffect.Play();
        }
    }

    [System.Serializable]
    private struct BatteryChargeItemState
    {
        public float chargeAmount;
    }

    #region Editor Helpers
    [ContextMenu("Set Default Values")]
    private void SetDefaultValues()
    {
        chargeAmount = 100f;
        Debug.Log($"[BatteryChargeItem] 기본값 설정 완료 - 충전량: {chargeAmount}");
    }
    #endregion
}

