using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    public RoomGenerator roomGenerator;
    public GameObject centerRoom;

    [Header("NavMesh 설정")]
    public NavMeshSurface navMeshSurface;
    private NavMeshData navMeshData;
    private bool isNavMeshBuilding = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        

    }

    /// <summary>
    /// NavMesh 데이터 초기화
    /// </summary>
    public void InitializeNavMesh()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface가 설정되지 않았습니다.");
            return;
        }

        navMeshData = new NavMeshData();
        navMeshSurface.navMeshData = navMeshData;
        NavMesh.AddNavMeshData(navMeshData);
    }

    /// <summary>
    /// NavMesh 비동기 빌드 시작
    /// </summary>
    public void BuildNavMeshAsync()
    {
        if (navMeshData == null)
        {
            InitializeNavMesh();
        }

        StartCoroutine(BuildNavMeshCoroutine());
    }

    /// <summary>
    /// RoomGenerator의 모든 방의 NavMesh 업데이트
    /// </summary>
    public void UpdateAllRoomsNavMeshAsync()
    {
        if (roomGenerator == null)
        {
            Debug.LogError("RoomGenerator가 설정되지 않았습니다.");
            return;
        }
        roomGenerator.allRoomList[0].roomSetting.navMeshSurface.BuildNavMesh();
        // StartCoroutine(UpdateRoomsNavMeshCoroutine());
    }

    private IEnumerator UpdateRoomsNavMeshCoroutine()
    {
        isNavMeshBuilding = true;
        Debug.Log("모든 방의 NavMesh 빌드 시작");

        // RoomGenerator의 roomList에서 모든 방 가져오기
        List<RoomNode> rooms = new List<RoomNode>(roomGenerator.allRoomList);
        
        // 시작 방도 포함
        if(roomGenerator.startRoomNode != null && !rooms.Contains(roomGenerator.startRoomNode))
        {
            rooms.Add(roomGenerator.startRoomNode);
        }

        int totalRooms = rooms.Count;
        int processedRooms = 0;

        foreach (RoomNode room in rooms)
        {
            if (room == null) continue;

            RoomSetting roomSetting = room.roomSetting;
            if (roomSetting == null || roomSetting.navMeshSurface == null)
            {
                Debug.LogWarning($"방 {room.name}에 RoomSetting 또는 NavMeshSurface가 없습니다.");
                continue;
            }

            // 각 방의 NavMesh 빌드
            roomSetting.navMeshSurface.BuildNavMesh();
            
            processedRooms++;
            Debug.Log($"NavMesh 빌드 진행률: {processedRooms}/{totalRooms} ({(float)processedRooms/totalRooms * 100:F1}%)");
            
            // 프레임 간 대기
            yield return null;
        }

        isNavMeshBuilding = false;
        Debug.Log("모든 방의 NavMesh 빌드 완료!");
    }

    private IEnumerator BuildNavMeshCoroutine()
    {
        isNavMeshBuilding = true;
        Debug.Log("NavMesh 비동기 빌드 시작");
        
        // 메인 스레드에서 NavMesh 소스 수집
        navMeshSurface.BuildNavMesh();
        
        yield return null;
        
        isNavMeshBuilding = false;
        Debug.Log("NavMesh 비동기 빌드 완료!");
    }
}
