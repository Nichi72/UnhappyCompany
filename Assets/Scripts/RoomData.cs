using UnityEngine;
using System.Collections.Generic;

public class RoomData : MonoBehaviour
{
    public RoomTemplate[] roomPrefabs;  // 인스펙터에서 할당할 방 프리팹들
    public int maxRooms = 10;           // 생성할 최대 방 개수
    public float roomSpacing = 10f;     // 방 사이의 간격

    private List<RoomTemplate> spawnedRooms = new List<RoomTemplate>();
    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        // 시작 방 생성
        Vector2Int currentPos = Vector2Int.zero;
        SpawnRoom(currentPos, 0);

        // 나머지 방들 생성
        int attempts = 0;
        int maxAttempts = maxRooms * 3;

        while (spawnedRooms.Count < maxRooms && attempts < maxAttempts)
        {
            attempts++;

            // 이미 생성된 방 중 하나를 무작위로 선택
            RoomTemplate currentRoom = spawnedRooms[Random.Range(0, spawnedRooms.Count)];
            
            // 가능한 방향 선택
            RoomTemplate.DoorDirection[] directions = System.Enum.GetValues(typeof(RoomTemplate.DoorDirection)) as RoomTemplate.DoorDirection[];
            RoomTemplate.DoorDirection randomDir = directions[Random.Range(0, directions.Length)];

            // 새로운 위치 계산
            Vector2Int newPos = GetNextPosition(GetRoomGridPosition(currentRoom.transform.position), randomDir);

            // 새 위치가 비어있다면 방 생성
            if (!occupiedPositions.Contains(newPos))
            {
                SpawnRoom(newPos, spawnedRooms.Count);
            }
        }
    }

    Vector2Int GetNextPosition(Vector2Int currentPos, RoomTemplate.DoorDirection direction)
    {
        switch (direction)
        {
            case RoomTemplate.DoorDirection.North:
                return currentPos + Vector2Int.up;
            case RoomTemplate.DoorDirection.South:
                return currentPos + Vector2Int.down;
            case RoomTemplate.DoorDirection.East:
                return currentPos + Vector2Int.right;
            case RoomTemplate.DoorDirection.West:
                return currentPos + Vector2Int.left;
            default:
                return currentPos;
        }
    }

    void SpawnRoom(Vector2Int gridPos, int roomIndex)
    {
        Vector3 worldPos = new Vector3(gridPos.x * roomSpacing, 0, gridPos.y * roomSpacing);
        RoomTemplate prefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        
        RoomTemplate newRoom = Instantiate(prefab, worldPos, Quaternion.identity);
        newRoom.transform.parent = transform;
        
        spawnedRooms.Add(newRoom);
        occupiedPositions.Add(gridPos);
    }

    Vector2Int GetRoomGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / roomSpacing),
            Mathf.RoundToInt(worldPosition.z / roomSpacing)
        );
    }
} 