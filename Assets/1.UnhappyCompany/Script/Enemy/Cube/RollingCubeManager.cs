using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class RollingCubeManager : MonoBehaviour
{
    public Transform target; // 타겟(플레이어)의 Transform
    //public Transform followTarget; // 따라다닐 오브젝트
    public RollingCubeController rollingCubeController;
    public GameObject rollingCubeControllerPrefab;
    private NavMeshAgent agent; // NavMesh 경로 계산용
    public List<Vector3> pathCorners; // 경로 지점 목록
    public float checkRange = 1f; // 웨이포인트 도달 판정 범위
    private int pathCornersIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false; // 이동은 직접 제어하므로 비활성화
        pathCorners = new List<Vector3>();
        Instantiate(rollingCubeControllerPrefab);
        StartCoroutine(CalculatePathToTarget());
    }

    private void Update()
    {
        // followTarget이 항상 NavMeshAgent 위치를 따라가도록 유지
        if (rollingCubeController != null)
        {
            transform.position = rollingCubeController.transform.position;
        }
    }

    IEnumerator CalculatePathToTarget()
    {
        while (true)
        {
            if (target != null)
            {
                yield return SetPathToPointAsync(target.position);
            }
            yield return new WaitForSeconds(2f); // 경로를 2초마다 재계산
        }
    }

    public IEnumerator SetPathToPointAsync(Vector3 targetPoint)
    {
        agent.enabled = true;

        // 목표 지점을 NavMesh 위에 있는 가장 가까운 점으로 보정
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(targetPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Target point is not on the NavMesh.");
            pathCorners.Clear();
            agent.enabled = false;
            yield break;
        }

        agent.SetDestination(hit.position);

        // 경로 계산 대기
        yield return new WaitUntil(() => agent.pathStatus != NavMeshPathStatus.PathInvalid);

        if (agent.path.status == NavMeshPathStatus.PathComplete)
        {
            List<Vector3> rawCorners = agent.path.corners.ToList();

            // 간격 기반 중간점 생성
            List<Vector3> refinedPath = RefinePathCorners(rawCorners, 1f);

            // 코너 감지 및 보조 지점 추가
            pathCorners = AddHelperPoints(refinedPath, 1f);
            pathCornersIndex = 0;
        }
        else
        {
            Debug.LogWarning("Path calculation failed.");
            pathCorners.Clear();
        }

        agent.enabled = false;
    }

    public Vector3 GetNextWaypoint()
    {
        if (pathCorners == null || pathCorners.Count == 0 || pathCornersIndex >= pathCorners.Count)
            return transform.position; // 경로가 없으면 현재 위치 반환

        var nextPoint = pathCorners[pathCornersIndex];

        // 웨이포인트 도달 판정
        if (Vector3.Distance(nextPoint, transform.position) <= checkRange)
        {
            pathCornersIndex++;
        }

        return pathCornersIndex < pathCorners.Count ? pathCorners[pathCornersIndex] : nextPoint;
    }

    private List<Vector3> RefinePathCorners(List<Vector3> corners, float segmentLength = 1f)
    {
        List<Vector3> refinedPath = new List<Vector3>();

        for (int i = 0; i < corners.Count - 1; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[i + 1];

            // 두 코너 간 거리 계산
            float distance = Vector3.Distance(start, end);
            int segmentCount = Mathf.CeilToInt(distance / segmentLength); // 세분화된 구간 개수

            // 세분화된 점 추가
            for (int j = 0; j < segmentCount; j++)
            {
                float t = j / (float)segmentCount; // 보간 값
                Vector3 intermediatePoint = Vector3.Lerp(start, end, t); // 중간점 계산
                refinedPath.Add(intermediatePoint);
            }
        }

        // 마지막 코너 추가
        if (corners.Count > 0)
        {
            refinedPath.Add(corners[corners.Count - 1]);
        }

        return refinedPath;
    }

    private List<Vector3> AddHelperPoints(List<Vector3> corners, float helperDistance = 1f)
    {
        List<Vector3> refinedPath = new List<Vector3>();

        for (int i = 0; i < corners.Count - 1; i++)
        {
            Vector3 current = corners[i];
            Vector3 next = corners[i + 1];

            refinedPath.Add(current);

            // 두 점 사이의 벡터
            Vector3 direction = (next - current).normalized;

            // 각도를 계산
            if (i < corners.Count - 2)
            {
                Vector3 nextDirection = (corners[i + 2] - next).normalized;
                float angle = Vector3.Angle(direction, nextDirection);

                if (angle > 30f) // 30도 이상의 코너
                {
                    // 보조 지점을 생성
                    Vector3 helperPoint = next - direction * helperDistance;
                    refinedPath.Add(helperPoint);
                }
            }
        }

        // 마지막 지점 추가
        refinedPath.Add(corners[corners.Count - 1]);

        return refinedPath;
    }

    private void OnDrawGizmos()
    {
        if (pathCorners != null && pathCorners.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < pathCorners.Count - 1; i++)
            {
                Gizmos.DrawLine(pathCorners[i], pathCorners[i + 1]);
                Gizmos.DrawSphere(pathCorners[i], 0.2f); // 각 점에 작은 구체 표시
            }
            Gizmos.DrawSphere(pathCorners[pathCorners.Count - 1], 0.2f); // 마지막 점 표시
        }
    }
}
