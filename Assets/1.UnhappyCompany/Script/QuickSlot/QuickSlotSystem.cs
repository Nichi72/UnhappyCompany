using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuickSlotSystem : MonoBehaviour
{
    public static QuickSlotSystem instance = null;
    [Header("Quick Slot UI")]
    public Transform quickSlotContainer; // 퀵슬롯 UI의 부모 오브젝트
    public GameObject quickSlotPrefab; // 퀵슬롯 UI 프리팹
    public int maxQuickSlots = 9; // 최대 퀵슬롯 개수

    [SerializeField] private int currentQuickSlotCount = 5; // 현재 퀵슬롯 개수

    [SerializeField] private List<QuickSlot> quickSlots = new List<QuickSlot>();
    [ReadOnly][SerializeField] private int selectedSlotIndex = -1;
    [ReadOnly] public GameObject mountingItem = null;
    [ReadOnly] [SerializeField] private Item currentItem;
    [SerializeField] private Player player;

    [Header("Player Settings")]
    public PlayerStatus playerStatus; // 플레이어 상태 (예: 스피드 등)

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
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
        // 퀵슬롯 초기화
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
        // 1번부터 9번 키 입력 처리하여 슬롯 선택
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
            if(mountingItem != null) Destroy(mountingItem); // 들기전에 파괴
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
        // 빈 슬롯에 아이템 추가
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
        mountingItem = Instantiate(itemData.prefab);
        mountingItem.transform.SetParent(player.rightHandPos);
        mountingItem.transform.localPosition = Vector3.zero;
    }

    public void DropItem()
    {
        //var tempQuickSlot = GetCurrentQuickSlot();
        if(mountingItem != null)
        {
            mountingItem.AddComponent<Rigidbody>();
        }
        RemoveCurrentItem();
    }

    public void DestroyCurrentItem()
    {
        if (mountingItem != null)
        {
            Destroy(mountingItem);
        }
        RemoveCurrentItem();
    }
    public void RemoveCurrentItem()
    {
        var tempQuickSlot = GetCurrentQuickSlot();
        if (mountingItem != null)
        {
            mountingItem.transform.SetParent(null);
            mountingItem = null;
            Destroy(mountingItem);
        }
        if (currentItem != null)
        {
            currentItem = null;
        }

        tempQuickSlot.icon.sprite = null;
        tempQuickSlot.RemoveItem();
        UpdatePlayerSpeed();
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
        // 아이템 무게에 따른 플레이어 스피드 업데이트
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