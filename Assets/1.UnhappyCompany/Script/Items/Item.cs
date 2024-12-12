using UnityEngine;

public abstract class Item : MonoBehaviour , IInteractable
{
    public ItemData itemData; // ��ũ���ͺ� ������Ʈ�� ���� ������ ������
   
    public virtual void HitEventInteractionF(Player player)
    {
        Debug.Log("HitEvent!");
        QuickSlotSystem.instance.AddItemToQuickSlot(itemData.prefab.GetComponent<Item>());
        PickUp();
    }

    public virtual void Use()
    {
        Debug.Log($"{itemData.itemName} USE");
    }

    public virtual void PickUp()
    {
        // �������� ������ �� ���忡�� ������Ʈ�� ����
        Debug.Log($"{itemData.itemName} picked up.");
        Destroy(gameObject);
        QuickSlotSystem.instance.UpdatePlayerSpeed();
    }

    

}
