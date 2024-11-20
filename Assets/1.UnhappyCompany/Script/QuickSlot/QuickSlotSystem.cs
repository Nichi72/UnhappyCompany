using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuickSlotSystem : MonoBehaviour
{
    public static QuickSlotSystem instance = null;
    [Header("Quick Slot UI")]
    public Transform quickSlotContainer; // ������ UI�� �θ� ������Ʈ
    public GameObject quickSlotPrefab; // ������ UI ������
    public int maxQuickSlots = 9; // �ִ� ������ ����

    [SerializeField] private int currentQuickSlotCount = 5; // ���� ������ ����

    [SerializeField] private List<QuickSlot> quickSlots = new List<QuickSlot>();
    [ReadOnly][SerializeField] private int selectedSlotIndex = -1;
    [ReadOnly] public GameObject mountingItem = null;
    [ReadOnly] [SerializeField] private Item currentItem;
    [SerializeField] private Player player;

    [Header("Player Settings")]
    public PlayerStatus playerStatus; // �÷��̾� ���� (��: ���ǵ� ��)

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
        // ������ �ʱ�ȭ
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
        // 1������ 9�� Ű �Է� ó���Ͽ� ���� ����
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
            if(mountingItem != null) Destroy(mountingItem); // ������� �ı�
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
        // �� ���Կ� ������ �߰�
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
        // ������ ���Կ� ���� �÷��̾� ���ǵ� ������Ʈ
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