using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

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
    /// <summary>
    /// 현재 들고 있는 아이템 오브젝트
    /// </summary>
    [ReadOnly] public GameObject currentItemObject = null;
    /// <summary>
    /// 현재 들고 있는 아이템의 아이템 클래스
    /// </summary>
    [ReadOnly] [SerializeField] private Item currentItem;
    [SerializeField] private Player player;

    public float totalWeight = 0;


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
#region 퀵슬롯 UI 관련
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

    private void HandleQuickSlotInput()
    {
        for (int i = 0; i < maxQuickSlots; i++)
        {
            if (Input.GetKeyDown((KeyCode)(KeyCode.Alpha1 + i)))
            {
                SelectSlot(i);
            }
        }
    }
#endregion
    
    #region 퀵슬롯 아이템 관련
    /// <summary>
    /// 슬롯을 선택 했을 때 호출 되는 함수
    /// </summary>
    /// <param name="slotIndex"></param>
    private void SelectSlot(int slotIndex)
    {
        if (slotIndex < quickSlots.Count)
        {
            if(selectedSlotIndex == slotIndex && currentItemObject != null)
            {
                Debug.Log("SelectSlot same slot");
                return;
            }

            if (selectedSlotIndex != -1 && selectedSlotIndex < quickSlots.Count)
            {
                quickSlots[selectedSlotIndex].Deselect();
            }

            selectedSlotIndex = slotIndex;
            quickSlots[selectedSlotIndex].Select();
            if(currentItemObject != null) Destroy(currentItemObject); 
            currentItem = GetCurrentItemInSlot();
            object state = GetCurrentItemStateInSlot();
            if (currentItem == null)
            {
                return;
            }
            MountItem(currentItem.itemData , state);
        }
    }

    /// <summary>
    /// 퀵슬롯에 아이템을 추가하는 함수
    /// </summary>
    /// <param name="item"></param>
    public void AddItemToQuickSlot(Item item, object state , string uniqueInstanceID)
    {
        string newItemID = uniqueInstanceID;
        Debug.Log($"AddItemToQuickSlot: {item.itemData.name}, ID = {newItemID}");
        foreach (QuickSlot slot in quickSlots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(item, state, uniqueInstanceID);
                Debug.Log($"QuickSlotSystem: 빈 슬롯에 {newItemID} 아이템 등록 완료.");
                UpdatePlayerSpeed();
                return;
            }
        }

        Debug.LogWarning("QuickSlotSystem: 더 이상 아이템을 넣을 슬롯이 없습니다.");
    }


    /// <summary>
    /// 아이템을 선택해서 플레이어가 손에 들게 하는 함수
    /// </summary>
    /// <param name="itemData"></param>
    public void MountItem(ItemData itemData , object state = null)
    {
        currentItemObject = Instantiate(itemData.prefab);
        currentItemObject.transform.SetParent(player.rightHandPos);
        currentItemObject.GetComponent<Item>().itemData = itemData;
        currentItemObject.GetComponent<Item>().Mount(player, state);
    }

    /// <summary>
    /// 현재 들고 있는 아이템을 버리는 함수
    /// </summary>
    /// <returns></returns>
    public GameObject DropItem()
    {
        if(currentItemObject == null)
        {
            return null;
        }
        var temp = currentItemObject;
        currentItemObject.GetComponent<Item>().OnDrop();
        ClearCurrentItemSlot();
        Debug.Log($"DropItem {temp.name}");
        return temp;
    }
#endregion

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
        currentItemObject.GetComponent<Item>().UnMount();
        if (currentItemObject != null)
        {
            currentItemObject.transform.SetParent(null);
            currentItemObject = null;
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
                return item;
            }
        }
        return null;
    }
    public object GetCurrentItemStateInSlot()
    {
        int slotIndex = selectedSlotIndex;
        if (slotIndex < quickSlots.Count)
        {
            QuickSlot slot = quickSlots[slotIndex];
            if (!slot.IsEmpty())
            {
                object state = slot.GetState();
                return state;
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
            return slot;
        }
        return null;
    }

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

    // (1) 퀵슬롯들의 상태를 한꺼번에 담을 클래스
    [Serializable]
    public class QuickSlotSystemState
    {
        public List<QuickSlotState> slots = new List<QuickSlotState>();
    }

    // (2) 퀵슬롯 시스템 직렬화 메서드
    public QuickSlotSystemState SerializeSystem()
    {
        QuickSlotSystemState systemState = new QuickSlotSystemState();

        foreach (var slot in quickSlots)
        {
            QuickSlotState slotState = new QuickSlotState();
            // 슬롯이 비어있지 않다면, 해당 슬롯의 QuickSlotState에 저장된 정보 복사
            if (!slot.IsEmpty())
            {
                // slot.quickSlotState에는 아이템의 ID, 직렬화 상태 등이 이미 들어 있음
                slotState.uniqueItemID = slot.quickSlotState.uniqueItemID;
                slotState.itemSerializedState = slot.quickSlotState.itemSerializedState;
                slotState.item = slot.quickSlotState.item;
            }
            else
            {
                slotState.uniqueItemID = null;
                slotState.itemSerializedState = null;
                slotState.item = null;
            }
            systemState.slots.Add(slotState);
        }
        return systemState;
    }

    // (3) 퀵슬롯 시스템 역직렬화(로드) 메서드
    public void DeserializeSystem(QuickSlotSystemState systemState)
    {
        for (int i = 0; i < quickSlots.Count; i++)
        {
            QuickSlot slot = quickSlots[i];
            QuickSlotState slotState = systemState.slots[i];

            if (!string.IsNullOrEmpty(slotState.uniqueItemID))
            {
                // 아이템의 직렬화 상태를 사용하여 아이템 복구
                Item restoredItem = slotState.item;

                if (restoredItem != null)
                {
                    // 복구된 아이템을 슬롯에 할당
                    slot.SetItem(restoredItem, slotState.itemSerializedState, slotState.uniqueItemID);
                }
                else
                {
                    Debug.LogWarning(
                        $"DeserializeSystem: 고유ID [{slotState.uniqueItemID}]에 해당하는 아이템을 복구할 수 없습니다."
                    );
                }
            }
            else
            {
                // 슬롯 비우기
                slot.RemoveItem();
            }
        }
    }

    private Item DeserializeItem(QuickSlotState slotState)
    {
         // 인스턴스 생성
        // 내부 필요 스탯 덮어씌우기
        // 인스턴스ID 할당
        

        // 아이템의 직렬화 상태를 사용하여 아이템을 복구하는 로직 구현
        // 예시: 직렬화된 데이터를 사용하여 새로운 아이템 인스턴스를 생성
        // 이 부분은 실제 구현에 따라 달라질 수 있음
        return null; // 실제 구현 필요
    }
}

[Serializable]
public class QuickSlotState
{
    public string uniqueItemID; // 슬롯에 들어있는 아이템의 ID. 없으면 null
    public object itemSerializedState; // 필요하면 추가(내부 상태)
    public Item item;
}

