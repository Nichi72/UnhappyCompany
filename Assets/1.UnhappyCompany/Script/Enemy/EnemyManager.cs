using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    public int EggID = 0;


    [Header("Enemy Spawning")]
    [Tooltip("적 생성 위치 리스트")] public List<Transform> spawnPoints;
    [Tooltip("알 상태의 적 프리팹")] public GameObject eggPrefab;
    [Tooltip("생성 확률 (0부터 1 사이의 값)")] public float spawnChance = 0.5f;
    [Tooltip("알이 성체로 부화하는데 걸리는 시간")] public float eggHatchTime = 5.0f;

    [Header("SO_Enemy")]
    public List<BaseEnemyAIData> soEnemies;
    public LayerMask SpawnLayer;

    [Header("Enemy Spawning")]
    [SerializeField] public List<GameObject> activeEggs = new List<GameObject>();
    [SerializeField] public List<GameObject> activeEnemies = new List<GameObject>();
    [SerializeField] private int spawnMaxCount = 10;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnEgg()
    {
        Debug.Log("SpawnEgg");
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (Random.value <= spawnChance)
            {
                if(activeEnemies.Count >= spawnMaxCount)
                {
                    Debug.Log("적 생성 최대 수 도달");
                    return;
                }
                Init(spawnPoint);
            }
        }
    }

    private GameObject Init(Transform spawnPoint)
    {
        GameObject egg = Instantiate(eggPrefab, spawnPoint.position, Quaternion.identity);
        activeEggs.Add(egg);
        Debug.Log("Egg 생성!");
        
        Vector3 rayOrigin = egg.transform.position;
        RaycastHit hit;
        if(Physics.Raycast(rayOrigin, transform.up * -1, out hit, 10f, SpawnLayer))
        {
            egg.transform.position = hit.point;
        }

        return egg;
    }
}
