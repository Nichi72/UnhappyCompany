using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuickSlotSystem : MonoBehaviour
{
    // public static QuickSlotSystem instance = null;
    [Header("Quick Slot UI")]
    public Transform quickSlotContainer; 
    public GameObject quickSlotPrefab; 
    public int maxQuickSlots = 9;

    [SerializeField] private int currentQuickSlotCount = 5; 

    [SerializeField] private List<QuickSlot> quickSlots = new List<QuickSlot>();
    [ReadOnly][SerializeField] private int selectedSlotIndex = -1;
    [ReadOnly] public GameObject currentItemObject = null;
    [ReadOnly] [SerializeField] private Item currentItem;
    [SerializeField] private Player player;

    [Header("Player Settings")]
    public PlayerStatus playerStatus; 

    private void Awake()
    {
        // if(instance == null)
        // {
        //     instance = this;
        // }
    }

    private void Start()
    {
        InitializeQuickSlots();
    }

    private void Update()
    {
        HandleQuickSlotInput();
    }

    void InitializeQuickSlots()
    {
        for (int i = 0; i < currentQuickSlotCount; i++)
        {
            AddQuickSlot();
        }
    }

    public void AddQuickSlot()
    {
        if (quickSlots.Count < maxQuickSlots)
        {
            GameObject slotObj = Instantiate(quickSlotPrefab, quickSlotContainer);
            QuickSlot newSlot = slotObj.GetComponent<QuickSlot>();
            quickSlots.Add(newSlot);
            newSlot.SetSlotIndex(quickSlots.Count);
        }
    }

    void HandleQuickSlotInput()
    {
        for (int i = 0; i < maxQuickSlots; i++)
        {
            if (Input.GetKeyDown((KeyCode)(KeyCode.Alpha1 + i)))
            {
                SelectSlot(i);
            }
        }
    }

    void SelectSlot(int slotIndex)
    {
        if (slotIndex < quickSlots.Count)
        {
            if (selectedSlotIndex != -1 && selectedSlotIndex < quickSlots.Count)
            {
                quickSlots[selectedSlotIndex].Deselect();
            }

            selectedSlotIndex = slotIndex;
            quickSlots[selectedSlotIndex].Select();
            //UseItemInSlot(selectedSlotIndex);
            if(currentItemObject != null) Destroy(currentItemObject); 
            currentItem = GetCurrentItemInSlot();

            if (currentItem == null)
            {
                return;
            }
            
            MountItem(currentItem.itemData);
        }
    }

    public void AddItemToQuickSlot(Item item)
    {
        foreach (QuickSlot slot in quickSlots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(item);
                break;
            }
        }
    }

    public void MountItem(ItemData itemData)
    {
        currentItemObject = Instantiate(itemData.prefab);
        currentItemObject.transform.SetParent(player.rightHandPos);
        currentItemObject.GetComponent<Item>().Mount(player);
        // currentItemObject.transform.localPosition = itemData.HandPosition;
        // currentItemObject.transform.localRotation = Quaternion.Euler(itemData.HandRotation);
    }

    public GameObject DropItem()
    {
        if(currentItemObject == null)
        {
            return null;
        }
        var temp = currentItemObject;
        var rigidbody = currentItemObject.GetComponent<Rigidbody>();
        if(rigidbody != null)
        {
            rigidbody.isKinematic = false;
        }
        var animator = currentItemObject.GetComponent<Animator>();
        if(animator != null)
        {
            animator.enabled = false;
        }
        ClearCurrentItemSlot();
        Debug.Log($"DropItem {temp.name}");
        return temp;
    }

    public void DestroyCurrentItem()
    {
        if (currentItemObject != null)
        {
            Destroy(currentItemObject);
        }
        ClearCurrentItemSlot();
    }
    public void ClearCurrentItemSlot()
    {
        var tempQuickSlot = GetCurrentQuickSlot();
        if (currentItemObject != null)
        {
            currentItemObject.transform.SetParent(null);
            currentItemObject = null;
            // Destroy(mountingItem);
        }
        if (currentItem != null)
        {
            currentItem = null;
        }

        tempQuickSlot.icon.sprite = null;
        tempQuickSlot.RemoveItem();
        UpdatePlayerSpeed();
        // 애니메이션 레이어 초기화
        var armAnimator = player.armAnimator;
        armAnimator.SetLayerWeight(0, 1); // Base Layer로 초기화
    }

    public Item GetCurrentItemInSlot()
    {
        int slotIndex = selectedSlotIndex;
        if (slotIndex < quickSlots.Count)
        {
            QuickSlot slot = quickSlots[slotIndex];
            if (!slot.IsEmpty())
            {
                Item item = slot.GetItem();
                //item.Use();
                return item;
            }
        }
        return null;
    }
    public QuickSlot GetCurrentQuickSlot()
    {
        int slotIndex = selectedSlotIndex;
        if (slotIndex < quickSlots.Count)
        {
            QuickSlot slot = quickSlots[slotIndex];
            //if (!slot.IsEmpty())
            //{
            //    return slot;
            //}
            return slot;
           
        }
        return null;
    }

    public float totalWeight = 0;
    public void UpdatePlayerSpeed()
    {
        float totalWeightTemp = 0;
        foreach (QuickSlot slot in quickSlots)
        {
            if (!slot.IsEmpty())
            {
                totalWeightTemp += slot.GetItem().itemData.weight;
            }
        }
        totalWeight = totalWeightTemp;
        player.firstPersonController.SetSpeedBasedOnWeight(totalWeight);
    }
}