using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public List<GameObject> roomPrefabs;
    public int maxRooms = 5;

    private List<GameObject> spawnedRooms = new List<GameObject>();
    private Queue<ConnectionPoint> openPoints = new Queue<ConnectionPoint>();

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (roomPrefabs.Count == 0) return;

        // 시작 방 생성
        GameObject startRoom = Instantiate(roomPrefabs[0], Vector3.zero, Quaternion.identity);
        spawnedRooms.Add(startRoom);

        // 시작 방의 ConnectionPoint를 큐에 등록
        foreach (var cp in startRoom.GetComponentsInChildren<ConnectionPoint>())
        {
            openPoints.Enqueue(cp);
        }

        while (spawnedRooms.Count < maxRooms && openPoints.Count > 0)
        {
            ConnectionPoint fromPoint = openPoints.Dequeue();
            if (!fromPoint.CanExpand()) continue;

            // 새로운 방 프리팹 랜덤 선택
            GameObject newRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];

            // fromPoint에 맞는 도어를 가진 방을 배치
            if (TryPlaceRoom(newRoomPrefab, fromPoint, out GameObject newRoom))
            {
                spawnedRooms.Add(newRoom);
                // 새 방의 CP들을 큐에 추가
                foreach (var cp in newRoom.GetComponentsInChildren<ConnectionPoint>())
                {
                    if (cp.CanExpand()) openPoints.Enqueue(cp);
                }
            }
        }
    }

    private bool TryPlaceRoom(GameObject prefab, ConnectionPoint fromPoint, out GameObject newRoom)
    {
        newRoom = null;
        ConnectionPoint.Direction neededDir = ConnectionPoint.GetOppositeDirection(fromPoint.connectionDirection);

        // 대상 프리팹에서 neededDir 방향을 가진 문(포인트) 찾기
        ConnectionPoint targetPoint = null;
        foreach (var cp in prefab.GetComponentsInChildren<ConnectionPoint>())
        {
            if (cp.connectionDirection == neededDir && cp.CanExpand())
            {
                targetPoint = cp;
                break;
            }
        }

        if (targetPoint == null) 
            return false; // 맞는 문이 없으면 실패

        // 문을 맞추기 위한 회전/이동  
        // 1) fromPoint의 forward 방향과 targetPoint의 forward 방향을 일치시켜야 함
        //    하지만 targetPoint는 반대 방향을 보고 있어야 함(문끼리 맞대는 상태)
        //    예: if fromPoint.direction = East, targetPoint.direction = West일 때,
        //    fromPoint의 forward 방향(동쪽)을 바라보도록 newRoom을 회전시켜 targetPoint가 서쪽을 향하게 함.
        
        // 미리 인스턴스화 해서 조정할 수도 있지만, 우선 Instantiate 후 위치/회전 조정
        newRoom = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // 회전 정렬:
        // fromPoint가 바라보는 방향과 targetPoint가 반대 방향을 바라보게 해야함
        // targetPoint.transform.forward가 neededDir에 따라 정해지므로,
        // newRoom 전체를 회전시켜 targetPoint가 fromPoint를 바라보게 한다.
        AlignDoorDirection(fromPoint, targetPoint, newRoom);

        // 위치 맞추기:
        // targetPoint 문이 fromPoint 문의 위치와 정확히 일치하게 이동
        Vector3 offset = fromPoint.transform.position - targetPoint.transform.position;
        newRoom.transform.position += offset;

        // 여기서 충돌 체크 가능 (OverlapBox 등)
        // 충돌 발생 시 GameObject.Destroy(newRoom); return false;
        
        // 연결 완료 처리
        fromPoint.isConnected = true;
        targetPoint.isConnected = true;

        return true;
    }

    private void AlignDoorDirection(ConnectionPoint fromPoint, ConnectionPoint targetPoint, GameObject newRoom)
    {
        // fromPoint가 보고 있는 방향(월드 방향), targetPoint가 보고 있는 방향(로컬 기준)을 맞춰야 한다.
        // 두 문이 서로 마주보도록 회전:
        Vector3 fromForward = fromPoint.transform.forward;
        // targetPoint는 반대 방향을 봐야 하므로, 두 문의 forward가 정면 충돌하듯 반대여야 함
        // targetPoint.transform.forward를 fromForward * -1 로 맞추면 된다.
        
        // current targetPoint forward
        Vector3 targetForward = targetPoint.transform.forward;
        
        // targetPoint를 fromForward 반대 방향으로 맞추기 위해
        Quaternion rotationNeeded = Quaternion.FromToRotation(targetForward, -fromForward);
        newRoom.transform.rotation = rotationNeeded * newRoom.transform.rotation;
    }
}
