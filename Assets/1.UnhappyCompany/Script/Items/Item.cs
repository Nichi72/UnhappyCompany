using UnityEngine;

public abstract class Item : MonoBehaviour , InteractionF
{
    public ItemData itemData; // 스크립터블 오브젝트를 통한 아이템 데이터
    /// <summary>
    /// 아이템이 월드에 존재하고 있을 때 플레이어에 의해 레이캐스xm를 맞았을 때 호출되는 함수
    /// </summary>
    /// <param name="player"></param>
    /// <param name="raycastHit"></param>
    public virtual void HitEventInteractionF(Player player , RaycastHit raycastHit)
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
