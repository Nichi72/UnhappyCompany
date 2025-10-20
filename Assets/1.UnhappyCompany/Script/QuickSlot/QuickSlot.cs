using System;
using UnityEngine;
using UnityEngine.UI;
public class QuickSlot : MonoBehaviour
{
    private int slotIndex;
    public QuickSlotState quickSlotState;
    public Image icon; // 

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public void SetItem(Item newItem , object itemSerializedState , string uniqueInstanceID)
    {
        quickSlotState.item = newItem;
        icon.sprite = newItem.itemData.icon;
        icon.enabled = true;
        quickSlotState.itemSerializedState = itemSerializedState;
        quickSlotState.uniqueItemID = uniqueInstanceID;
        // item.AssignUniqueInstanceID(uniqueInstanceID);
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