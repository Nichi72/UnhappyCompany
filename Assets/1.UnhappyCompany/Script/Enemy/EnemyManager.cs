using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    

    // 이벤트 선언
    public event Action<List<GameObject>> OnEggsListChanged;

    [Header("Enemy Spawning")]
    // [Tooltip("적 생성 위치 리스트")] public List<RoomS> spawnPoints;
    public List<RoomSetting> roomSettings;
    public List<RoomSettingsByDoorDirection> roomSettingsByDoorDirection;
    
    List<EggSpawnPoint> eggSpawnPoints = new List<EggSpawnPoint>();

    [Tooltip("알 상태의 적 프리팹")] public GameObject eggPrefab;
    [Tooltip("알이 성체로 부화하는데 걸리는 시간")] public float eggHatchTime = 5.0f;

    [Header("SO_Enemy")]
    public List<BaseEnemyAIData> soEnemies;
    public LayerMask SpawnLayer;

    [Header("Spawn Settings")]
    [SerializeField] private int spawnMinCount = 2;
    [SerializeField] private int spawnMaxCount = 10;
    [SerializeField] [Tooltip("알이 생성되는 비율(0부터 1 사이의 값)")] private float eggSpawnRatio = 0.2f;
    [Tooltip("방에 알이 생성 될 확률 (0부터 1 사이의 값)")] public float RoomSpawnChance = 0.5f;


    [Header("active Eggs & Enemies")]
    [SerializeField] private List<GameObject> _activeEggs = new List<GameObject>();
    [SerializeField] public List<GameObject> activeEnemies = new List<GameObject>();
    
    private int enemySpawnIndex = 0;
    [HideInInspector] public int EggID = 0;




    // activeEggs 프로퍼티
    public List<GameObject> activeEggs
    {
        get => _activeEggs;
        private set
        {
            _activeEggs = value;
            OnEggsListChanged?.Invoke(_activeEggs);
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // 이벤트 구독
            OnEggsListChanged += HandleEggsListChanged;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        if(RoomManager.Instance != null) // RoomManager는 테스트씬에서 안돌아가는 경우가 대부분임 
        {   
            RoomManager.Instance.roomGenerator.OnGenerationComplete += SpawnEggsInEachRoom;
            RoomManager.Instance.roomGenerator.OnGenerationComplete += InitializeRoomSettingsByDoorDirection;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (instance == this)
        {
            OnEggsListChanged -= HandleEggsListChanged;
        }
    }

    private void HandleEggsListChanged(List<GameObject> eggs)
    {
        Debug.Log($"알 리스트가 변경되었습니다. 현재 알 개수: {eggs.Count}");
        // 여기에 알 리스트가 변경될 때 실행하고 싶은 추가 로직을 구현하세요
    }

    public void SpawnEggsInEachRoom()
    {
        foreach (RoomSetting room in roomSettings)
        {
            // 각 방마다 생성 확률 체크
            if (UnityEngine.Random.value <= RoomSpawnChance)
            {
                // 이 방에서 생성할 알의 개수 결정 (최소~최대 사이)
                int eggsToSpawn = UnityEngine.Random.Range(spawnMinCount, spawnMaxCount + 1);
                
                // 이 방의 스폰 포인트 중 사용할 비율 계산
                int availablePoints = room.eggSpawnPoints.Count;
                int pointsToUse = Mathf.RoundToInt(availablePoints * eggSpawnRatio); 
                pointsToUse = Mathf.Max(1, pointsToUse); // 최소 1개 이상 사용
                pointsToUse = Mathf.Min(pointsToUse, eggsToSpawn); // 생성할 알 개수보다 많이 사용할 필요 없음
                
                // 스폰 포인트 랜덤하게 선택
                List<EggSpawnPoint> shuffledPoints = new List<EggSpawnPoint>(room.eggSpawnPoints);
                ShuffleList(shuffledPoints);
                
                // 선택된 포인트에 알 생성
                for (int i = 0; i < pointsToUse && i < shuffledPoints.Count; i++)
                {
                    InitEgg(shuffledPoints[i].transform);
                }
                
                Debug.Log($"{room.name}에 {pointsToUse}개의 알 생성됨");
            }
        }
    }

    // 리스트를 섞는 헬퍼 함수
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // 리스트 수정 메서드 추가
    public void AddEgg(GameObject egg)
    {
        _activeEggs.Add(egg);
        OnEggsListChanged?.Invoke(_activeEggs);
    }

    public void RemoveEgg(GameObject egg)
    {
        _activeEggs.Remove(egg);
        OnEggsListChanged?.Invoke(_activeEggs);
    }

    private GameObject InitEgg(Transform spawnPoint)
    {
        // 스폰 포인트가 속한 방 찾기
        // 부모에 RoomNode 검색을 반복
        RoomNode room = null;
        Transform current = spawnPoint;
        int maxIterations = 20; // 무한 루프 방지
        int iterations = 0;
        
        while (current != null && room == null && iterations < maxIterations)
        {
            room = current.GetComponent<RoomNode>();
            current = current.parent;
            iterations++;
        }
        
        if (iterations >= maxIterations)
        {
            Debug.LogWarning($"RoomNode 검색 중 최대 반복 횟수({maxIterations})에 도달했습니다. spawnPoint: {spawnPoint.name}");
        }

        
        Transform parentTransform = room != null ? room.transform : null;
        
        // 알 생성 (방의 자식으로)
        GameObject egg = Instantiate(eggPrefab, spawnPoint.position, Quaternion.identity, parentTransform);
        egg.GetComponent<Egg>().enemyAIData = soEnemies[UnityEngine.Random.Range(0, soEnemies.Count)];
        AddEgg(egg);  // activeEggs.Add() 대신 AddEgg() 메서드 사용
        
        // RoomSetting의 spawnedEggs 리스트에 추가
        if (room != null && room.roomSetting != null)
        {
            room.roomSetting.spawnedEggs.Add(egg);
            
            // LODGroup에서 알 렌더러 제외 (방의 LOD에 영향받지 않도록)
            LODGroup roomLOD = room.roomSetting.lodGroup;
            if (roomLOD != null)
            {
                Renderer[] eggRenderers = egg.GetComponentsInChildren<Renderer>();
                if (eggRenderers.Length > 0)
                {
                    Debug.Log($"Egg 생성! (방: {room.gameObject.name}, RoomSetting에 추가됨, LODGroup 확인됨)");
                    // LODGroup이 자동으로 알의 렌더러를 포함하지 않도록 주의 필요
                }
            }
            else
            {
                Debug.Log($"Egg 생성! (방: {room.gameObject.name}, RoomSetting에 추가됨)");
            }
        }
        else if (room != null)
        {
            Debug.LogWarning($"Egg 생성! (방: {room.gameObject.name}, 경고: RoomSetting을 찾을 수 없음)");
        }
        else
        {
            Debug.LogWarning("Egg 생성! (경고: 방을 찾을 수 없어 부모 없이 생성됨)");
        }
        
        Vector3 rayOrigin = egg.transform.position;
        RaycastHit hit;
        if(Physics.Raycast(rayOrigin, transform.up * -1, out hit, 10f, SpawnLayer))
        {
            egg.transform.position = hit.point;
        }

        return egg;
    }

    public void RandomSpawnEnemy()
    {
        int randomIndex = UnityEngine.Random.Range(0, roomSettings.Count);
        SpawnEnemy(roomSettings[randomIndex].eggSpawnPoints[UnityEngine.Random.Range(0, roomSettings[randomIndex].eggSpawnPoints.Count)].transform);
    }

    public void SpawnEnemy(Transform spawnPoint)
    {
        Debug.Log("SpawnEnemy");
        var enemyData = soEnemies[UnityEngine.Random.Range(0, soEnemies.Count)];

        GameObject enemy = Instantiate(enemyData.prefab, spawnPoint.position, Quaternion.identity);
        
        // EnemyAIController 컴포넌트 확인
        var controller = enemy.GetComponent<EnemyAIController>();
        if (controller == null)
        {
            Debug.LogError($"생성된 적 {enemy.name}에 EnemyAIController 컴포넌트가 없습니다!");
            Destroy(enemy);
            return;
        }
        enemy.name = $"{enemyData.enemyName}_{enemySpawnIndex}";
        activeEnemies.Add(enemy);
        enemySpawnIndex++;
    }

    /// <summary>
    /// DoorDirection별로 RoomSetting을 분류하여 roomSettingsByDoorDirection 리스트를 초기화합니다.
    /// </summary>
    public void InitializeRoomSettingsByDoorDirection()
    {
        roomSettingsByDoorDirection = new List<RoomSettingsByDoorDirection>();
        
        // DoorDirection 열거형의 모든 값에 대해 RoomSettingsByDoorDirection 객체 생성
        System.Array doorDirections = System.Enum.GetValues(typeof(DoorDirection));
        
        foreach (DoorDirection direction in doorDirections)
        {
            RoomSettingsByDoorDirection roomSettingsByDirection = new RoomSettingsByDoorDirection
            {
                doorDirection = direction,
                roomSettings = new List<RoomSetting>()
            };
            roomSettingsByDoorDirection.Add(roomSettingsByDirection);
        }
        
        // RoomManager에서 생성된 모든 RoomNode를 가져와서 DoorDirection별로 분류
        if (RoomManager.Instance != null && RoomManager.Instance.roomGenerator != null)
        {
            List<RoomNode> allRooms = RoomManager.Instance.roomGenerator.allRoomList;
            
            foreach (RoomNode roomNode in allRooms)
            {
                if (roomNode.roomSetting != null)
                {
                    // 해당 DoorDirection에 맞는 RoomSettingsByDoorDirection 찾기
                    RoomSettingsByDoorDirection targetGroup = roomSettingsByDoorDirection.Find(
                        group => group.doorDirection == roomNode.doorDirection
                    );
                    
                    if (targetGroup != null)
                    {
                        targetGroup.roomSettings.Add(roomNode.roomSetting);
                    }
                }
            }
            
            // 결과 로그 출력
            foreach (var group in roomSettingsByDoorDirection)
            {
                Debug.Log($"{group.doorDirection} 방향: {group.roomSettings.Count}개의 방");
            }
        }
        else
        {
            Debug.LogWarning("RoomManager 또는 RoomGenerator가 null입니다. roomSettingsByDoorDirection 초기화를 완료할 수 없습니다.");
        }
    }

    public void CheatEggHatchIntoEnemy()
    {
        foreach(var egg in activeEggs)
        {
            egg.GetComponent<Egg>().HatchIntoEnemy();
        }
    }
}

[System.Serializable]
public class RoomSettingsByDoorDirection
{
    public DoorDirection doorDirection;
    public List<RoomSetting> roomSettings;
}