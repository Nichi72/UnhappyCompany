using UnityEngine;
using UnityEngine.Localization.Settings;
using MyUtility;
using System;
public abstract class Item : MonoBehaviour , IInteractable , IToolTip
{
    public ItemData itemData;
    // string interactionText;

    public string InteractionText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Item_ITR"); set => InteractionText = value; }
    public virtual string ToolTipText { get => ""; set => ToolTipText = value; }
    public virtual string ToolTipText2 { get => ""; set => ToolTipText2 = value; }
    public virtual string ToolTipText3 { get => ""; set => ToolTipText3 = value; }

    private void Start()
    {
        // ToolTipText = 
    }
    public virtual void HitEventInteractionF(Player player)

    {
        Debug.Log("HitEvent!");
        player.quickSlotSystem.AddItemToQuickSlot(itemData.prefab.GetComponent<Item>());
        PickUp(player);
    }


    public virtual void Use(Player player)
    {
        // Debug.Log($"{itemData.itemName} USE");
    }

    public virtual void PickUp(Player player)
    {
        Debug.Log($"{itemData.itemName} picked up.");
        Destroy(gameObject);
        player.quickSlotSystem.UpdatePlayerSpeed();
    }

    public virtual void Mount(Player player)
    {
        Debug.Log($"{itemData.itemName} mounted.");
        ToolTipUI.instance.SetToolTip(this);
    }

    public virtual void UnMount()
    {
        ToolTipUI.instance.SetToolTip(null);
    }


    public void ExceptionItem()
    {
        if(gameObject.layer != LayerMask.NameToLayer("PlayerRaycastHit"))
        {
            // gameObject.layer = LayerMask.NameToLayer("PlayerRaycastHit");
            Debug.LogError("Item은 PlayerRaycastHit가 일반적입니다. " + gameObject.layer);
        }

        if(gameObject.GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("Item은 Rigidbody가 일반적입니다. " + gameObject.name);
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
        }


    }
}
