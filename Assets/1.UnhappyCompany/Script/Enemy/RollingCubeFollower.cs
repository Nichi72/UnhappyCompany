using UnityEngine;



[RequireComponent(typeof(Rigidbody))]
public class RollingCubeFllower : MonoBehaviour
{
    // 상태 관련 변수
    [ReadOnly] [SerializeField] private AIState currentState = AIState.Idle;
    [ReadOnly] [SerializeField] private float stateTimer = 0f;

    // 이동 및 공격 관련 변수
    public Transform player;  // 플레이어 Transform
    public float moveForce = 10f;  // 이동 시 힘의 크기
    public float maxSpeed = 5f;  // 최대 속도
    public float restTime = 1f;  // 쉬는 시간
    public float chargeForce = 50f;  // 돌진 시 힘의 크기
    public float chargeDelay = 5f;  // 돌진 간격
    public float rotationForce = 20f;  // 회전 시 힘의 크기

    // 감지 및 공격 범위
    public float detectionRange = 15f;  // 감지 범위
    public float attackRange = 5f;  // 공격 범위

    // 순찰 관련 변수
    public Transform[] patrolPoints;  // 순찰 지점
    private int currentPatrolIndex = 0;

    // 내부 상태 관리 변수
    private Rigidbody rb;
    private bool isResting = false;
    private float restTimer = 0f;
    private bool isCharging = false;
    private float chargeTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CustomUpdate();
    }

    private void CustomUpdate()
    {
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState();
                break;
            case AIState.Patrol:
                HandlePatrolState();
                break;
            case AIState.Chase:
                HandleChaseState();
                break;
            case AIState.Attack:
                HandleAttackState();
                break;
        }
    }

    private void HandleIdleState()
    {
        // 일정 시간 후 순찰 상태로 전환
        stateTimer += Time.deltaTime;
        if (stateTimer >= 2f)
        {
            stateTimer = 0f;
            currentState = AIState.Patrol;
        }

        // 플레이어 감지 시 추적 상태로 전환
        if (IsPlayerInDetectionRange())
        {
            currentState = AIState.Chase;
        }
    }

    private void HandlePatrolState()
    {
        if (patrolPoints.Length == 0)
        {
            currentState = AIState.Idle;
            return;
        }

        if (!isResting && !isCharging)
        {
            // 현재 순찰 지점으로 이동
            MoveTowards(patrolPoints[currentPatrolIndex].position);

            // 순찰 지점에 도착하면 다음 지점으로 변경
            if (Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex].position) < 1f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                // 쉬는 시간 설정
                isResting = true;
                restTimer = restTime;
            }

            // 돌진 타이머 감소
            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0f)
            {
                isCharging = true;
                chargeTimer = chargeDelay;
            }
        }
        else if (isResting)
        {
            restTimer -= Time.deltaTime;
            if (restTimer <= 0f)
            {
                isResting = false;
            }
        }
        else if (isCharging)
        {
            // 돌진 방향 계산
            Vector3 chargeDirection = (patrolPoints[currentPatrolIndex].position - transform.position).normalized;
            rb.AddForce(chargeDirection * chargeForce, ForceMode.Impulse);
            rb.AddTorque(Random.onUnitSphere * rotationForce);
            isCharging = false;
            isResting = true;
            restTimer = restTime;
        }

        // 플레이어 감지 시 추적 상태로 전환
        if (IsPlayerInDetectionRange())
        {
            currentState = AIState.Chase;
        }
    }

    private void HandleChaseState()
    {
        if (player == null)
        {
            currentState = AIState.Patrol;
            return;
        }

        if (!isResting && !isCharging)
        {
            // 플레이어 방향으로 이동
            MoveTowards(player.position);

            // 공격 범위에 들어오면 공격 상태로 전환
            if (IsPlayerInAttackRange())
            {
                currentState = AIState.Attack;
            }

            // 돌진 타이머 감소
            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0f)
            {
                isCharging = true;
                chargeTimer = chargeDelay;
            }
        }
        else if (isResting)
        {
            restTimer -= Time.deltaTime;
            if (restTimer <= 0f)
            {
                isResting = false;
            }
        }
        else if (isCharging)
        {
            // 플레이어를 향해 돌진
            Vector3 chargeDirection = (player.position - transform.position).normalized;
            rb.AddForce(chargeDirection * chargeForce, ForceMode.Impulse);
            rb.AddTorque(Random.onUnitSphere * rotationForce);
            isCharging = false;
            isResting = true;
            restTimer = restTime;
        }

        // 플레이어를 잃어버리면 순찰 상태로 전환
        if (!IsPlayerInDetectionRange())
        {
            currentState = AIState.Patrol;
        }
    }

    private void HandleAttackState()
    {
        if (player == null)
        {
            currentState = AIState.Patrol;
            return;
        }

        // 이동 정지
        rb.linearVelocity = Vector3.zero;

        // 공격 수행
        Attack();

        // 공격 후 추적 상태로 전환
        currentState = AIState.Chase;
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        // 속도 제한
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(direction * moveForce);
        }
    }

    private void Attack()
    {
        // 플레이어를 향해 강하게 돌진
        Vector3 attackDirection = (player.position - transform.position).normalized;
        rb.AddForce(attackDirection * chargeForce, ForceMode.Impulse);
        rb.AddTorque(Random.onUnitSphere * rotationForce);
    }

    private bool IsPlayerInDetectionRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }

    private bool IsPlayerInAttackRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= attackRange;
    }

    // 디버그를 위한 기즈모 그리기
    private void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 순찰 지점 연결
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Vector3 currentPoint = patrolPoints[i].position;
                Vector3 nextPoint = patrolPoints[(i + 1) % patrolPoints.Length].position;
                Gizmos.DrawLine(currentPoint, nextPoint);
            }
        }
    }
}
