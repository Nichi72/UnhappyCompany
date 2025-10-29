using UnityEngine;
using System.Collections.Generic;
using UnhappyCompany.Utility;

/// <summary>
/// 선물상자 아이템
/// F키 상호작용 또는 인벤토리에서 사용 시 드랍 테이블에 따라 랜덤 아이템을 드랍합니다.
/// 습득(줍기)은 불가능합니다.
/// </summary>
public class ItemGiftBox : Item
{
    [Header("Gift Box Settings")]
    [Tooltip("드랍 테이블 (어떤 아이템이 나올지)")]
    [SerializeField] private GiftBoxDropTable dropTable;
    
    [Tooltip("아이템 드랍 오프셋 (선물상자 위치 기준)")]
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0.5f, 0f);
    
    [Tooltip("드랍된 아이템이 흩어지는 범위")]
    [SerializeField] private float scatterRadius = 0.5f;
    
    [Header("Effects")]
    [Tooltip("선물상자 열 때 파티클")]
    [SerializeField] private ParticleSystem openParticle;
    
    private bool isOpened = false;
    
    // 상호작용 텍스트 재정의 (F키) - new 키워드로 기존 프로퍼티 숨김
    public new string InteractionTextF => "상자 열기";
    
    // 툴팁 텍스트 오버라이드
    public override string ToolTipText => "선물상자 열기";
    public override string ToolTipText2 => isOpened ? "이미 열림" : "F키로 열기";
    
    /// <summary>
    /// F키 상호작용 - 선물상자를 바로 엽니다 (줍지 않음)
    /// </summary>
    public override void HitEventInteractionF(Player player)
    {
        if (isOpened)
        {
            Debug.Log("[ItemGiftBox] 이미 열린 선물상자입니다.");
            return;
        }
        
        Debug.Log("[ItemGiftBox] F키로 선물상자를 열었습니다!");
        OpenGiftBox(player);
        
        // 주의: PickUp()을 호출하지 않아서 인벤토리에 추가되지 않음
    }
    
    /// <summary>
    /// 인벤토리에서 사용 - 선물상자를 엽니다
    /// </summary>
    public override void Use(Player player)
    {
        if (isOpened)
        {
            Debug.Log("[ItemGiftBox] 이미 열린 선물상자입니다.");
            return;
        }
        
        Debug.Log("[ItemGiftBox] 인벤토리에서 선물상자를 사용했습니다!");
        OpenGiftBox(player);
    }
    
    /// <summary>
    /// 선물상자를 엽니다
    /// </summary>
    private void OpenGiftBox(Player player)
    {
        isOpened = true;
        
        Debug.Log("[ItemGiftBox] 선물상자를 열었습니다!");
        
        // 사운드 재생
        PlayOpenSound();
        
        // 파티클 재생
        PlayOpenEffect();
        
        // 드랍 테이블에서 아이템 선택
        if (dropTable != null)
        {
            List<(ItemData itemData, int count)> droppedItems = dropTable.RollDrop();
            
            // 선택된 아이템들을 월드에 스폰
            SpawnDroppedItems(droppedItems);
        }
        else
        {
            Debug.LogWarning("[ItemGiftBox] 드랍 테이블이 설정되지 않았습니다!");
        }
        
        // 선물상자 아이템 즉시 제거 (파티클은 독립적으로 재생되므로 영향 없음)
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 드랍된 아이템들을 월드에 스폰
    /// </summary>
    private void SpawnDroppedItems(List<(ItemData itemData, int count)> droppedItems)
    {
        if (droppedItems == null || droppedItems.Count == 0)
        {
            Debug.Log("[ItemGiftBox] 드랍된 아이템이 없습니다.");
            return;
        }
        
        Vector3 spawnPosition = transform.position + dropOffset;
        
        foreach (var (itemData, count) in droppedItems)
        {
            for (int i = 0; i < count; i++)
            {
                // 랜덤 위치 계산 (원형으로 흩어짐)
                Vector2 randomCircle = Random.insideUnitCircle * scatterRadius;
                Vector3 randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
                Vector3 finalPosition = spawnPosition + randomOffset;
                
                // 아이템 스폰
                GameObject spawnedItem = Instantiate(itemData.prefab.gameObject, finalPosition, Quaternion.identity);
                
                // 아이템에 약간의 위로 튀는 힘 추가
                Rigidbody rb = spawnedItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    Vector3 upwardForce = Vector3.up * Random.Range(2f, 4f);
                    Vector3 randomHorizontal = new Vector3(
                        Random.Range(-1f, 1f), 
                        0f, 
                        Random.Range(-1f, 1f)
                    ).normalized * Random.Range(0.5f, 1.5f);
                    
                    rb.AddForce(upwardForce + randomHorizontal, ForceMode.Impulse);
                }
                
                Debug.Log($"[ItemGiftBox] 아이템 스폰: {itemData.itemName}");
            }
        }
        
        Debug.Log($"[ItemGiftBox] 총 {droppedItems.Count}종류의 아이템이 드랍되었습니다.");
    }
    
    /// <summary>
    /// 선물상자 열기 소리 재생
    /// </summary>
    private void PlayOpenSound()
    {
        if (AudioManager.instance != null && FMODEvents.instance != null && 
            !FMODEvents.instance.giftBoxOpen.IsNull)
        {
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.giftBoxOpen, transform, 40f, "Gift Box Open");
        }
    }
    
    /// <summary>
    /// 선물상자 열기 이펙트 재생 (독립적인 파티클 인스턴스 생성)
    /// </summary>
    private void PlayOpenEffect()
    {
        if (openParticle != null)
        {
            // 범용 EffectSpawnSystem을 사용하여 독립적인 파티클 생성
            // 파티클은 재생 완료 후 자동으로 삭제됨
            GameObject particleInstance = EffectSpawnSystem.SpawnParticleAtTransform(
                particlePrefab: openParticle,
                targetTransform: transform,
                offset: Vector3.zero,
                attachToParent: false  // 부모에 붙이지 않고 독립적으로 생성
            );
            
            Debug.Log($"[ItemGiftBox] 독립 파티클 생성 완료: {particleInstance?.name} - 자동 삭제 예정");
        }
    }
    
    /// <summary>
    /// 인스펙터에서 선물상자 열기 테스트 (디버그용)
    /// </summary>
    [ContextMenu("Test Open Gift Box")]
    private void TestOpenGiftBox()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[ItemGiftBox] 플레이 모드에서만 테스트할 수 있습니다.");
            return;
        }
        
        if (isOpened)
        {
            Debug.LogWarning("[ItemGiftBox] 이미 열린 선물상자입니다.");
            return;
        }
        
        Debug.Log("[ItemGiftBox] 테스트: 선물상자를 엽니다!");
        OpenGiftBox(null);
    }
    
    /// <summary>
    /// 인스펙터에서 드랍 테이블 확률 확인 (디버그용)
    /// </summary>
    [ContextMenu("Show Drop Chances")]
    private void ShowDropChances()
    {
        if (dropTable == null)
        {
            Debug.LogWarning("드랍 테이블이 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("=== 선물상자 드랍 확률 ===");
        float total = dropTable.CalculateTotalWeight();
        
        foreach (var entry in dropTable.dropEntries)
        {
            if (entry.itemData != null)
            {
                float percentage = (entry.dropWeight / total) * 100f;
                Debug.Log($"{entry.itemData.itemName}: {percentage:F1}% (가중치: {entry.dropWeight:F3}, x{entry.minCount}~{entry.maxCount})");
            }
        }
        
        Debug.Log($"총 가중치 합계: {total:F3}");
        Debug.Log(dropTable.GetWeightStatusMessage());
    }
    
    /// <summary>
    /// 인스펙터에서 드랍 시뮬레이션 (실제 스폰 없이 로그만)
    /// </summary>
    [ContextMenu("Simulate Drop (Log Only)")]
    private void SimulateDrop()
    {
        if (dropTable == null)
        {
            Debug.LogWarning("드랍 테이블이 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("=== 드랍 시뮬레이션 시작 ===");
        List<(ItemData itemData, int count)> droppedItems = dropTable.RollDrop();
        
        if (droppedItems.Count == 0)
        {
            Debug.Log("결과: 아무것도 드랍되지 않았습니다.");
        }
        else
        {
            Debug.Log($"결과: 총 {droppedItems.Count}종류의 아이템이 선택되었습니다:");
            foreach (var (itemData, count) in droppedItems)
            {
                Debug.Log($"  - {itemData.itemName} x{count}");
            }
        }
        Debug.Log("=== 드랍 시뮬레이션 종료 ===");
    }
}

