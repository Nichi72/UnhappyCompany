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
                
                // 이전 슬롯의 아이템 상태를 저장
                if(currentItemObject != null)
                {
                    Item currentItemComponent = currentItemObject.GetComponent<Item>();
                    object currentState = currentItemComponent.SerializeState();
                    quickSlots[selectedSlotIndex].UpdateItemState(currentState);
                }
            }

            selectedSlotIndex = slotIndex;
            quickSlots[selectedSlotIndex].Select();
            if(currentItemObject != null)
            {
                currentItemObject.GetComponent<Item>().UnMount();
                Debug.Log("슬롯 변경으로 다른 아이템으로 변경되면서 삭제됨. Destroy currentItemObject");
                Destroy(currentItemObject);
            }
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
    /// 스택 가능한 아이템(예: Coin)은 같은 아이템이 있는 슬롯에 먼저 추가를 시도합니다.
    /// </summary>
    /// <param name="item"></param>
    public void AddItemToQuickSlot(Item item, object state , string uniqueInstanceID, int count = 1)
    {
        string newItemID = uniqueInstanceID;
        Debug.Log($"AddItemToQuickSlot: {item.itemData.name}, ID = {newItemID}, Count = {count}");
        
        int remainingCount = count;
        ItemStackType stackType = item.itemData.stackType;
        
        // 스택 가능한 아이템인 경우 (Single이 아닌 경우)
        if (stackType != ItemStackType.Single)
        {
            // 1단계: 같은 아이템이 있는 슬롯을 찾아서 추가 시도
            foreach (QuickSlot slot in quickSlots)
            {
                if (!slot.IsEmpty() && 
                    slot.GetItem().itemData == item.itemData)
                {
                    int overflow = slot.AddCount(remainingCount);
                    remainingCount = overflow;
                    
                    if (remainingCount == 0)
                    {
                        Debug.Log($"QuickSlotSystem: 기존 슬롯에 {count}개 추가 완료.");
                        UpdatePlayerSpeed();
                        return;
                    }
                }
            }
            
            // 2단계: 남은 개수가 있으면 빈 슬롯에 추가
            while (remainingCount > 0)
            {
                QuickSlot emptySlot = FindEmptySlot();
                if (emptySlot == null)
                {
                    Debug.LogWarning($"QuickSlotSystem: 슬롯이 부족합니다. {remainingCount}개를 추가하지 못했습니다.");
                    break;
                }
                
                int maxStack = (int)stackType;
                int countToAdd = Mathf.Min(remainingCount, maxStack);
                emptySlot.SetItem(item, state, uniqueInstanceID, countToAdd);
                remainingCount -= countToAdd;
                
                Debug.Log($"QuickSlotSystem: 새 슬롯에 {countToAdd}개 추가. 남은 개수: {remainingCount}");
            }
        }
        else
        {
            // 스택 불가능한 아이템 (Single) - 기존 로직
            QuickSlot emptySlot = FindEmptySlot();
            if (emptySlot != null)
            {
                emptySlot.SetItem(item, state, uniqueInstanceID, 1);
                Debug.Log($"QuickSlotSystem: 빈 슬롯에 {newItemID} 아이템 등록 완료.");
            }
            else
            {
                Debug.LogWarning("QuickSlotSystem: 더 이상 아이템을 넣을 슬롯이 없습니다.");
            }
        }
        
        UpdatePlayerSpeed();
    }
    
    /// <summary>
    /// 빈 슬롯을 찾습니다.
    /// </summary>
    private QuickSlot FindEmptySlot()
    {
        foreach (QuickSlot slot in quickSlots)
        {
            if (slot.IsEmpty())
            {
                return slot;
            }
        }
        return null;
    }


    /// <summary>
    /// 아이템을 선택해서 플레이어가 손에 들게 하는 함수
    /// </summary>
    /// <param name="itemData"></param>
    public void MountItem(ItemData itemData , object state = null)
    {
        currentItemObject = Instantiate(itemData.prefab);
        // currentItemObject.transform.SetParent(player.rightHandPos); // Mount로 옮겨짐
        currentItemObject.GetComponent<Item>().itemData = itemData;
        currentItemObject.GetComponent<Item>().Mount(player, state);
        currentItemObject.GetComponent<Rigidbody>().isKinematic = true;

        if(currentItemObject.GetComponent<Item>().useItemDataPosition)
        {
            currentItemObject.transform.localPosition = itemData.ItemPosition;
        }
        if(currentItemObject.GetComponent<Item>().useItemDataRotation)
        {
            currentItemObject.transform.localRotation = Quaternion.Euler(itemData.ItemRotation);
        }
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
        currentItemObject.GetComponent<Rigidbody>().isKinematic = false;
        currentItemObject.GetComponent<Item>().OnDrop();
        ClearCurrentItemSlot();
        Debug.Log($"DropItem {temp.name}");
        return temp;
    }
#endregion

    public void DestroyCurrentItem()
    {
        ClearCurrentItemSlot();
        if (currentItemObject != null)
        {
            Destroy(currentItemObject);
        }
    }
    public void ClearCurrentItemSlot()
    {
        Debug.Log("ClearCurrentItemSlot");
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
    
    /// <summary>
    /// 퀵슬롯에서 코인을 찾습니다
    /// </summary>
    /// <returns>코인을 찾으면 true, 못 찾으면 false</returns>
    public bool HasCoin()
    {
        foreach (QuickSlot slot in quickSlots)
        {
            if (!slot.IsEmpty())
            {
                Item item = slot.GetItem();
                if (item is CoinItem)
                {
                    Debug.Log("HasCoin: " + item.itemData.name);
                    return true;
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// 퀵슬롯에서 코인을 제거합니다 (1개만)
    /// </summary>
    /// <returns>제거된 코인의 가치, 없으면 0</returns>
    public int RemoveCoin()
    {
        for (int i = 0; i < quickSlots.Count; i++)
        {
            QuickSlot slot = quickSlots[i];
            if (!slot.IsEmpty())
            {
                Item item = slot.GetItem();
                if (item is CoinItem coinItem)
                {
                    int coinValue = coinItem.GetCoinValue();
                    
                    // 현재 들고 있는 아이템이면 언마운트하고 삭제
                    if (selectedSlotIndex == i && currentItemObject != null)
                    {
                        currentItemObject.GetComponent<Item>().UnMount();
                        Destroy(currentItemObject);
                        currentItemObject = null;
                        currentItem = null;
                    }
                    
                    // 슬롯에서 아이템 제거
                    slot.icon.sprite = null;
                    slot.icon.enabled = false;
                    slot.RemoveItem();
                    UpdatePlayerSpeed();
                    
                    Debug.Log($"QuickSlotSystem: 코인 제거 완료 (가치: {coinValue})");
                    return coinValue;
                }
            }
        }
        Debug.LogWarning("QuickSlotSystem: 제거할 코인을 찾을 수 없습니다.");
        return 0;
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
                slotState.itemCount = slot.quickSlotState.itemCount; // 개수 저장
            }
            else
            {
                slotState.uniqueItemID = null;
                slotState.itemSerializedState = null;
                slotState.item = null;
                slotState.itemCount = 1;
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
                    // 복구된 아이템을 슬롯에 할당 (개수 포함)
                    slot.SetItem(restoredItem, slotState.itemSerializedState, slotState.uniqueItemID, slotState.itemCount);
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
    public int itemCount = 1; // 스택 가능 아이템의 개수 (기본값 1)
}

