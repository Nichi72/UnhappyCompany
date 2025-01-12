using UnityEngine;

public abstract class Item : MonoBehaviour , IInteractable
{
    public ItemData itemData; 
   
    public virtual void HitEventInteractionF(Player player)
    {
        Debug.Log("HitEvent!");
        player.quickSlotSystem.AddItemToQuickSlot(itemData.prefab.GetComponent<Item>());
        PickUp(player);
    }

    public virtual void Use(Player player)
    {
        Debug.Log($"{itemData.itemName} USE");
        var animator = GetComponent<Animator>();
        if(animator != null)
        {
            animator.enabled = true;
        }
    }

    public virtual void PickUp(Player player)
    {
        Debug.Log($"{itemData.itemName} picked up.");
        Destroy(gameObject);
        player.quickSlotSystem.UpdatePlayerSpeed();
    }

    public virtual void Mount()
    {
        Debug.Log($"{itemData.itemName} mounted.");
        var animator = GetComponent<Animator>();
        var rigidbody = GetComponent<Rigidbody>();
        if(rigidbody != null)
        {
            rigidbody.isKinematic = true;
        }
        if(animator != null)
        {
            animator.enabled = true;
        }
    }

    

}
