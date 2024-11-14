using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Spawning")]
    [Tooltip("적 생성 위치 리스트")] public List<Transform> spawnPoints; // 적 생성 위치 리스트
    [Tooltip("알 상태의 적 프리팹")] public GameObject eggPrefab; // 알 상태의 적 프리팹
    [Tooltip("성체 상태의 적 프리팹")] public GameObject adultEnemyPrefab; // 성체 상태의 적 프리팹
    [Tooltip("적이 생성될 확률 (0에서 1 사이의 값)")] public float spawnChance = 0.5f; // 적이 생성될 확률 (0에서 1 사이의 값)
    [Tooltip("알이 성체로 부화하는 데 걸리는 시간")] public float eggHatchTime = 5.0f; // 알이 성체로 부화하는 데 걸리는 시간

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        // 각 생성 위치에 대해 적을 생성할지 결정
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (Random.value <= spawnChance)
            {
                GameObject egg = Instantiate(eggPrefab, spawnPoint.position, Quaternion.identity);
                activeEnemies.Add(egg);
                StartCoroutine(HatchEgg(egg));
            }
        }
    }

    IEnumerator HatchEgg(GameObject egg)
    {
        // 일정 시간이 지난 후 알을 부화시킴
        yield return new WaitForSeconds(eggHatchTime);

        if (egg != null) // 알이 아직 존재하는지 확인
        {
            Vector3 eggPosition = egg.transform.position;
            Destroy(egg); // 알을 파괴
            GameObject adult = Instantiate(adultEnemyPrefab, eggPosition, Quaternion.identity); // 성체로 부화
            activeEnemies.Add(adult);
            EnemyBehaviorFSM enemyBehavior = adult.GetComponent<EnemyBehaviorFSM>();
            if (enemyBehavior != null)
            {
                enemyBehavior.ChangeState(EnemyState.Patrolling);
            }
        }
    }
}

public enum EnemyState
{
    Idle, // 대기 상태
    Patrolling, // 순찰 상태
    Charging, // 돌진 상태
    Dead // 죽음 상태
}