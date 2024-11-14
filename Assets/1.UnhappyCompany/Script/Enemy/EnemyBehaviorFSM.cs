using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class EnemyBehaviorFSM : MonoBehaviour
{
    [Tooltip("적이 순찰할 반경")] public float patrolRadius = 5.0f; // 순찰 반경
    [Tooltip("적의 이동 속도")] public float moveSpeed = 2.0f; // 이동 속도
    [Tooltip("플레이어를 감지하는 반경")] public float detectionRadius = 10.0f; // 플레이어 감지 반경

    private Vector3 patrolPoint;
    private GameObject player;
    private EnemyState currentState;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        // 현재 상태에 따른 행동 수행
        switch (currentState)
        {
            case EnemyState.Idle:
                // 대기 상태 로직
                ChangeState(EnemyState.Patrolling);
                break;
            case EnemyState.Patrolling:
                Patrol();
                if (player != null && Vector3.Distance(transform.position, player.transform.position) <= detectionRadius)
                {
                    ChangeState(EnemyState.Charging);
                }
                break;
            case EnemyState.Charging:
                ChargePlayer();
                break;
            case EnemyState.Dead:
                // 죽음 상태 로직
                break;
        }
    }

    public void ChangeState(EnemyState newState)
    {
        currentState = newState;

        // 상태 전환 시 초기화 작업 수행
        switch (currentState)
        {
            case EnemyState.Idle:
                // 대기 상태 초기화
                break;
            case EnemyState.Patrolling:
                SetRandomPatrolPoint();
                break;
            case EnemyState.Charging:
                // 돌진 상태 초기화
                break;
            case EnemyState.Dead:
                // 죽음 처리 로직
                Destroy(gameObject);
                break;
        }
    }

    void SetRandomPatrolPoint()
    {
        // 순찰할 무작위 지점 설정
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y; // 동일한 높이 유지
        patrolPoint = randomDirection;
    }

    void Patrol()
    {
        // 순찰 동작 수행
        transform.position = Vector3.MoveTowards(transform.position, patrolPoint, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, patrolPoint) < 0.1f)
        {
            SetRandomPatrolPoint();
        }
    }

    void ChargePlayer()
    {
        // 플레이어에게 돌진
        if (player != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * 2 * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerWeapon"))
        {
            // 플레이어 무기에 맞으면 죽음 상태로 전환
            ChangeState(EnemyState.Dead);
        }
    }
}