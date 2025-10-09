using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Vector3를 직렬화 가능한 형태로 변환하는 유틸리티 클래스
/// </summary>
[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3() { }

    public SerializableVector3(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public static implicit operator Vector3(SerializableVector3 sv)
    {
        return sv.ToVector3();
    }

    public static implicit operator SerializableVector3(Vector3 v)
    {
        return new SerializableVector3(v);
    }
}

/// <summary>
/// Quaternion을 직렬화 가능한 형태로 변환하는 유틸리티 클래스
/// </summary>
[System.Serializable]
public class SerializableQuaternion
{
    public float x, y, z, w;

    public SerializableQuaternion() { }

    public SerializableQuaternion(Quaternion q)
    {
        x = q.x;
        y = q.y;
        z = q.z;
        w = q.w;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }

    public static implicit operator Quaternion(SerializableQuaternion sq)
    {
        return sq.ToQuaternion();
    }

    public static implicit operator SerializableQuaternion(Quaternion q)
    {
        return new SerializableQuaternion(q);
    }
}

/// <summary>
/// 방의 문 연결 정보를 저장하는 클래스
/// </summary>
[System.Serializable]
public class DoorConnectionData
{
    public int doorIndex;           // 현재 방의 몇 번째 문인지 (doorList 내 인덱스)
    public int connectedRoomIndex;  // 연결된 방의 인덱스 (allRoomList 내 인덱스, -1이면 연결 안됨)
    public DoorDirection direction; // 문 방향

    public DoorConnectionData()
    {
        doorIndex = -1;
        connectedRoomIndex = -1;
        direction = DoorDirection.North;
    }

    public DoorConnectionData(int doorIdx, int connectedRoomIdx, DoorDirection dir)
    {
        doorIndex = doorIdx;
        connectedRoomIndex = connectedRoomIdx;
        direction = dir;
    }
}

/// <summary>
/// 알의 저장 데이터
/// </summary>
[System.Serializable]
public class EggSpawnData
{
    public string enemyDataName;            // 적 데이터 이름 (BaseEnemyAIData의 enemyName)
    public SerializableVector3 position;    // 위치
    public SerializableQuaternion rotation; // 회전
    public EggStage currentStage;           // 현재 스테이지 (Stage1, Stage2, Stage3)
    public float realElapsedMinutes;        // 부화 진행도 (현실 경과 시간)
    public float createdRealTime;           // 생성 시간
    public int hp;                          // 체력
    public int id;                          // 알 ID

    public EggSpawnData() { }

    public EggSpawnData(Egg egg)
    {
        if (egg.enemyAIData != null)
        {
            enemyDataName = egg.enemyAIData.enemyName;
        }
        position = new SerializableVector3(egg.transform.position);
        rotation = new SerializableQuaternion(egg.transform.rotation);
        currentStage = egg.GetCurrentStage();
        realElapsedMinutes = egg.realElapsedMinutes;
        createdRealTime = egg.createdRealTime;
        hp = egg.hp;
        id = egg.id;
    }
}

/// <summary>
/// 개별 방의 저장 데이터
/// </summary>
[System.Serializable]
public class RoomSaveData
{
    // === Phase 1: 기본 방 구조 ===
    public string roomPrefabName;                       // 방 프리팹 식별자 (프리팹 이름)
    public SerializableVector3 position;                // 월드 위치
    public SerializableQuaternion rotation;             // 월드 회전
    public RoomNode.RoomType roomType;                  // 방 타입
    public int depth;                                   // 방 깊이
    public DoorDirection doorDirection;                 // 생성 방향
    public bool isOtherDirection;                       // 다른 방향으로 생성되었는지 여부
    public int connectToParentDoorIndex;                // 부모와 연결된 문의 인덱스 (-1이면 시작 방)
    public List<DoorConnectionData> doorConnections;    // 문 연결 정보

    // === Phase 2: 방 내부 상태 ===
    public List<EggSpawnData> eggs;                     // 이 방의 알들 (방별 저장)

    public RoomSaveData()
    {
        doorConnections = new List<DoorConnectionData>();
        connectToParentDoorIndex = -1; // 기본값: 연결 없음 (시작 방)
        eggs = new List<EggSpawnData>(); // Phase 2
    }

    /// <summary>
    /// RoomNode로부터 저장 데이터 생성
    /// </summary>
    public static RoomSaveData FromRoomNode(RoomNode room, int roomIndex, List<RoomNode> allRooms)
    {
        RoomSaveData data = new RoomSaveData();

        // 방 프리팹 이름 저장 (프리팹 루트의 이름 사용)
        data.roomPrefabName = room.gameObject.name.Replace("(Clone)", "").Trim();
        
        // 위치/회전 저장
        data.position = new SerializableVector3(room.transform.position);
        data.rotation = new SerializableQuaternion(room.transform.rotation);

        // 방 속성 저장
        data.roomType = room.currentRoomType;
        data.depth = room.depth;
        data.doorDirection = room.doorDirection;
        data.isOtherDirection = room.isOtherDirection;

        // RoomNode의 모든 문을 순회하며 연결 정보 저장
        // (RoomNode.InitChildRoomList()로 채워진 doorList 사용)
        var doorListField = typeof(RoomNode).GetField("doorList", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // connectToParentDoor 인덱스 저장
        data.connectToParentDoorIndex = -1; // 기본값
        
        // 문 연결 정보 저장
        data.doorConnections = new List<DoorConnectionData>();
        
        if (doorListField != null)
        {
            List<DoorEdge> doorList = doorListField.GetValue(room) as List<DoorEdge>;
            
            if (doorList != null)
            {
                for (int i = 0; i < doorList.Count; i++)
                {
                    DoorEdge door = doorList[i];
                    int connectedRoomIdx = -1;

                    // 연결된 방이 있다면 해당 방의 인덱스 찾기
                    if (door.toRoomNode != null)
                    {
                        connectedRoomIdx = allRooms.IndexOf(door.toRoomNode);
                    }

                    // connectToParentDoor 인덱스 저장
                    if (door == room.connectToParentDoor)
                    {
                        data.connectToParentDoorIndex = i;
                    }

                    DoorConnectionData doorData = new DoorConnectionData(
                        i,
                        connectedRoomIdx,
                        door.direction
                    );

                    data.doorConnections.Add(doorData);
                }
            }
        }

        return data;
    }
}

/// <summary>
/// 전체 방 시스템의 저장 데이터
/// </summary>
[System.Serializable]
public class RoomSystemSaveData
{
    public List<RoomSaveData> allRooms;     // 모든 방 데이터
    public int startRoomIndex;              // 시작 방(센터) 인덱스
    public float saveTimestamp;             // 저장 시간 (디버깅용)
    public string saveVersion;              // 저장 버전 (추후 호환성 관리용)

    public RoomSystemSaveData()
    {
        allRooms = new List<RoomSaveData>();
        startRoomIndex = -1;
        saveTimestamp = 0f;
        saveVersion = "1.0";
    }
}

/// <summary>
/// 적의 저장 데이터 (전역 저장)
/// </summary>
[System.Serializable]
public class EnemySpawnData
{
    public string enemyDataName;            // 적 데이터 이름 (BaseEnemyAIData의 enemyName)
    public SerializableVector3 position;    // 위치
    public SerializableQuaternion rotation; // 회전
    public int hp;                          // 체력

    public EnemySpawnData() { }

    public EnemySpawnData(GameObject enemyObj, BaseEnemyAIData enemyData)
    {
        if (enemyData != null)
        {
            enemyDataName = enemyData.enemyName;
        }
        position = new SerializableVector3(enemyObj.transform.position);
        rotation = new SerializableQuaternion(enemyObj.transform.rotation);
        
        // HP 가져오기 (IDamageable 인터페이스 사용)
        var damageable = enemyObj.GetComponent<IDamageable>();
        hp = damageable != null ? damageable.hp : 100;
    }
}

/// <summary>
/// 전체 적 시스템의 저장 데이터 (전역 저장)
/// </summary>
[System.Serializable]
public class EnemySystemSaveData
{
    public List<EnemySpawnData> allEnemies; // 모든 적 데이터
    public int nextEggID;                   // 다음 알 ID (EnemyManager.EggID)

    public EnemySystemSaveData()
    {
        allEnemies = new List<EnemySpawnData>();
        nextEggID = 0;
    }
}

