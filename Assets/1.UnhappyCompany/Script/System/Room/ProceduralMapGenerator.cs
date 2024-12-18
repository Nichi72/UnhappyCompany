using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProceduralMapGenerator : MonoBehaviour {
    public GameObject startRoomPrefab;
    public GameObject[] roomPrefabs;
    public int additionalRoomsPerExtension = 3; // 추가 생성시 생성할 방 개수
    
    [System.Serializable]
    public class DoorGeneration {
        public string doorName = "Door";
        public int maxRooms = 3;
        public bool shouldGenerate = true;
    }
    
    public DoorGeneration[] doorGenerationSettings;
    
    private List<GameObject> roomInstances = new List<GameObject>();
    private bool isGenerating = false; // 현재 생성 중인지 확인하는 플래그
    private List<ConnectionPoint> endPoints = new List<ConnectionPoint>(); // 끝 부분 문들 저장

    void Start() 
    {
        StartCoroutine(GenerateRoom());
    }

    void Update()
    {
        // 생성 중이 아니고 스페이스바를 눌렀을 때
        if (!isGenerating && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(ExtendDungeon());
        }
    }

    // 끝 부분 문들 찾기
    void FindEndPoints()
    {
        endPoints.Clear();
        foreach (var room in roomInstances)
        {
            var doors = room.GetComponentsInChildren<ConnectionPoint>();
            foreach (var door in doors)
            {
                // 이 문이 다른 방과 연결되어 있는지 확인
                bool isConnected = false;
                foreach (var otherRoom in roomInstances)
                {
                    if (otherRoom == room) continue;
                    
                    var otherDoors = otherRoom.GetComponentsInChildren<ConnectionPoint>();
                    foreach (var otherDoor in otherDoors)
                    {
                        if (Vector3.Distance(door.transform.position, otherDoor.transform.position) < 0.1f)
                        {
                            isConnected = true;
                            break;
                        }
                    }
                    if (isConnected) break;
                }
                
                // 연결되지 않은 문이면 끝 부분으로 추가
                if (!isConnected)
                {
                    endPoints.Add(door);
                }
            }
        }
    }

    IEnumerator ExtendDungeon()
    {
        isGenerating = true;
        FindEndPoints();

        if (endPoints.Count > 0)
        {
            // 랜덤하게 끝 부분 문 선택
            int randomEndPointIndex = Random.Range(0, endPoints.Count);
            ConnectionPoint selectedEndPoint = endPoints[randomEndPointIndex];

            // 선택된 문에서 추가 방 생성
            yield return StartCoroutine(GenerateRoomsFromDoor(selectedEndPoint, additionalRoomsPerExtension));
        }

        isGenerating = false;
    }

    IEnumerator GenerateRoom()
    {
        isGenerating = true;
        
        // 시작 방 생성
        GameObject startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        roomInstances.Add(startRoom);
        
        var startDoors = startRoom.GetComponentsInChildren<ConnectionPoint>();
        
        if (doorGenerationSettings == null || doorGenerationSettings.Length != startDoors.Length)
        {
            doorGenerationSettings = new DoorGeneration[startDoors.Length];
            for (int i = 0; i < startDoors.Length; i++)
            {
                doorGenerationSettings[i] = new DoorGeneration();
            }
        }

        for (int doorIndex = 0; doorIndex < startDoors.Length; doorIndex++)
        {
            if (!doorGenerationSettings[doorIndex].shouldGenerate) continue;
            
            ConnectionPoint startDoor = startDoors[doorIndex];
            int maxRoomsForDoor = doorGenerationSettings[doorIndex].maxRooms;
            
            yield return StartCoroutine(GenerateRoomsFromDoor(startDoor, maxRoomsForDoor));
        }

        isGenerating = false;
        yield return null;
    }

    IEnumerator GenerateRoomsFromDoor(ConnectionPoint startDoor, int maxRoomsForDoor)
    {
        List<ConnectionPoint> currentBranchDoors = new List<ConnectionPoint> { startDoor };
        List<GameObject> branchRooms = new List<GameObject>();

        for(int i = 0; i < maxRoomsForDoor && currentBranchDoors.Count > 0; i++) 
        {
            // 현재 가능한 문 중 하나를 랜덤하게 선택
            int doorIndex = Random.Range(0, currentBranchDoors.Count);
            ConnectionPoint existingDoor = currentBranchDoors[doorIndex];
            currentBranchDoors.RemoveAt(doorIndex);

            // 새 방 생성
            GameObject roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
            GameObject newRoom = Instantiate(roomPrefab);
            ConnectionPoint newDoor = FindRandomConnectionPoint(newRoom);
            
            // 방 연결
            NewConnectRooms(existingDoor.gameObject.transform.parent.gameObject, newRoom, existingDoor, newDoor);
            
            // 방 겹침 체크
            Room roomComponent = newRoom.GetComponent<Room>();
            yield return new WaitForSeconds(0.5f);

            bool hasOverlap = false;
            foreach(var groundChecker in roomComponent.groundList)
            {
                if(groundChecker.isOverlap)
                {
                    while (!groundChecker.flagCheck)
                    {
                        yield return null;
                    }
                    hasOverlap = true;
                    break;
                }
            }

            if(hasOverlap)
            {
                Destroy(newRoom);
                currentBranchDoors.Insert(doorIndex, existingDoor);
                continue;
            }

            // 새 방의 다른 문들을 현재 브랜치의 사용 가능한 문 목록에 추가
            foreach(ConnectionPoint door in newRoom.GetComponentsInChildren<ConnectionPoint>()) 
            {
                if(door != newDoor)
                {
                    currentBranchDoors.Add(door);
                }
            }
            
            branchRooms.Add(newRoom);
            roomInstances.Add(newRoom);
        }
    }

    void InitRoom()
    {

    }

 

    ConnectionPoint FindRandomConnectionPoint(GameObject room) 
    {
        ConnectionPoint[] points = room.GetComponentsInChildren<ConnectionPoint>();
        int randomIndex = Random.Range(0, points.Length);
        
        // 선택되지 않은 문들 비활성화
        // for (int i = 0; i < points.Length; i++) {
        //     if (i != randomIndex) {
        //         points[i].gameObject.SetActive(false);
        //     }
        // }
        
        return points[randomIndex];
    }
    // 두 방을 주어진 두 문의 방향을 기준으로 이어붙이는 로직
    void NewConnectRooms(GameObject roomA, GameObject roomB, ConnectionPoint doorA, ConnectionPoint doorB) 
    {
        doorA.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        doorB.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
        
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

        Debug.Log("방 연결 완료: " + roomA.name + "와 " + roomB.name + "가 연결되었습니다.");

        void SetRoom()
        {
            Vector3 dir = doorA.transform.position - doorB.transform.position;
            Vector3 doorBPos = doorB.transform.localPosition;
            Vector3 offset = new Vector3(dir.x,roomB.transform.position.y,dir.z);
            roomB.transform.position += offset;
        }
    }
}
