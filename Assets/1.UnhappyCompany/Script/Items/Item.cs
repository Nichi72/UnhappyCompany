using UnityEngine;
using UnityEngine.Localization.Settings;
using MyUtility;
using System;
[Serializable]
public abstract class Item : MonoBehaviour , IInteractable , IToolTip
{
    public ItemData itemData;
    // string interactionText;

    public string InteractionText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Item_ITR"); set => InteractionText = value; }
    public virtual string ToolTipText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Item_TT_Drop"); set => ToolTipText = value; }
    public virtual string ToolTipText2 { get => ""; set => ToolTipText2 = value; }
    public virtual string ToolTipText3 { get => ""; set => ToolTipText3 = value; }

    [SerializeField]protected string uniqueInstanceID;

    public bool useItemDataPosition = false;
    public bool useItemDataRotation = false;

    void Awake()
    {
        if (string.IsNullOrEmpty(uniqueInstanceID))
        {
            uniqueInstanceID = System.Guid.NewGuid().ToString();
        }
    }

    private void Start()
    {
    }

    public string GetUniqueInstanceID()
    {
        return uniqueInstanceID;
    }

    public virtual void HitEventInteractionF(Player player)
    {
        Debug.Log("HitEvent!");
        Item newItem = itemData.prefab.GetComponent<Item>();
        object state = SerializeState();
        player.quickSlotSystem.AddItemToQuickSlot(newItem, state, uniqueInstanceID);
        PickUp(player);
    }

    public virtual void OnDrop()
    {
        var rigidbody = GetComponent<Rigidbody>();
        if(rigidbody != null)
        {
            rigidbody.isKinematic = false;
        }
        var animator = GetComponent<Animator>();
        if(animator != null)
        {
            animator.enabled = false;
        }
    }

    public virtual void Use(Player player)
    {
        // Debug.Log($"{itemData.itemName} USE");
    }

    public virtual void PickUp(Player player)
    {
        Debug.Log($"{itemData.itemName} picked up.");

        // SavableItemData가 있는 경우, 주운 상태로 표시
        if(itemData != null && itemData.savableItemData != null)
        {
            itemData.savableItemData.isPickedUp = true;
        }
        
        Destroy(gameObject);
        player.quickSlotSystem.UpdatePlayerSpeed();
    }

    public virtual void Mount(Player player , object state = null)
    {
        Debug.Log($"{itemData.itemName} mounted.");
        if(state != null)
        {
            DeserializeState(state);
        }

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
    
    private int GenerateItemID()
    {
        return System.Guid.NewGuid().GetHashCode();
    }
    /// <summary>
    /// 상태 적용을 위한 가상 메서드 (별도 처리하지 않으면 아무 것도 하지 않음)
    /// </summary>
 
    public virtual void DeserializeState(object state) 
    { 

    }
        // 상태 저장을 위한 가상 메서드 (특별한 상태가 없으면 기본적으로 null 반환)
    public virtual object SerializeState() 
    { 
        return null; 
    }

    public void AssignUniqueInstanceID(string newID)
    {
        uniqueInstanceID = newID;
    }
}


