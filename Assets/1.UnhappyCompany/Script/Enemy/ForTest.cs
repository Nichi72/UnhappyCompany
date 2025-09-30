using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForTest : MonoBehaviour
{
    public Transform target;  // 큐브가 이동할 목표 지점
    public float torqueForce = 50f;  // 큐브에 가할 회전 토크의 크기
    public float maxAngularVelocity = 10f; // 큐브의 최대 각속도
    private Rigidbody rb;
    public float waitTime = 1.5f;
    [SerializeField] private List<GameObject> others;
    [SerializeField] List<FixedJoint> fixedJoints;

    private Vector3 currentTorqueDirection; // 현재 큐브에 적용되고 있는 토크 방향 저장

    void Start()
    {
        // Rigidbody 컴포넌트 가져오기
        rb = GetComponent<Rigidbody>();

        // 최대 각속도 설정 (기본값보다 높여 자연스럽게 회전 이동 가능하도록)
        rb.maxAngularVelocity = maxAngularVelocity;
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.V))
        {
            if (target != null)
            {
                MoveTowardsTarget();
            }
        }
    }

    public IEnumerator CoUpdate()
    {
        int count = 0;
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            count++;
            MoveTowardsTarget();
            if (3 < count)
            {
                count = 0;
                MoveTowardsTarget();
            }
        }
    }

    void MoveTowardsTarget()
    {
        // 목표 지점까지의 방향 계산 (월드 좌표계 기준)
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // 월드 좌표계에서 큐브의 현재 진행 방향
        Vector3 currentForward = transform.forward;

        // 목표 방향과 현재 진행 방향의 차이를 바탕으로 회전 벡터 계산
        currentTorqueDirection = Vector3.Cross(currentForward, directionToTarget);

        // 큐브에 토크를 가해 굴러가게 하기
        rb.AddTorque(currentTorqueDirection * torqueForce, ForceMode.Force);
    }

    // Gizmos를 사용하여 방향 벡터 시각화
    void OnDrawGizmos()
    {
        if (target != null)
        {
            // 큐브에서 목표 지점까지의 방향
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position, 0.5f);

            // 현재 목표 지점을 향한 방향 벡터
            Gizmos.color = Color.blue;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            Gizmos.DrawRay(transform.position, directionToTarget * 2f);

            // 현재 적용되고 있는 회전 토크 벡터
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, currentTorqueDirection.normalized * 2f);
        }
    }

    GameObject GetFarthestTransform(Transform player, List<GameObject> targets)
    {
        GameObject farthestTarget = null;
        float farthestDistanceSqr = 0f;

        foreach (GameObject target in targets)
        {
            float distanceSqr = (target.transform.position - player.position).sqrMagnitude;
            if (distanceSqr > farthestDistanceSqr)
            {
                farthestDistanceSqr = distanceSqr;
                farthestTarget = target;
            }
        }

        return farthestTarget;
    }
}
