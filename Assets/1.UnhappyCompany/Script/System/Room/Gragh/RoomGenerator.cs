using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
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
    public class RoomTypeGeneration
    {
        public RoomNode.RoomType roomType;
        public List<RoomRandomGeneration> roomPrefabList = new List<RoomRandomGeneration>();
    }
    [System.Serializable]
    public class DoorRetryInfo
    {
        public DoorEdge door;
        public int failCount = 0;
        public float lastTryTime = 0f;
    }
    public static RoomGenerator instance;
    [ReadOnly] public List<RoomNode> roomList = new List<RoomNode>(); // 생성된 방 리스트
    public RoomNode startRoomNode;// 시작 방
    public DoorGeneration[] doorGenerationSettings; // 시작방에서 시작 문 선택
    [SerializeField] private float createRoomTime = 1f;
    public bool isGenerating = false;

    public RoomTypeGeneration[] roomTypeGenerationSettingsForSmallRoom;
    public RoomTypeGeneration[] roomTypeGenerationSettingsForStairRoom;

    [Header("==== 밸런스 처리 ====")]
    [Header("각각의 방이 생성 될 확률(!!반드시!!총합이 1이 되어야함)")]
    public RoomTypeGeneration[] roomTypeGenerationSettings;
    [Header("생성 할 방의 개수")]
    public int roomCountFirstTime = 10; // 처음에 생성 될 방의 개수
    [Header("깊이를 반복 할 횟수")]
    public int depthCount = 10; // 깊이를 반복 할 횟수
    [Header("깊이를 반복 할 때마다 생성 될 방의 개수")]
    public int roomCountPerDepth = 6; // 깊이를 반복 할 때마다 생성 될 방의 개수
    [Header("더 먼곳에 있는 방을 먼저 생성할 확률을 조절하는 함수")]
    public float farRoomProbability = 0.5f; // 더 먼곳에 있는 방을 먼저 생성할 확률을 조절하는 함수
    [Header("계단을 생성할 확률을 조절하는 함수")]
    public float stairProbability = 0.5f; // 계단을 생성할 확률을 조절하는 함수
    [Header("방 생성 시도 횟수")]
    public int maxTryCount = 10; // 방 생성 시도 횟수
    [Header("문 재시도 설정")]
    [Tooltip("실패한 문을 다시 큐에 추가할 확률 (0-1)")]
    public float doorRetryProbability = 0.5f;
    [Tooltip("실패 횟수에 따른 재시도 확률 감소 계수 (0-1)")]
    public float failPenaltyFactor = 0.1f;
    [Tooltip("문 재시도 간 최소 대기 시간 (초)")]
    public float doorRetryDelay = 5.0f;
    [Tooltip("최대 실패 허용 횟수, 이 이상 실패하면 문은 영구히 제외됨")]
    public int maxDoorFailCount = 3;

    private Dictionary<DoorEdge, DoorRetryInfo> doorRetryData = new Dictionary<DoorEdge, DoorRetryInfo>();
    private float gameStartTime;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        gameStartTime = Time.time;
        doorRetryData.Clear();
    }   
    void Start()
    {
        StartCoroutine(GenerateRoomFirstTime());
    }

    // Update is called once per frame
    void Update()
    {
       
    }
   
    private IEnumerator GenerateRoomFirstTime()
    {
        foreach(var startDoor in doorGenerationSettings)
        {
            if(startDoor.shouldGenerate)
            {
                // 일단 방 생성
                var newRoom = Instantiate(GetRoomNodePrefab(RoomNode.RoomType.HospitalRoom));
                newRoom.depth = 0; // 시작 방의 깊이를 0으로 설정
                
                // 겹침 체크 전에 대기
                yield return new WaitForFixedUpdate();
                yield return new WaitForSeconds(createRoomTime);
                
                // 겹침 체크
                if (newRoom.isOverlap)
                {
                    Debug.LogWarning("초기 방 생성 중 겹침이 발생했습니다. 다음 문으로 넘어갑니다.");
                    Destroy(newRoom.gameObject);
                    continue;
                }
                yield return new WaitForFixedUpdate();
                yield return new WaitForSeconds(createRoomTime);
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

                // 첫 번째 방 생성
                RoomNode.RoomType randomRoomType = (RoomNode.RoomType)Random.Range(0, System.Enum.GetValues(typeof(RoomNode.RoomType)).Length);
                StartCoroutine(GenerateRoom(newRoom, roomCountFirstTime));
            }
        }
    }
    private void ExpandRoom()
    {
        List<int> tempRoomList = new List<int>();
      
        for(int i = 0; i < depthCount; i++)
        {
            if (roomList.Count > 0)
            {
                // roomList에서 랜덤으로 방을 선택
                int randomIndex = Random.Range(0, roomList.Count);
                if(tempRoomList.Contains(randomIndex))
                {
                    continue;
                }
                RoomNode roomToExpand = roomList[randomIndex];
                tempRoomList.Add(randomIndex);
                
                // 선택된 방을 기준으로 확장
                StartCoroutine(GenerateRoom(roomToExpand, roomCountPerDepth));
            }
        }
    }

    private IEnumerator GenerateRoom(RoomNode startRoomNode, int maxRoomCount)
    {
        isGenerating = true;
        yield return new WaitForSeconds(2f);

        Queue<DoorEdge> doorQueue = new Queue<DoorEdge>();
        List<DoorEdge> failedDoorList = new List<DoorEdge>(); // 실패한 문들을 저장할 리스트
        Queue<RoomNode> roomQueue = new Queue<RoomNode>();
        List<RoomNode> createdRooms = new List<RoomNode>(); // 이번 생성에서 추가된 방들

        // 시작 방의 문을 큐에 추가
        foreach (var door in startRoomNode.GetUnconnectedDoors())
        {
            doorQueue.Enqueue(door);
        }

        // 시작 방을 큐에 추가
        roomQueue.Enqueue(startRoomNode);

        int currentRoomCount = 0;
        int totalAttempts = 0; // 전체 시도 횟수 (무한 루프 방지용)
        int maxTotalAttempts = maxRoomCount * 3; // 최대 전체 시도 횟수

        // doorQueue가 비어있어도 목표 방 개수에 도달하지 못했다면 계속 시도
        while (currentRoomCount < maxRoomCount && totalAttempts < maxTotalAttempts)
        {
            totalAttempts++;
            
            // 문 큐가 비었는데 방 개수가 부족하면 모든 방의 연결되지 않은 문을 다시 수집
            if (doorQueue.Count == 0)
            {
                Debug.Log("문 큐가 비었습니다. 모든 방의 연결되지 않은 문을 다시 수집합니다.");
                
                // 이미 생성된 모든 방들(시작 방 + 새로 생성된 방들)에서 연결되지 않은 문 수집
                foreach (var room in roomQueue)
                {
                    var unusedDoors = room.GetUnconnectedDoors();
                    foreach (var door in unusedDoors)
                    {
                        doorQueue.Enqueue(door);
                    }
                }
                
                // 만약 문이 없다면 더 이상 생성 불가능
                if (doorQueue.Count == 0)
                {
                    Debug.LogWarning($"더 이상 사용 가능한 문이 없습니다. 방 {currentRoomCount}/{maxRoomCount}개만 생성했습니다.");
                    break;
                }
                
                yield return new WaitForSeconds(0.5f);
            }

            DoorEdge beforeDoor = doorQueue.Dequeue();
            RoomNode beforeRoom = beforeDoor.formRoomNode;

            bool roomCreated = false;
            int tryCount = 0;

            while (!roomCreated && tryCount < maxTryCount)
            {
               
                // 첫 번째 방 생성
                // RoomNode.RoomType randomRoomType = (RoomNode.RoomType)Random.Range(0, System.Enum.GetValues(typeof(RoomNode.RoomType)).Length);
                // 새 RoomNode 생성 및 연결
                RoomNode.RoomType randomRoomType = RoomNode.RoomType.HospitalRoom;
                RoomNode newRoom = Instantiate(GetRoomNodePrefab(randomRoomType));
                // newRoom.transform.position = beforeRoom.transform.position + new Vector3(0,1000,0);
                newRoom.ConnectToParentRoom(beforeRoom);
                DoorEdge childDoor = beforeRoom.ConnectToChildRoom(newRoom);
                
                ConnectRooms(newRoom.gameObject, childDoor, newRoom.connectToParentDoor);

                yield return new WaitForFixedUpdate();
                yield return new WaitForSeconds(createRoomTime);

                if (!newRoom.isOverlap)
                {
                    roomQueue.Enqueue(newRoom);
                    createdRooms.Add(newRoom); // 생성된 방 추가
                    currentRoomCount++;
                    
                    // 새롭게 생성된 방의 문을 큐에 추가
                    foreach (var door in newRoom.GetUnconnectedDoors())
                    {
                        doorQueue.Enqueue(door);
                    }

                    roomCreated = true; // 겹치지 않았으므로 방 생성 성공
                    Debug.Log($"방 생성 성공: {currentRoomCount}/{maxRoomCount}");
                }
                else
                {
                    // 겹쳤다면 방 연결 취소 후 방 제거
                    Debug.Log($"겹쳤다면 방 연결 취소 후 방 제거 {tryCount + 1} 번째 시도");
                    CancelConnectRooms(beforeDoor, childDoor);
                    Destroy(newRoom.gameObject);

                    tryCount++;
                }
            }
            
            // 최대 시도 횟수를 초과했다면, 해당 문을 다시 큐에 넣을지 결정
            if (!roomCreated && tryCount >= maxTryCount)
            {
                Debug.Log($"문 {beforeDoor.name}에서 최대 시도 횟수를 초과했습니다.");
                
                // 문 재시도 데이터 업데이트
                if (!doorRetryData.ContainsKey(beforeDoor))
                {
                    doorRetryData.Add(beforeDoor, new DoorRetryInfo { door = beforeDoor });
                }
                
                DoorRetryInfo retryInfo = doorRetryData[beforeDoor];
                retryInfo.failCount++;
                retryInfo.lastTryTime = Time.time;
                
                // 실패 횟수가 최대 허용치를 넘지 않았고, 적절한 확률로 재시도가 결정된 경우
                float adjustedProbability = doorRetryProbability - (retryInfo.failCount * failPenaltyFactor);
                adjustedProbability = Mathf.Clamp01(adjustedProbability); // 0-1 사이로 제한
                
                if (retryInfo.failCount < maxDoorFailCount && Random.value < adjustedProbability)
                {
                    Debug.Log($"문 {beforeDoor.name}을(를) 나중에 다시 시도하기 위해 실패 목록에 추가합니다. (실패 횟수: {retryInfo.failCount}, 재시도 확률: {adjustedProbability:P0})");
                    failedDoorList.Add(beforeDoor); // 실패 목록에 추가
                }
                else
                {
                    Debug.Log($"문 {beforeDoor.name}은(는) 실패 횟수({retryInfo.failCount})가 너무 많거나 확률({adjustedProbability:P0})에 의해 재시도하지 않습니다.");
                }
            }
            
            // 큐가 비었는데 실패 목록에 문이 있고, 충분한 방이 생성되지 않았으면 실패한 문 중 일부를 다시 시도
            if (doorQueue.Count == 0 && failedDoorList.Count > 0 && currentRoomCount < maxRoomCount)
            {
                Debug.Log($"문 큐가 비었습니다. 실패한 문 중 일부를 다시 시도합니다. (실패 문 개수: {failedDoorList.Count})");
                
                // 실패 목록에서 시간이 충분히 지난 문들을 필터링
                float currentTime = Time.time;
                List<DoorEdge> eligibleDoors = failedDoorList.Where(door => 
                    currentTime - doorRetryData[door].lastTryTime >= doorRetryDelay).ToList();
                
                if (eligibleDoors.Count > 0)
                {
                    // 실패 횟수가 적은 문부터 정렬 (우선 시도)
                    eligibleDoors.Sort((a, b) => doorRetryData[a].failCount.CompareTo(doorRetryData[b].failCount));
                    
                    // 최대 3개의 문만 다시 큐에 추가 (너무 많은 재시도 방지)
                    int doorToRetry = Mathf.Min(3, eligibleDoors.Count);
                    for (int i = 0; i < doorToRetry; i++)
                    {
                        doorQueue.Enqueue(eligibleDoors[i]);
                        failedDoorList.Remove(eligibleDoors[i]);
                        Debug.Log($"문 {eligibleDoors[i].name}을(를) 다시 큐에 추가합니다. (실패 횟수: {doorRetryData[eligibleDoors[i]].failCount})");
                    }
                    
                    // 잠시 대기하여 재시도 과정 시각화 
                    yield return new WaitForSeconds(0.5f);
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }

        // 목표 방 개수에 도달했는지 확인
        if (currentRoomCount < maxRoomCount)
        {
            Debug.LogWarning($"목표 방 개수({maxRoomCount})에 도달하지 못했습니다. 현재 생성된 방: {currentRoomCount}개");
        }
        else
        {
            Debug.Log($"목표 방 개수({maxRoomCount})에 성공적으로 도달했습니다!");
        }

        // 문이 남아 있는 방만 roomList에 추가
        foreach (var room in roomQueue)
        {
            if (room.GetUnconnectedDoors().Count > 0 && !roomList.Contains(room))
            {
                roomList.Add(room);
            }
        }
        
        isGenerating = false;
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
            // doorA.GetComponent<MeshRenderer>().material.color = Color.red;
            // doorB.GetComponent<MeshRenderer>().material.color = Color.blue;
            roomB.transform.position = new Vector3(0,0,0);
            // 문 A와 문 B의 방향 벡터
            Vector3 doorADir = doorA.transform.rotation.eulerAngles;
            Vector3 doorBDir = doorB.transform.rotation.eulerAngles;
            // Debug.Log($"문 A의 방향: {doorADir}, 문 B의 방향: {doorBDir}");
            
            // 두 문의 Y축 회전 차이 계산
            float rotationDifference = Mathf.DeltaAngle(doorADir.y, doorBDir.y);
            // Debug.Log($"두 문의 회전 차이: {rotationDifference}도");
            
            const float ANGLE_TOLERANCE = 5f; // 허용 오차 범위
            float absDifference = Mathf.Abs(rotationDifference); // 절대값 계산
            
            if(absDifference <= ANGLE_TOLERANCE || Mathf.Abs(absDifference - 360) <= ANGLE_TOLERANCE)
            {
                Debug.Log("회전값이 같은 경우 (0도 또는 360도)");
                roomB.transform.Rotate(0, 180, 0);
                SetRoom();
            }
            else if (Mathf.Abs(absDifference - 180) <= ANGLE_TOLERANCE)
            {
                Debug.Log("회전값이 180도 차이나는 경우 (180도 또는 -180도)");
                SetRoom();
            }
            else if (Mathf.Abs(absDifference - 90) <= ANGLE_TOLERANCE || Mathf.Abs(absDifference - 270) <= ANGLE_TOLERANCE)
            {
                Debug.Log("회전값이 90도 차이나는 경우 (90도 또는 -90도 또는 270도 또는 -270도)");
                if (rotationDifference > 0)
                {
                    roomB.transform.Rotate(0, 90, 0);
                }
                else
                {
                    roomB.transform.Rotate(0, -90, 0);
                }
                SetRoom();
            }
            else
            {
                Debug.LogError($"회전값이 다른 경우 {rotationDifference} (절대값: {absDifference})");
                Debug.Break();
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
            // Vector3 offset = new Vector3(dir.x,roomB.transform.position.y,dir.z);
            Vector3 offset = new Vector3(dir.x,dir.y,dir.z);

            roomB.transform.position += offset;
        }
    }
    private RoomNode GetStairRoomNodePrefab(RoomNode.RoomType roomType)
    {
         foreach(var roomTypeGeneration in roomTypeGenerationSettingsForStairRoom)
        {
            if(roomTypeGeneration.roomType == roomType)
            {
                return roomTypeGeneration.roomPrefabList[0].room;
            }
        }
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
  
    [ContextMenu("ExpandRoomForTest")]
    public void ExpandRoomForTest()
    {
        ExpandRoom();
    }
}

 

