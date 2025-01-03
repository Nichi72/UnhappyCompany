using UnityEngine;

public abstract class Item : MonoBehaviour , IInteractable
{
    public ItemData itemData; 
   
    public virtual void HitEventInteractionF(Player player)
    {
        Debug.Log("HitEvent!");
        QuickSlotSystem.instance.AddItemToQuickSlot(itemData.prefab.GetComponent<Item>());
        PickUp();
    }

    public virtual void Use()
    {
        Debug.Log($"{itemData.itemName} USE");
        var animator = GetComponent<Animator>();
        if(animator != null)
        {
            animator.enabled = true;
        }
    }

    public virtual void PickUp()
    {
        Debug.Log($"{itemData.itemName} picked up.");
        Destroy(gameObject);
        QuickSlotSystem.instance.UpdatePlayerSpeed();
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
