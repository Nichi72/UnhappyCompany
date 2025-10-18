using UnityEngine;
using UnityEngine.Localization.Settings;
using MyUtility;
using System;
[Serializable]
public abstract class Item : MonoBehaviour , IInteractableF , IToolTip
{
    public ItemData itemData;
    // string interactionText;

    public string InteractionTextF { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Item_ITR"); set => InteractionTextF = value; }
    public bool IgnoreInteractionF { get; set; } = false;
    public virtual string ToolTipText { get => LocalizationUtils.GetLocalizedString(tableEntryReference: "Item_TT_Drop"); set => ToolTipText = value; }
    public virtual string ToolTipText2 { get => ""; set => ToolTipText2 = value; }
    public virtual string ToolTipText3 { get => ""; set => ToolTipText3 = value; }

    [SerializeField]protected string uniqueInstanceID;
    
    /// <summary>
    /// 원래 레이어 상태 저장 (Drop 시 복구용)
    /// </summary>
    private int originalLayer = -1;

    public bool useItemDataPosition = false;
    public bool useItemDataRotation = false;
    /// <summary>
    /// 모델 손을 사용할건지 여부
    /// Mount 함수 이전에 설정해야 함.
    /// </summary>
    public bool isModelHandAnimation = false;

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
        // 레이어 복구
        RestoreOriginalLayer();
        
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
        ExceptionItem();
        
        // 손에 들 때 레이어 저장 및 변경 (Raycast 무시용)
        SaveAndChangeLayer();
        
        player.SetModelHandTransform(isModelHandAnimation);
        player.firstPersonController.SetPivotHandTransform(isModelHandAnimation);
        transform.SetParent(player.rightHandPos);

        if(itemData.ItemPosition != Vector3.zero)
        {
            transform.localPosition = itemData.ItemPosition;
        }
        if(itemData.ItemRotation != Vector3.zero)
        {
            transform.localRotation = Quaternion.Euler(itemData.ItemRotation);
        }
        if(itemData.ItemScale != Vector3.zero)
        {
            transform.localScale = itemData.ItemScale;
        }
        Debug.Log($"{transform.position} mounted pos.");
        ToolTipUI.instance.SetToolTip(this);
    }

    public virtual void UnMount()
    {
        ToolTipUI.instance.SetToolTip(null);
        ResetAnimatorLayers();
    }

    private void ResetAnimatorLayers()
    {
        var animator = GetComponent<Animator>();
        // Debug.Log($"ResetAnimatorLayers{animator.isActiveAndEnabled}");
        if(animator != null)
        {
            for (int i = 0; i < animator.layerCount; i++)
            {
                animator.SetLayerWeight(i, 0);
            }
        }
    }

    public void ExceptionItem()
    {
        if(gameObject.layer != LayerMask.NameToLayer("PlayerRaycastHit"))
        {
            // gameObject.layer = LayerMask.NameToLayer("PlayerRaycastHit");
            // Debug.LogError("Item은 PlayerRaycastHit가 일반적입니다. " + gameObject.layer);
        }

        if(gameObject.GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("Item은 Rigidbody가 일반적입니다. " + gameObject.name);
            // Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            // rigidbody.isKinematic = true;
        }


    }
    
    /// <summary>
    /// 현재 레이어를 저장하고 "Ignore Raycast" 레이어로 변경합니다.
    /// 이를 통해 손에 들고 있는 아이템이 레이캐스트에 감지되지 않도록 합니다.
    /// </summary>
    private void SaveAndChangeLayer()
    {
        // 원래 레이어 저장
        originalLayer = gameObject.layer;
        
        // Ignore Raycast 레이어로 변경 (레이어 인덱스 2)
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        SetLayerRecursively(gameObject, ignoreRaycastLayer);
        
        Debug.Log($"[Item] {itemData?.itemName ?? gameObject.name}: 레이어 저장 (원래: {LayerMask.LayerToName(originalLayer)}) 및 변경 (새로운: Ignore Raycast)");
    }
    
    /// <summary>
    /// 저장된 원래 레이어로 복구합니다.
    /// </summary>
    private void RestoreOriginalLayer()
    {
        if (originalLayer != -1)
        {
            SetLayerRecursively(gameObject, originalLayer);
            Debug.Log($"[Item] {itemData?.itemName ?? gameObject.name}: 레이어 복구 (복구된 레이어: {LayerMask.LayerToName(originalLayer)})");
            originalLayer = -1; // 복구 후 초기화
        }
    }
    
    /// <summary>
    /// GameObject와 모든 자식 오브젝트의 레이어를 재귀적으로 변경합니다.
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        
        obj.layer = layer;
        
        // 모든 자식 오브젝트도 동일하게 변경
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
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
    [ContextMenu("SetEditorItemComponent")]
    public void SetEditorItemComponent()
    {
        var rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = false;

        var boxCollider = gameObject.AddComponent<BoxCollider>();
        // boxCollider.isTrigger = true;
    }
}


