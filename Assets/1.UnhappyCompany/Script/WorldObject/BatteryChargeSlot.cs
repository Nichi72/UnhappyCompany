using UnityEngine;
using MyUtility;

/// <summary>
/// CenterBattery의 특정 영역에 부착하여 배터리 충전 아이템을 삽입할 수 있는 슬롯
/// </summary>
public class BatteryChargeSlot : MonoBehaviour, IInteractableF, IToolTip
{
    [Header("Interaction Settings")]
    public string InteractionTextF { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "BatteryChargeSlot_ITR"); set => InteractionTextF = value; }
    public bool IgnoreInteractionF { get; set; } = false;
    
    [Header("ToolTip Settings")]
    public string ToolTipText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "BatteryChargeSlot_TT"); set => ToolTipText = value; }
    public string ToolTipText2 { get => ""; set => ToolTipText2 = value; }
    public string ToolTipText3 { get => ""; set => ToolTipText3 = value; }

    [Header("Visual Feedback")]
    [SerializeField] private Renderer slotRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.green;
    
    private Material slotMaterial;

    void Start()
    {
        // 시각적 피드백을 위한 머티리얼 설정 (옵션)
        if (slotRenderer != null)
        {
            slotMaterial = slotRenderer.material;
        }
    }

    public void HitEventInteractionF(Player player)
    {
        // 플레이어가 현재 손에 들고있는 아이템 확인
        GameObject currentItemObject = player.quickSlotSystem.currentItemObject;
        
        if (currentItemObject == null)
        {
            Debug.Log("[BatteryChargeSlot] 손에 아이템이 없습니다.");
            return;
        }

        // 손에 들고있는 아이템이 BatteryChargeItem인지 확인
        BatteryChargeItem chargeItem = currentItemObject.GetComponent<BatteryChargeItem>();
        
        if (chargeItem == null)
        {
            Debug.Log("[BatteryChargeSlot] 배터리 충전 아이템이 아닙니다.");
            return;
        }

        // CentralBatterySystem에 충전
        if (CentralBatterySystem.Instance == null)
        {
            Debug.LogError("[BatteryChargeSlot] CentralBatterySystem이 존재하지 않습니다.");
            return;
        }

        // 최대 배터리 레벨 확인
        if (CentralBatterySystem.Instance.currentBatteryLevel >= CentralBatterySystem.Instance.MaxBatteryLevel)
        {
            Debug.Log("[BatteryChargeSlot] 중앙 배터리가 이미 가득 찼습니다.");
            return;
        }

        // 배터리 충전
        float chargeAmount = chargeItem.chargeAmount;
        float beforeCharge = CentralBatterySystem.Instance.currentBatteryLevel;
        CentralBatterySystem.Instance.currentBatteryLevel = Mathf.Min(
            CentralBatterySystem.Instance.currentBatteryLevel + chargeAmount,
            CentralBatterySystem.Instance.MaxBatteryLevel
        );
        float actualCharged = CentralBatterySystem.Instance.currentBatteryLevel - beforeCharge;

        Debug.Log($"[BatteryChargeSlot] 배터리 충전 완료! +{actualCharged} (현재: {CentralBatterySystem.Instance.currentBatteryLevel}/{CentralBatterySystem.Instance.MaxBatteryLevel})");

        // UI 업데이트
        UIManager.instance.UpdateTotalBatteryLevel(CentralBatterySystem.Instance.currentBatteryLevel);

        // 손에 들고있던 충전 아이템 제거
        // DestroyCurrentItem()은 버그가 있어서 직접 처리
        GameObject itemToDestroy = player.quickSlotSystem.currentItemObject;
        player.quickSlotSystem.ClearCurrentItemSlot();
        
        if (itemToDestroy != null)
        {
            Destroy(itemToDestroy);
        }
    }

    // 시각적 피드백 (옵션)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && slotMaterial != null)
        {
            slotMaterial.color = highlightColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && slotMaterial != null)
        {
            slotMaterial.color = normalColor;
        }
    }
}

