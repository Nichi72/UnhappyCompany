using UnityEngine;
using UnityEngine.UI;
public class QuickSlot : MonoBehaviour
{
    private int slotIndex;
    [SerializeField] private Item item;
    public Image icon; // ���Կ� ǥ�õ� ������ ������

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
        // ���� ���� �� �ð��� ȿ�� �߰�
        Debug.Log($"Slot {slotIndex} selected.");
    }

    public void Deselect()
    {
        // ���� ���� ���� �� �ð��� ȿ�� ����
        Debug.Log($"Slot {slotIndex} deselected.");
    }

    public void RemoveItem()
    {
        item = null;
    }
}