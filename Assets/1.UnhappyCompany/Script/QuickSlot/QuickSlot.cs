using UnityEngine;
using UnityEngine.UI;
public class QuickSlot : MonoBehaviour
{
    private int slotIndex;
    [SerializeField] private Item item;
    public Image icon; // 슬롯에 표시될 아이템 아이콘

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public void SetItem(Item newItem)
    {
        item = newItem;
        icon.sprite = newItem.itemData.icon;
        icon.enabled = true;
    }

    public Item GetItem()
    {
        return item;
    }

    public bool IsEmpty()
    {
        return item == null;
    }

    public void Select()
    {
        // 슬롯 선택 시 시각적 효과 추가
        Debug.Log($"Slot {slotIndex} selected.");
    }

    public void Deselect()
    {
        // 슬롯 선택 해제 시 시각적 효과 제거
        Debug.Log($"Slot {slotIndex} deselected.");
    }

    public void RemoveItem()
    {
        item = null;
    }
}