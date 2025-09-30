using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoomNode : MonoBehaviour
{
    public enum RoomType
    {
        KoreaRoom,
        HospitalRoom,
    }
    public RoomSetting roomSetting;

    public RoomType currentRoomType;
    // [ReadOnly] public RoomNode parentRoom;
    [ReadOnly] public DoorEdge connectToParentDoor; // 부모와 연결 할 문
    // 방이 생 성될 문 선택된 문 리스트 (이미 부모와 연결된 간선은 제외됨.)
    [ReadOnly] public List<DoorEdge> SelectedDoors = new List<DoorEdge>();
    [ReadOnly] [SerializeField] private List<DoorEdge> doorList = new List<DoorEdge>();
    public List<OtherGoundChecker> roomGeneratorZones = new List<OtherGoundChecker>();
    public bool isOverlap = false;
    [Header("Door")]
    [Tooltip("여러개의 문이 선택 될 확률")]
    public float multipleDoorProbability = 0.6f;
    public int depth; // 방의 깊이를 저장하는 변수
    public DoorDirection doorDirection;
    [ReadOnly] public bool isOtherDirection = false;
    
    void Awake()
    {
        if(roomGeneratorZones.Count != 0)
        {
            StartCoroutine(CheckOverlap());
        }
    }
    IEnumerator CheckOverlap()
    {
        while(true)
        {
            isOverlap = false;
            foreach (var zone in roomGeneratorZones)
            {
                // 하나라도 오버랩이 있으면 오버랩임
                if (zone.isOverlap)
                {
                    isOverlap = true;
                    break;
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public DoorEdge ConnectToParentRoom(RoomNode otherRoom, DoorGeneration doorGeneration)
    {
        DoorDirection oppositeDirection = GetOppositeDirection(doorDirection);
        Debug.Log("@@@@doorDirection: " + doorDirection);
        foreach (var door in doorList)
        {
            Debug.Log("@@DoorDirection: " + door.direction);
            if(oppositeDirection == door.direction)
            {
                Debug.Log("@@ConnectToParentRoom: " + door.direction);
                connectToParentDoor = door;
                break;
            }
        }
        //connectToParentDoor = doorList[0]; // forTest
        connectToParentDoor.toRoomNode = otherRoom;
        depth = otherRoom.depth + 1; // 부모 방의 깊이 + 1
        InitSelectedDoors(doorGeneration);
        // 
        if(connectToParentDoor.gameObject.GetComponent<Door>() == null)
        {
            connectToParentDoor.gameObject.SetActive(false);
        }
        return connectToParentDoor;
    }

    public DoorEdge ConnectToChildRoom(RoomNode otherRoom)
    {
        // 선정된 문은 모두 방이 연결 될 여지가 있는 문들임.
        // 그 중에서 처음꺼부터 차례대로 연결.
        foreach(var door in SelectedDoors)
        {
            // 단 이미 연결된 문은 제외
            if(door.toRoomNode != null)
            {
                continue;
            }
            door.toRoomNode = otherRoom;
            return door;
        }
        return null;
    }
    // 방의 doordirection을 기준으로 맞는 문을 선택한다.
    public void InitSelectedDoors(DoorGeneration doorGeneration) 
    {
        SelectedDoors.Clear();
        
        // 처음 생성시 몇번동안 다른 방향으로 생성하지 않을지
        if(doorGeneration.currentInitialDirectionChangeDelay < doorGeneration.initialDirectionChangeDelay)
        {
            Debug.Log("처음 생성시 몇번동안 다른 방향으로 생성하지 않음");
            GenerateInSameDirection();
            doorGeneration.currentInitialDirectionChangeDelay++;
            return;
        }
        else if(doorGeneration.currentInitialDirectionChangeDelay == doorGeneration.initialDirectionChangeDelay)
        {
            Debug.Log("중간에서 방향을 한번 틀음 다른 방향으로 생성될 확률");
            GenerateInOtherDirection();
            doorGeneration.currentInitialDirectionChangeDelay = 9999;
            return;
        }

        // 다른 방향으로 생성될 확률
        if(doorGeneration.currentOtherDirectionProbability > UnityEngine.Random.value && doorGeneration.currentInitialDirectionChangeDelay > 0)
        {
            GenerateInOtherDirection();
        }
        else
        {   
            GenerateInSameDirection();
        }
        void GenerateInOtherDirection()
        {
            Debug.Log("다른 방향으로 생성될 확률");
            var tempDoorList = new List<DoorEdge>(doorList);
            tempDoorList.Remove(connectToParentDoor);
            // 다른 방향으로 생성될 확률
            foreach(var door in tempDoorList)
            {
               SelectedDoors.Add(door);
            }
            isOtherDirection = true;
            doorGeneration.currentOtherDirectionProbability = 0;
        }
        // 같은 방향으로 생성될 확률
        void GenerateInSameDirection()
        {
            Debug.Log("같은 방향으로 생성될 확률");
            foreach(var door in doorList)
            {
                if(door.direction == doorDirection)
                {
                    SelectedDoors.Add(door);
                }
            }
            var randomValue = UnityEngine.Random.Range(0, doorGeneration.directionProbabilityIncreaseRate);
            doorGeneration.currentOtherDirectionProbability = randomValue;
        }
    }

    // OLD 버전
    /// <summary>
    /// 방의 간선을 정렬하여 먼 문을 선택할지 정렬된 문을 선택할지 확률에 의해 결정
    /// </summary>
    // public void InitSelectedDoors() 
    // {
    //     // 중복 호출이 가능하다면 누적되지 않도록 매번 초기화
    //     SelectedDoors.Clear();

    //     // doorList를 복사한 뒤 사용하지 않을 문은 제외
    //     List<DoorEdge> sortedList = new List<DoorEdge>(doorList);
    //     sortedList.Remove(connectToParentDoor);                // 부모와 연결된 문 제거
    //     sortedList.RemoveAll(door => !door.gameObject.activeSelf); // 비활성 문 제거
    //     // 만약 이미 연결된 문을 제외하려면 아래 주석을 해제하세요
    //     // sortedList.RemoveAll(door => door.toRoomNode != null);

    //     // 남은 문이 없다면 그냥 종료
    //     if (sortedList.Count == 0) 
    //     {
    //         return;
    //     }

    //     // 남은 문이 하나뿐이면 그대로 선택 후 종료
    //     if (sortedList.Count == 1)
    //     {
    //         SelectedDoors.Add(sortedList[0]);
    //         return;
    //     }

    //     // 가장 먼 문이 앞으로 오도록 내림차순 정렬
    //     sortedList.Sort((a, b) =>
    //     {
    //         float distA = Vector3.Distance(RoomGenerator.instance.startRoomNode.transform.position, a.transform.position);
    //         float distB = Vector3.Distance(RoomGenerator.instance.startRoomNode.transform.position, b.transform.position);
    //         return distB.CompareTo(distA);
    //     });

    //     // 확률 계산을 좀 더 단순화
    //     bool isFarDoorArea = (Random.value < RoomGenerator.instance.farRoomProbability);
    //     bool isMultiDoor    = (Random.value < multipleDoorProbability);

    //     Debug.Log($"isFarDoorArea: {isFarDoorArea}, isMultiDoor: {isMultiDoor}");

    //     if (isFarDoorArea)
    //     {
    //         // '먼 문'을 우선해서 선택
    //         Debug.Log("먼 문을 우선해서 선택");
    //         if (isMultiDoor && sortedList.Count > 1)
    //         {
    //             // 여러 문을 사용할 확률일 때,
    //             // 일단 첫 번째 문(가장 먼 문)을 넣고,
    //             // 뒤에서부터 랜덤 개수만큼 추가
    //             int randomCount = Random.Range(1, sortedList.Count);
    //             SelectedDoors.Add(sortedList[0]); // 가장 먼 문
    //             SelectedDoors.AddRange(sortedList.GetRange(1, randomCount));
    //         }
    //         else
    //         {
    //             // '먼 문' 중에서 하나만 선택
    //             SelectedDoors.Add(sortedList[0]);
    //         }
    //     }
    //     else
    //     {
    //         // '그 외(가까운 쪽 포함)' 문을 우선해서 선택
    //         Debug.Log("가까운 문을 우선해서 선택");
    //         if (isMultiDoor && sortedList.Count > 1)
    //         {
    //             // 먼 문(0번을 제외한) 중에서 여러 개
    //             int randomCount = Random.Range(1, sortedList.Count);
    //             SelectedDoors.AddRange(sortedList.GetRange(1, randomCount));
    //         }
    //         else
    //         {
    //             // 여러 문을 쓰지 않을 때
    //             // 예: 가장 가까운 문을 하나만 선택하고 싶은 경우
    //             // sortedList[sortedList.Count - 1]가 가장 가까운 문
    //             if (sortedList.Count >= 2)
    //             {
    //                 SelectedDoors.Add(sortedList[sortedList.Count - 1]);
    //             }
    //             else
    //             {
    //                 // 사실상 2개 이하인 경우에는 0번(제일 먼 문)이라도 선택
    //                 SelectedDoors.Add(sortedList[0]);
    //             }
    //         }
    //     }
    // }
   
    public List<DoorEdge> GetUnconnectedDoors()
    {
        List<DoorEdge> unconnectedDoors = new List<DoorEdge>();
        foreach (var door in SelectedDoors)
        {
            if (door.toRoomNode == null)
            {
                Debug.Log($"RoomNode : {name} 에 있는 Unconnected Door : {door.name}");
                unconnectedDoors.Add(door);
            }
        }
        return unconnectedDoors;
    }
   // 이후 Todo
   // 예외 검사
   // 현재 하위에 있는 자식 노드들이 제대로 할당 되어 있는지 검사

   // 자동화
   // 현재 하위에 있는 자식 노드들을 할당 해주는 함수
   [ContextMenu("InitChildRoomList")]
    public void InitChildRoomList()
   {
        // 기존 리스트 초기화
        doorList.Clear();
        
        // Room의 모든 자식 오브젝트에서 Door 컴포넌트 검색
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(transform);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            foreach (Transform child in current)
            {
                DoorEdge door = child.GetComponent<DoorEdge>();
                if (door != null)
                {
                    doorList.Add(door);
                }
                queue.Enqueue(child);
            }
        }

        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
   }

   [ContextMenu("SetRoomGeneratorZones")]
   public void SetRoomGeneratorZones()
   {
        roomGeneratorZones.Clear();
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(transform);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            OtherGoundChecker checker = current.GetComponent<OtherGoundChecker>();
            if (checker != null)
            {
                roomGeneratorZones.Add(checker);
            }
            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
   }

   public void DetermineDirectionDoors()
   {
        foreach(var door in doorList)
        {
            door.DetermineDirection();
        }
   }

    public DoorDirection GetOppositeDirection(DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.North: return DoorDirection.South;
            case DoorDirection.South: return DoorDirection.North;
            case DoorDirection.East: return DoorDirection.West;
            case DoorDirection.West: return DoorDirection.East;
            case DoorDirection.Up: return DoorDirection.Down;
            case DoorDirection.Down: return DoorDirection.Up;
            default: return DoorDirection.North; // 기본값으로 North 반환
        }
    }
}
