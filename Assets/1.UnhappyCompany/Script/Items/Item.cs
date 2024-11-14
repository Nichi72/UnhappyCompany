using UnityEngine;

public abstract class Item : MonoBehaviour , InteractionF
{
    public ItemData itemData; // 스크립터블 오브젝트를 통한 아이템 데이터
   
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
        // 아이템을 습득할 때 월드에서 오브젝트를 제거
        Debug.Log($"{itemData.itemName} picked up.");
        Destroy(gameObject);
        QuickSlotSystem.instance.UpdatePlayerSpeed();
    }

    

}
