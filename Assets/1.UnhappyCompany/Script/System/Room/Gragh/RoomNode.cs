using UnityEngine;
using System.Collections.Generic;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoomNode : MonoBehaviour
{
    public enum RoomType
    {
        KoreaRoom,
        BunkerRoom,
        OfficeRoom,
        WarehouseRoom,
    }
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

    public DoorEdge ConnectToParentRoom(RoomNode otherRoom)
    {
        var randomIndex = Random.Range(0, doorList.Count);
        connectToParentDoor = doorList[randomIndex];
        connectToParentDoor.toRoomNode = otherRoom;
        depth = otherRoom.depth + 1; // 부모 방의 깊이 + 1
        InitSelectedDoors();
        connectToParentDoor.gameObject.SetActive(false);
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

    /// <summary>
    /// 방의 간선을 정렬하여 먼 문을 선택할지 정렬된 문을 선택할지 확률에 의해 결정
    /// </summary>
    public void InitSelectedDoors()
    {
        if(doorList.Count == 1)
        {
            SelectedDoors.Add(doorList[0]);
            return;
        }
        List<DoorEdge> SortedList = new List<DoorEdge>(doorList);

        // SelectedDoors에 parentRoom이 들어있으면 삭제
        SortedList.Remove(connectToParentDoor);

        // 연결된 간선 제거 후 정렬
        SortedList.Sort((a, b) => {
            float distA = Vector3.Distance(RoomGenerator.instance.startRoomNode.transform.position, a.transform.position);
            float distB = Vector3.Distance(RoomGenerator.instance.startRoomNode.transform.position, b.transform.position);
            return distB.CompareTo(distA); // 내림차순 정렬 (먼 거리가 앞으로)
        });
    
        // 확률에 따라서 멀리 있는 Door에 방을 생성할지 정렬 
        float randomValue = Random.Range(0f, 1f);
        float multipleDoorRandomValue = Random.Range(0f, 1f);

        // 먼 문을 선택할 확률
        if(randomValue < RoomGenerator.instance.farRoomProbability) // 먼 문을 선택 
        {
            Debug.Log($"먼 문이 선택됨 - randomValue ({randomValue}) < farRoomProbability({RoomGenerator.instance.farRoomProbability})");
            // 생성될 문이 여러개일 확률
            if(multipleDoorRandomValue < multipleDoorProbability)
            {
                // 여러개의 방이 생성됨.
                int randomCount = Random.Range(1, SortedList.Count);
                if(SortedList.Count <= 1)
                {
                   SelectedDoors.Add(SortedList[0]);
                }
                else
                {
                   SelectedDoors.Add(SortedList[0]);
                   SelectedDoors.AddRange(SortedList.GetRange(1, randomCount));
                }
            }
            else
            {
                SelectedDoors.Add(SortedList[0]);
            }
        }
        else
        {
            Debug.Log("먼 문을 제외한 문이 선택됨");
            if(multipleDoorRandomValue < multipleDoorProbability)
            {
                if(SortedList.Count <= 1)
                {
                    SelectedDoors.Add(SortedList[0]);   
                }
                else
                {
                    // 여러개의 방이 생성됨.
                    int randomCount = Random.Range(1, SortedList.Count);
                    SelectedDoors.AddRange(SortedList.GetRange(1, randomCount));
                }
            }
            else
            {
                // 먼 문을 제외한 문 중에서 랜덤으로 선택
                if(SortedList.Count <= 1) // 근데 이미 먼 문을 제외한 문이 하나밖에 없으면 먼 문을 선택
                {
                    SelectedDoors.Add(SortedList[0]);
                }
                else
                {
                    SelectedDoors.Add(SortedList[Random.Range(1, SortedList.Count)]);
                }
            }
        }

        
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
        foreach (Transform child in transform)
        {
            DoorEdge door = child.GetComponent<DoorEdge>();
            if (door != null)
            {
                doorList.Add(door);
            }
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
   }

   [ContextMenu("SetRoomGeneratorZones")]
   public void SetRoomGeneratorZones()
   {
        var temp = GetComponent<OtherGoundChecker>();
        if(temp != null)
        {
            roomGeneratorZones.Add(temp);
        }
   }

    public List<DoorEdge> GetUnconnectedDoors()
    {
        List<DoorEdge> unconnectedDoors = new List<DoorEdge>();
        foreach (var door in SelectedDoors)
        {
            if (door.toRoomNode == null)
            {
                unconnectedDoors.Add(door);
            }
        }
        return unconnectedDoors;
    }
}
