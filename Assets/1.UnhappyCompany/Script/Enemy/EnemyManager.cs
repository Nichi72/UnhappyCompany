using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    public int EggID = 0;

    // 이벤트 선언
    public event Action<List<GameObject>> OnEggsListChanged;

    [Header("Enemy Spawning")]
    [Tooltip("적 생성 위치 리스트")] public List<Transform> spawnPoints;
    [Tooltip("알 상태의 적 프리팹")] public GameObject eggPrefab;
    [Tooltip("생성 확률 (0부터 1 사이의 값)")] public float spawnChance = 0.5f;
    [Tooltip("알이 성체로 부화하는데 걸리는 시간")] public float eggHatchTime = 5.0f;

    [Header("SO_Enemy")]
    public List<BaseEnemyAIData> soEnemies;
    public LayerMask SpawnLayer;

    [Header("Enemy Spawning")]
    [SerializeField] private List<GameObject> _activeEggs = new List<GameObject>();
    [SerializeField] public List<GameObject> activeEnemies = new List<GameObject>();
    [SerializeField] private int spawnMaxCount = 10;
    private int enemySpawnIndex = 0;

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

    public void SpawnEgg()
    {
        Debug.Log("SpawnEgg");
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (UnityEngine.Random.value <= spawnChance)
            {
                if(activeEnemies.Count >= spawnMaxCount)
                {
                    Debug.Log("적 생성 최대 수 도달");
                    return;
                }
                InitEgg(spawnPoint);
            }
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
        GameObject egg = Instantiate(eggPrefab, spawnPoint.position, Quaternion.identity);
        AddEgg(egg);  // activeEggs.Add() 대신 AddEgg() 메서드 사용
        Debug.Log("Egg 생성!");
        
        Vector3 rayOrigin = egg.transform.position;
        RaycastHit hit;
        if(Physics.Raycast(rayOrigin, transform.up * -1, out hit, 10f, SpawnLayer))
        {
            egg.transform.position = hit.point;
        }

        return egg;
    }

    public void SpawnEnemy()
    {
        Debug.Log("SpawnEnemy");
        var enemyData = soEnemies[UnityEngine.Random.Range(0, soEnemies.Count)];

        GameObject enemy = Instantiate(enemyData.prefab, spawnPoints[0].position, Quaternion.identity);
        
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
}
