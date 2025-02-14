using UnityEngine;
using UnityEngine.Localization.Settings;
using MyUtility;
using System;
public abstract class Item : MonoBehaviour , IInteractable , IToolTip, ISavable
{

    public ItemData itemData;
    // string interactionText;

    public string InteractionText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Item_ITR"); set => InteractionText = value; }
    public virtual string ToolTipText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Item_TT_Drop"); set => ToolTipText = value; }
    public virtual string ToolTipText2 { get => ""; set => ToolTipText2 = value; }
    public virtual string ToolTipText3 { get => ""; set => ToolTipText3 = value; }
    // override public string ToolTipText { get => "[LM]발사"; set => ToolTipText = value; } // [LM]발사
    // override public string ToolTipText2 { get => "[RM]공기 충전."; set => ToolTipText2 = value; } // [RM]공기 충전.
    // override public string ToolTipText3 { get => "[R]물 충전."; set => ToolTipText3 = value; } // [R]물 충전. 

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

    public virtual void SaveState()
    {
        if(itemData != null)
        {
            string key = "ItemData_" + itemData.savableItemData.GetItemID();
            ES3.Save<SavableItemData>(key, itemData.savableItemData, SaveManager.Instance.saveFileName);
            Debug.Log("아이템 데이터 저장 완료: " + key);
        }
    }
    public virtual void LoadState()
    {
        if(itemData != null)
        {
            string key = "ItemData_" + itemData.savableItemData.GetItemID();
            if (ES3.KeyExists(key, SaveManager.Instance.saveFileName))
            {
                itemData.savableItemData = ES3.Load<SavableItemData>(key, SaveManager.Instance.saveFileName);
                // 필요하다면 불러온 데이터로 transform 갱신
                transform.position = itemData.savableItemData.position;
                transform.rotation = itemData.savableItemData.rotation;
                transform.localScale = itemData.savableItemData.scale;
                Debug.Log("아이템 데이터 불러오기 성공: " + key);
            }
            else
            {
                Debug.LogWarning("저장된 아이템 데이터가 없습니다: " + key);
            }
        }
    }
}


