using System.Collections;
using UnityEngine;

public class RollingCubeController : MonoBehaviour
{
    public RollingCubeManager pathManager; // 경로 관리자를 참조
    public float moveForce = 10f; // 이동 시 힘의 크기
    public float maxSpeed = 5f; // 최대 속도
    public float rotationForce = 20f; // 회전 효과
    public float chargeForce = 50f; // 돌진 힘
    public float chargeCooldown = 3f; // 돌진 쿨타임
    public float stuckThreshold = 0.1f; // 정지 상태로 간주할 거리 기준
    public float stuckCheckInterval = 3f; // 정지 상태 확인 주기
    public int stuckCheckCount = 5; // 위치 비교 횟수

    private Rigidbody rb;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    public Transform centerPivot;
    private Vector3 currentDirection; // 현재 진행 방향 저장
    private Vector3[] positionHistory; // 최근 위치 기록 배열
    private int positionIndex; // 현재 기록 위치 인덱스
    public float stuckPower = 1.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        positionHistory = new Vector3[stuckCheckCount];
        positionIndex = 0;

        // 초기화 - 모든 값을 "비활성화" 상태로 설정
        for (int i = 0; i < stuckCheckCount; i++)
        {
            positionHistory[i] = Vector3.positiveInfinity; // 초기값 설정
        }
    }

    float currentTime = 0;
    [SerializeField] float restTime = 3;

    void FixedUpdate()
    {
        if (pathManager == null)
            return;

        currentTime += Time.fixedDeltaTime;
        if (restTime <= currentTime)
        {
            currentTime = 0;
            StartCoroutine(CoMove());
        }

        // 정지 상태 확인
        if (Time.frameCount % Mathf.RoundToInt(stuckCheckInterval / Time.fixedDeltaTime) == 0)
        {
            CheckForStuck();
        }

        // 돌진 로직
        chargeTimer -= Time.fixedDeltaTime;
        if (chargeTimer <= 0f && !isCharging)
        {
            //StartCoroutine(ChargeTowards(nextWaypoint));
        }
    }

    IEnumerator CoMove()
    {
        yield return pathManager.CalculatePathToTarget();
        yield return CoMoveToWayPoint();
    }


    IEnumerator CoStuckMove()
    {
        var temp = moveForce;
        moveForce = moveForce * stuckPower;
        yield return CoMoveToWayPoint();
        moveForce = temp;
    }

    IEnumerator CoMoveToWayPoint()
    {
        Vector3 nextWaypoint = pathManager.GetNextWaypoint();
        MoveToWaypoint(nextWaypoint);
        yield return new WaitForFixedUpdate();
    }

    private void MoveToWaypoint(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction = new Vector3(direction.x, 0f, direction.z);
        currentDirection = direction; // 현재 진행 방향 저장
        Debug.Log($"길찾기 방향 {direction} 지정 완료");

        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(direction * moveForce);
        }
    }

    private void CheckForStuck()
    {
        // 이전 위치를 기록
        positionHistory[positionIndex] = transform.position;
        positionIndex = (positionIndex + 1) % stuckCheckCount;

        // 위치 변화 검사
        float totalDistance = 0f;
        for (int i = 1; i < stuckCheckCount; i++)
        {
            totalDistance += Vector3.Distance(positionHistory[i - 1], positionHistory[i]);
        }

        // 일정 거리 이하로 움직였으면 멈춤 상태로 간주
        if (totalDistance / stuckCheckCount < stuckThreshold)
        {
            Debug.LogWarning("큐브가 멈췄습니다. OnStuck 호출!");
            OnStuck();
        }
    }

    private void OnStuck()
    {
        Debug.LogError("OnStuck 함수 호출됨!");
        StartCoroutine(CoStuckMove());
    }

    private IEnumerator ChargeTowards(Vector3 target)
    {
        isCharging = true;
        Vector3 chargeDirection = (target - transform.position).normalized;

        rb.AddForce(chargeDirection * chargeForce, ForceMode.Impulse);
        rb.AddTorque(Random.onUnitSphere * rotationForce);

        yield return new WaitForSeconds(1f); // 돌진 시간
        isCharging = false;
        chargeTimer = chargeCooldown;
    }

    private void OnDrawGizmos()
    {
        if (pathManager != null && currentDirection != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Vector3 currentPosition = transform.position;
            Vector3 endPosition = currentPosition + currentDirection * 2.0f;

            Gizmos.DrawLine(currentPosition, endPosition);
            Gizmos.DrawSphere(endPosition, 0.1f);
        }
    }
}
