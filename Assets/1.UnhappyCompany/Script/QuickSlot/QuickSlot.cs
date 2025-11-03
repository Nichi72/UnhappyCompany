using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용

public class QuickSlot : MonoBehaviour
{
    private int slotIndex;
    public QuickSlotState quickSlotState;
    public Image icon;
    public TextMeshProUGUI countText; // 아이템 개수 표시용 텍스트 

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public void SetItem(Item newItem , object itemSerializedState , string uniqueInstanceID, int count = 1)
    {
        quickSlotState.item = newItem;
        icon.sprite = newItem.itemData.icon;
        icon.enabled = true;
        quickSlotState.itemSerializedState = itemSerializedState;
        quickSlotState.uniqueItemID = uniqueInstanceID;
        quickSlotState.itemCount = count;
        UpdateCountDisplay();
        
        Debug.Log($"<color=green>[QuickSlot]</color> 슬롯 {slotIndex}: <b>{newItem.itemData.itemName}</b> x{count}개 설정됨");
    }
    
    /// <summary>
    /// 아이템 개수를 증가시킵니다. 최대 스택 수를 초과하면 초과된 개수를 반환합니다.
    /// </summary>
    /// <param name="amount">증가시킬 개수</param>
    /// <returns>최대 스택을 초과한 개수 (0이면 모두 추가됨)</returns>
    public int AddCount(int amount)
    {
        if (quickSlotState.item == null) return amount;
        
        int maxStack = (int)quickSlotState.item.itemData.stackType;
        int oldCount = quickSlotState.itemCount;
        int newCount = quickSlotState.itemCount + amount;
        
        if (newCount <= maxStack)
        {
            quickSlotState.itemCount = newCount;
            UpdateCountDisplay();
            Debug.Log($"<color=green>[QuickSlot]</color> 슬롯 {slotIndex}: <b>{quickSlotState.item.itemData.itemName}</b> {oldCount}개 → {newCount}개 (추가: +{amount})");
            return 0; // 모두 추가됨
        }
        else
        {
            int overflow = newCount - maxStack;
            quickSlotState.itemCount = maxStack;
            UpdateCountDisplay();
            Debug.Log($"<color=green>[QuickSlot]</color> 슬롯 {slotIndex}: <b>{quickSlotState.item.itemData.itemName}</b> {oldCount}개 → {maxStack}개 (최대치 도달, 초과: {overflow}개)");
            return overflow; // 초과된 개수 반환
        }
    }
    
    /// <summary>
    /// 아이템 개수를 감소시킵니다.
    /// </summary>
    /// <param name="amount">감소시킬 개수</param>
    /// <returns>true면 아이템이 모두 소모됨, false면 아직 남음</returns>
    public bool RemoveCount(int amount = 1)
    {
        int oldCount = quickSlotState.itemCount;
        quickSlotState.itemCount -= amount;
        
        if (quickSlotState.itemCount <= 0)
        {
            Debug.Log($"<color=green>[QuickSlot]</color> 슬롯 {slotIndex}: <b>{quickSlotState.item.itemData.itemName}</b> {oldCount}개 → 0개 (모두 소모됨)");
            RemoveItem();
            return true;
        }
        
        UpdateCountDisplay();
        Debug.Log($"<color=green>[QuickSlot]</color> 슬롯 {slotIndex}: <b>{quickSlotState.item.itemData.itemName}</b> {oldCount}개 → {quickSlotState.itemCount}개 (사용: -{amount})");
        return false;
    }
    
    /// <summary>
    /// UI에 아이템 개수를 업데이트합니다.
    /// </summary>
    private void UpdateCountDisplay()
    {
        if (countText == null) return;
        
        // 아이템이 없거나 개수가 1개면 텍스트를 숨김
        if (quickSlotState.item == null || quickSlotState.itemCount <= 1)
        {
            countText.gameObject.SetActive(false);
        }
        else
        {
            countText.gameObject.SetActive(true);
            countText.text = quickSlotState.itemCount.ToString();
        }
    }
    
    public int GetCount()
    {
        return quickSlotState.itemCount;
    }

    public bool IsEmpty()
    {
        return quickSlotState.item == null;
    }

    public Item GetItem()
    {
        return quickSlotState.item;
    }

    public object GetState()
    {
        return quickSlotState.itemSerializedState;
    }

    public void UpdateItemState(object newState)
    {
        quickSlotState.itemSerializedState = newState;
    }

    public void Select()
    {
        Debug.Log($"Slot {slotIndex} selected.");
    }

    public void Deselect()
    {
        Debug.Log($"Slot {slotIndex} deselected.");
    }

    public void RemoveItem()
    {
        quickSlotState.item = null;
        quickSlotState.itemSerializedState = null;
        quickSlotState.uniqueItemID = null;
        quickSlotState.itemCount = 1;
        UpdateCountDisplay();
    }
// #if UNITY_EDITOR
    [ContextMenu("Execute")]
    private void ExecuteInEditor()
    {
        Debug.Log($"{quickSlotState.item.GetUniqueInstanceID()}");
    }
// #endif
}

// [Serializable]
// public class QuickSlotState
// {
//     public string uniqueItemID; // 슬롯에 들어있는 아이템의 ID. 없으면 null
//     public Item item;
//     public object state;
// }