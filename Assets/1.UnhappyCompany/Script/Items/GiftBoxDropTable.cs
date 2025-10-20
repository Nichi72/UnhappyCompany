using UnityEngine;
using System.Collections.Generic;
using UnhappyCompany.Utility;



/// <summary>
/// 아이템 드랍 엔트리 (개수 정보 포함)
/// </summary>
[System.Serializable]
public class ItemDropEntry
{
    [Tooltip("드랍될 아이템 데이터")]
    public ItemData itemData;
    
    [Tooltip("드랍 가중치 (0~1, 총합이 1을 넘지 않도록 주의)")]
    [Range(0f, 1f)]
    public float dropWeight = 0.1f;
    
    [Tooltip("최소 드랍 개수")]
    [Min(1)]
    public int minCount = 1;
    
    [Tooltip("최대 드랍 개수")]
    [Min(1)]
    public int maxCount = 1;
}

/// <summary>
/// 선물상자 드랍 테이블 (범용 확률 시스템 사용)
/// </summary>
[CreateAssetMenu(fileName = "GiftBoxDropTable", menuName = "UnhappyCompany/Items/Gift Box Drop Table")]
public class GiftBoxDropTable : ScriptableObject
{
    [Header("Drop Settings")]
    [Tooltip("선물상자를 열 때 드랍될 아이템 리스트")]
    public List<ItemDropEntry> dropEntries = new List<ItemDropEntry>();
    
    [Tooltip("한 번에 드랍될 최대 아이템 종류 수")]
    [Min(1)]
    public int maxDropTypes = 1;
    
    [Header("Drop Table Info")]
    [Tooltip("현재 설정된 총 드랍 가중치 (자동 계산, 1.0 이하 권장)")]
    [SerializeField] private float totalWeight = 0f;
    
    [Tooltip("가중치 상태 메시지")]
    [TextArea(2, 3)]
    [SerializeField] private string weightStatus = "";
    
    [Header("Info")]
    [TextArea(3, 5)]
    public string description = "선물상자에서 드랍될 아이템과 가중치를 설정합니다.\n범용 가중치 기반 확률 시스템을 사용합니다.";
    
    // 내부 확률 선택 시스템
    private WeightedRandomSystem<ItemDropEntry> selector;
    
    /// <summary>
    /// 확률 선택기 초기화 (매번 최신 가중치 반영)
    /// </summary>
    private void InitializeSelector()
    {
        // 항상 최신 dropWeight 값을 반영하기 위해 매번 재생성
        var weightedEntries = new List<WeightedEntry<ItemDropEntry>>();
        
        foreach (var dropEntry in dropEntries)
        {
            if (dropEntry.itemData != null)
            {
                weightedEntries.Add(new WeightedEntry<ItemDropEntry>(dropEntry, dropEntry.dropWeight));
            }
        }
        
        selector = new WeightedRandomSystem<ItemDropEntry>(weightedEntries);
    }
    
    /// <summary>
    /// 드랍 테이블에서 가중치 기반으로 랜덤 아이템 선택 (범용 확률 시스템 사용)
    /// </summary>
    /// <returns>선택된 아이템 데이터와 개수 리스트</returns>
    public List<(ItemData itemData, int count)> RollDrop()
    {
        List<(ItemData, int)> droppedItems = new List<(ItemData, int)>();
        
        if (dropEntries == null || dropEntries.Count == 0)
        {
            Debug.LogWarning("[GiftBoxDropTable] 드랍 테이블이 비어있습니다!");
            return droppedItems;
        }
        
        // 선택기 초기화
        InitializeSelector();
        
        // maxDropTypes만큼 반복
        var selectedEntries = selector.SelectMultiple(maxDropTypes, entry => entry.itemData != null);
        
        for (int i = 0; i < selectedEntries.Count; i++)
        {
            var selectedEntry = selectedEntries[i];
            
            if (selectedEntry != null && selectedEntry.itemData != null)
            {
                // 드랍 개수 결정
                int count = Random.Range(selectedEntry.minCount, selectedEntry.maxCount + 1);
                droppedItems.Add((selectedEntry.itemData, count));
                
                Debug.Log($"[GiftBoxDropTable] 드랍 성공 #{i+1}: {selectedEntry.itemData.itemName} x{count} " +
                         $"(가중치: {selectedEntry.dropWeight:F2})");
            }
        }
        
        // 아무것도 안 나왔으면 로그
        if (droppedItems.Count == 0)
        {
            Debug.Log("[GiftBoxDropTable] 이번에는 아무것도 드랍되지 않았습니다.");
        }
        
        return droppedItems;
    }
    
    /// <summary>
    /// 총 드랍 가중치 계산
    /// </summary>
    public float CalculateTotalWeight()
    {
        float total = 0f;
        foreach (var entry in dropEntries)
        {
            if (entry.itemData != null)
                total += entry.dropWeight;
        }
        return total;
    }
    
    /// <summary>
    /// 각 아이템의 실제 드랍 확률 가져오기
    /// </summary>
    public Dictionary<ItemData, float> GetDropChances()
    {
        Dictionary<ItemData, float> chances = new Dictionary<ItemData, float>();
        
        float totalWeight = CalculateTotalWeight();
        if (totalWeight <= 0f)
            return chances;

        foreach (var entry in dropEntries)
        {
            if (entry.itemData != null)
            {
                float percentage = (entry.dropWeight / totalWeight) * 100f;
                chances[entry.itemData] = percentage;
            }
        }

        return chances;
    }
    
    /// <summary>
    /// 가중치 상태 메시지 가져오기
    /// </summary>
    public string GetWeightStatusMessage()
    {
        return weightStatus;
    }
    
    /// <summary>
    /// 인스펙터에서 값이 변경될 때마다 호출 (에디터 전용)
    /// </summary>
    private void OnValidate()
    {
        UpdateWeightInfo();
        // InitializeSelector()가 매번 재생성하므로 별도 처리 불필요
    }
    
    /// <summary>
    /// 가중치 정보 업데이트
    /// </summary>
    private void UpdateWeightInfo()
    {
        totalWeight = CalculateTotalWeight();
        
        if (totalWeight <= 0f)
        {
            weightStatus = "⚠️ 경고: 가중치가 설정되지 않았습니다!";
        }
        else if (totalWeight > 1f)
        {
            weightStatus = $"⚠️ 경고: 총 가중치가 1.0을 초과합니다! ({totalWeight:F3})\n" +
                          $"정규화되어 처리됩니다.";
        }
        else if (totalWeight < 1f)
        {
            float nothingChance = (1f - totalWeight) * 100f;
            weightStatus = $"✅ 정상 (총 가중치: {totalWeight:F3})\n" +
                          $"아무것도 안 나올 확률: {nothingChance:F1}%";
        }
        else
        {
            weightStatus = $"✅ 완벽! (총 가중치: {totalWeight:F3})\n" +
                          "항상 아이템이 드랍됩니다.";
        }
    }
}

