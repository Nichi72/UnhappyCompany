using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Random = UnityEngine.Random;
public class RoomGenerator : MonoBehaviour
{
    [System.Serializable]
    public class DoorGeneration {
        public DoorEdge door;
        public bool shouldGenerate = true;
        public int stair = 0;
    }
    [System.Serializable]
    public class RoomRandomGeneration {
        public RoomNode room;
        public float probability = 0.5f;
    }
    [System.Serializable]
    public class RoomTypeGeneration{
        public RoomNode.RoomType roomType;
        public List<RoomRandomGeneration> roomPrefabList = new List<RoomRandomGeneration>();
    }
    public static RoomGenerator instance;
    
    [ReadOnly] public List<RoomNode> roomList = new List<RoomNode>(); // 생성된 방 리스트
    public RoomNode startRoomNode;// 시작 방
    public DoorGeneration[] doorGenerationSettings; // 시작방에서 시작 문 선택

    [SerializeField] private float createRoomTime = 1f;

    public RoomTypeGeneration[] roomTypeGenerationSettingsForSmallRoom;
    public RoomTypeGeneration[] roomTypeGenerationSettingsForStairRoom;

    [Header("==== 밸런스 처리 ====")]
    [Header("각각의 방이 생성 될 확률(!!반드시!!총합이 1이 되어야함)")]
    public RoomTypeGeneration[] roomTypeGenerationSettings;
    [Header("생성 할 방의 개수")]
    public int roomCountFirstTime = 10; // 처음에 생성 될 방의 개수
    [Header("깊이를 반복 할 때마다 생성 될 방의 개수")]
    public int roomCountPerDepth = 6; // 깊이를 반복 할 때마다 생성 될 방의 개수
    [Header("더 먼곳에 있는 방을 먼저 생성할 확률을 조절하는 함수")]
    public float farRoomProbability = 0.5f; // 더 먼곳에 있는 방을 먼저 생성할 확률을 조절하는 함수
    [Header("계단을 생성할 확률을 조절하는 함수")]
    public float stairProbability = 0.5f; // 계단을 생성할 확률을 조절하는 함수


    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }   
    void Start()
    {
        GenerateRoomFirstTime();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void InitStartRoom()
    {
        // 시작 방 생성
    }
    void GenerateRoomFirstTime()
    {
        foreach(var startDoor in doorGenerationSettings)
        {
            if(startDoor.shouldGenerate)
            {
                // 일단 방 생성
                var newRoom = Instantiate(GetRoomNodePrefab(RoomNode.RoomType.KoreaRoom));
                // 부모 노드를 할당 해주고 부모와 연결 할 문을 선택
                newRoom.ConnectToParentRoom(startRoomNode);
                startRoomNode.ConnectToChildRoom(newRoom);
                // 두 문을 연결 해줌
                ConnectRooms(newRoom.gameObject, startDoor.door, newRoom.connectToParentDoor);
                // doorGenerationSettings를 셔플
                var tempList = new List<DoorGeneration>(doorGenerationSettings);
                for (int i = tempList.Count - 1; i > 0; i--)
                {
                    int randomIndex = Random.Range(0, i + 1);
                    var temp = tempList[i];
                    tempList[i] = tempList[randomIndex];
                    tempList[randomIndex] = temp;
                }
                doorGenerationSettings = tempList.ToArray();
                doorGenerationSettings[0].stair = -1;
                doorGenerationSettings[1].stair = 0;
                doorGenerationSettings[2].stair = 1;
                doorGenerationSettings[3].stair = 2;
                StartCoroutine(GenerateRoom(newRoom, roomCountFirstTime,RoomNode.RoomType.KoreaRoom));
            }
        }
    }
    private IEnumerator GenerateRoom(RoomNode startRoomNode, int maxRoomCount,RoomNode.RoomType roomType)
    {
        /// 이 알고리즘은 BFS 방식으로 기존 방에서 새 방을 확장해 나가는 절차를 정의한다.
    /// 
    /// 1. 이미 생성된 방(기존 방)들이 존재한다고 가정한다.
    ///    - 각 기존 방에는 아직 연결되지 않은 문들이 있을 수 있다.
    /// 
    /// 2. "생성 가능한 문"을 큐(Queue)에 넣는다.
    ///    - "생성 가능한 문"이란 아직 연결될 방이 없고(toRoomNode가 null), 
    ///      새로운 방을 만들 수 있는 상태의 문을 의미한다.
    /// 
    /// 3. 큐에서 문(Door)을 하나씩 꺼내 처리한다.
    ///    - 큐에서 꺼낸 문이 '방문(처리)되지 않은 상태'(toRoomNode == null)라면 다음을 수행한다:
    ///      a. 새로운 방(RoomNode)을 생성한다.
    ///      b. 해당 문(Door)의 toRoomNode에 새로 만든 방을 할당한다. (toRoomNode != null)
    ///      c. 이렇게 하면 이 문은 '처리 완료(visited)' 상태가 되며, 
    ///         동시에 새로운 방이 기존 방 목록에 추가된다.
    /// 
    /// 4. 새롭게 생성된 방의 문들 중, 아직 연결되지 않은 "생성 가능한 문"들을 다시 큐에 삽입한다.
    ///    - 이를 통해 점차 주변으로 방을 확장해나갈 수 있다.
    /// 
    /// 5. 큐가 빌 때까지 3~4 단계를 반복한다.
    ///    - 큐가 비었다는 것은 더 이상 새로 생성할 방이 없음을 의미한다.
    /// 
    /// 6. 모든 과정이 끝나면, 기존 방들과 새로 생성된 방들이 연결된 하나의 그래프(미로)가 형성된다.
    /// 
    /// 정리하자면,
    /// - 시작점: 이미 생성된 방의 "생성 가능한 문들"
    /// - BFS 큐: 아직 처리되지 않은 문들
    /// - 방문(처리) 기준: 문에 연결된 toRoomNode(새로운 방)의 존재 여부
    /// - 종료 조건: 더 이상 처리할 문이 없음(큐가 빔)
    /// 
    /// 이를 통해 단계별로 새로운 방을 생성하고 연결함으로써 
    /// 확장 가능한 그래프(미로)를 구성할 수 있다.
    /// 


    /// 방을 생성 
    /// 방에 문에 부모와 연결된 문을 제외한 나머지 문을 큐에 넣어
    /// 문을 하나 뽑아.
    /// 그 문을 기준으로 방을 생성하고 연결해.
    /// 
    /// 그 방에 문에 부모와 연결된 문을 제외한 나머지 문을 큐에 넣어.
    /// 문을 하나 뽑아.
    /// 그 문을 기준으로 방을 생성하고 연결해.
    /// 계속 반복해
    /// 
        yield return new WaitForSeconds(2f);
        /// 생성 될 문을 넣어둠
        // Queue<RoomNode> roomQueue = new Queue<RoomNode>();
        Queue<DoorEdge> doorQueue = new Queue<DoorEdge>();
        foreach(var door in startRoomNode.SelectedDoors)
        {
            if(door.toRoomNode == null)
            {
                doorQueue.Enqueue(door);
            }
        }

        // roomQueue.Enqueue(startRoomNode);
        
        int currentRoomCount = 0;
        /// 최대 방 개수를 넘어가면 종료
        while(0 < doorQueue.Count && currentRoomCount < maxRoomCount)
        {
            // 현재 방의 룸 개수만큼 처리
            int currentDoorCount = doorQueue.Count;

            
            for(int i = 0; i < currentDoorCount; i++)
            {
                DoorEdge beforedoor = doorQueue.Dequeue();
                RoomNode beforeRoom = beforedoor.formRoomNode;
                DoorEdge childDoor = null;
                RoomNode newRoom = null;
                Debug.Log($"currentRoom{beforedoor.formRoomNode.name}");
                // 새로운 방을 생성
                newRoom = Instantiate(GetRoomNodePrefab(roomType));
                newRoom.ConnectToParentRoom(beforeRoom);
                childDoor = beforeRoom.ConnectToChildRoom(newRoom);
                currentRoomCount++;
                ConnectRooms(newRoom.gameObject,childDoor ,newRoom.connectToParentDoor);

                
                // 문에 맞게끔 기본적인 회전 및 위치를 처리.
                // 겹치는거 감지하는 처리
                // 만약 Room에 겹치는것이 감지 되지 않는다면? => 다음 단계로 넘어감
                yield return new WaitForFixedUpdate();
                yield return new WaitForSeconds(createRoomTime);
                if(!newRoom.isOverlap)
                {
                    var temp = newRoom.SelectedDoors;
                    foreach(var door in temp)
                    {
                        if(door.toRoomNode == null)
                        {
                            doorQueue.Enqueue(door);
                        }
                    }
                }
                else // 겹치는거 감지 된다면?
                {
                    Debug.Log("@@@겹치는거 감지 됨");
                    CancelConnectRooms(beforedoor,childDoor); // 연결한것들 취소
                    Destroy(newRoom.gameObject);
                    RoomNode replacementRoom = null;
                    if(Random.value < stairProbability)
                    {
                        Debug.Log("@@@계단 생성");
                        // 계단 생성
                    }
                    else
                    {
                        // 더 작은 방으로 교체 시도
                        replacementRoom = Instantiate(GetSmallRoomNodePrefab(roomType));
                    }



                    newRoom = replacementRoom;
                    newRoom.ConnectToParentRoom(beforeRoom);
                    childDoor = beforeRoom.ConnectToChildRoom(newRoom);
                    ConnectRooms(newRoom.gameObject,childDoor ,newRoom.connectToParentDoor);

                    // 작은 방도 겹치면 연결 취소
                    yield return new WaitForFixedUpdate();
                    yield return new WaitForSeconds(createRoomTime);

                    if(newRoom.isOverlap)
                    {
                        CancelConnectRooms(beforedoor, childDoor);
                        Destroy(newRoom.gameObject);
                        currentRoomCount--;
                    }
                    yield return new WaitForFixedUpdate();
                    yield return new WaitForSeconds(createRoomTime);
                }
            }
            yield return new WaitForSeconds(createRoomTime);
        }
        yield return null;
    }
    private void CancelConnectRooms(DoorEdge parentDoor, DoorEdge childDoor)
    {
        parentDoor.toRoomNode = null;
        childDoor.toRoomNode = null;
    }
    private void ConnectRooms(GameObject roomB, DoorEdge doorA, DoorEdge doorB) 
    {
        try
        {
            doorA.GetComponent<MeshRenderer>().material.color = Color.red;
            doorB.GetComponent<MeshRenderer>().material.color = Color.blue;

            Debug.Log($"디버그 roomB {roomB.name}");
            Debug.Log($"디버그 doorA {doorA.name}");
            Debug.Log($"디버그 doorB {doorB.name}");
            
            // 문 A와 문 B의 방향 벡터
            Vector3 doorADir = doorA.transform.rotation.eulerAngles;
            Vector3 doorBDir = doorB.transform.rotation.eulerAngles;
            Debug.Log($"문 A의 방향: {doorADir}, 문 B의 방향: {doorBDir}");
            
            // 두 문의 Y축 회전 차이 계산
            float rotationDifference = Mathf.DeltaAngle(doorADir.y, doorBDir.y);
            Debug.Log($"두 문의 회전 차이: {rotationDifference}도");
            if((int)rotationDifference == 0)
            {
                Debug.Log("회전값이 같은 경우");
                roomB.transform.Rotate(0, 180, 0);
                SetRoom();
            }
            else if ((int)rotationDifference == 180)
            {
                Debug.Log("회전값이 180도 차이나는 경우");
                SetRoom();
            }
            else if ((int)rotationDifference == 90)
            {
                Debug.Log("회전값이 90도 차이나는 경우");
                roomB.transform.Rotate(0, 90, 0);
                SetRoom();
            }
            else if ((int)rotationDifference == -90)
            {
                Debug.Log("회전값이 -90도 차이나는 경우");
                roomB.transform.Rotate(0, -90, 0);
                SetRoom();
            }


            Debug.Log("방 연결 완료: " + roomB.name + "가 연결되었습니다.");
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
        void SetRoom()
        {
            Vector3 dir = doorA.transform.position - doorB.transform.position;
            Vector3 doorBPos = doorB.transform.localPosition;
            Vector3 offset = new Vector3(dir.x,roomB.transform.position.y,dir.z);
            roomB.transform.position += offset;
        }
    }
    private RoomNode GetStairRoomNodePrefab(RoomNode.RoomType roomType)
    {
        return null;
    }
    private RoomNode GetSmallRoomNodePrefab(RoomNode.RoomType roomType)
    {
        foreach(var roomTypeGeneration in roomTypeGenerationSettingsForSmallRoom)
        {
            if(roomTypeGeneration.roomType == roomType)
            {
                return roomTypeGeneration.roomPrefabList[0].room;
            }
        }
        return null;
    }
    private RoomNode GetRoomNodePrefab(RoomNode.RoomType roomType)
    {
        // 해당 룸타입의 프리팹 리스트 찾기
        List<RoomRandomGeneration> RoomRandomGenerationTemp = null;
        foreach(var roomTypeGeneration in roomTypeGenerationSettings)
        {
            if(roomTypeGeneration.roomType == roomType)
            {
                RoomRandomGenerationTemp = roomTypeGeneration.roomPrefabList;
                break;
            }
        }
        
        // 예외처리
        if (RoomRandomGenerationTemp == null || RoomRandomGenerationTemp.Count == 0)
        {
            Debug.LogError($"No room prefabs found for type: {roomType}");
            return null;
        }
        
        // 총합이 1인지 확인
        float totalProbability = 0f;
        foreach(var room in RoomRandomGenerationTemp)
        {
            totalProbability += room.probability;
        }
        if(Mathf.Abs(totalProbability - 1f) > 0.001f)
        {
            Debug.LogError($"Total probability for {roomType} rooms is {totalProbability}, should be 1!");
            return null;
        }
        
        // 확률에 따라 방 프리팹 선택
        float randomValue = Random.Range(0f, 1f);
        float currentProbability = 0f;
        
        foreach(var room in RoomRandomGenerationTemp) 
        {
            currentProbability += room.probability;
            if(randomValue <= currentProbability) 
            {
                return room.room;
            }
        }
        
        // 마지막 방 반환 (부동소수점 오차 처리를 위해)
        return RoomRandomGenerationTemp[RoomRandomGenerationTemp.Count-1].room;
    }
  
}

