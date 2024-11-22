using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class RollingCubeManager : MonoBehaviour
{
    public Transform target;
    public RollingCubeController rollingCubeController;
    public GameObject rollingCubeControllerPrefab;
    private NavMeshAgent agent;
    public List<Vector3> pathCorners;
    public float checkRange = 1f;
    private int pathCornersIndex = 0;

    public enum PathCalculationMethod
    {
        Default,
        Smoothing,
        Simplification,
        Custom
    }

    public PathCalculationMethod calculationMethod = PathCalculationMethod.Default;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        pathCorners = new List<Vector3>();
        InitRollingCubeControllerPrefab();
        //StartCoroutine(CalculatePathToTarget());
    }

    private void InitRollingCubeControllerPrefab()
    {
        var temp = Instantiate(rollingCubeControllerPrefab);
        rollingCubeController = temp.GetComponent<RollingCubeController>();
        rollingCubeController.pathManager = this;
        rollingCubeController.transform.position = transform.position;

        target = GameManager.instance.currentPlayer.transform;
    }

    private void Update()
    {
        if (rollingCubeController != null)
        {
            transform.position = rollingCubeController.centerPivot.transform.position;
        }
    }

    public IEnumerator CalculatePathToTarget()
    {
        while (true)
        {
            if (target != null)
            {
                yield return SetPathToPointAsync(target.position);
                Debug.Log("길찾기 계산 완료");
                break;
            }
        }
    }

    public IEnumerator SetPathToPointAsync(Vector3 targetPoint)
    {
        agent.enabled = true;

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(targetPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Target point is not on the NavMesh.");
            pathCorners.Clear();
            agent.enabled = false;
            yield break;
        }

        agent.SetDestination(hit.position);

        yield return new WaitUntil(() => agent.pathStatus != NavMeshPathStatus.PathInvalid);

        if (agent.path.status == NavMeshPathStatus.PathComplete)
        {
            List<Vector3> rawCorners = agent.path.corners.ToList();
            switch (calculationMethod)
            {
                case PathCalculationMethod.Default:
                    pathCorners = rawCorners;
                    break;

                case PathCalculationMethod.Smoothing:
                    pathCorners = RefinePathCorners(rawCorners, 1f);
                    break;

                case PathCalculationMethod.Simplification:
                    pathCorners = SimplifyPath(rawCorners, 0.5f);
                    break;

                case PathCalculationMethod.Custom:
                    pathCorners = CustomPathCalculation(rawCorners);
                    break;
            }
            pathCornersIndex = 0;
        }
        else
        {
            Debug.LogWarning("Path calculation failed.");
            pathCorners.Clear();
            // 나 -> 플레이어 
            Vector3 temp = GameManager.instance.currentPlayer.transform.position - transform.position;
            temp.Normalize();
            pathCorners.Add(temp);
            
        }

        agent.enabled = false;
    }

    public Vector3 GetNextWaypoint()
    {
        if (pathCorners == null || pathCorners.Count == 0 || pathCornersIndex >= pathCorners.Count)
            return transform.position;

        var nextPoint = pathCorners[pathCornersIndex];
        if (Vector3.Distance(nextPoint, transform.position) <= checkRange)
        {
            pathCornersIndex++;
        }

        return pathCornersIndex < pathCorners.Count ? pathCorners[pathCornersIndex] : nextPoint;
    }

    private List<Vector3> SimplifyPath(List<Vector3> corners, float tolerance)
    {
        // Ramer-Douglas-Peucker 알고리즘 기반 단순화
        if (corners.Count <= 2) return corners;

        List<Vector3> simplifiedPath = new List<Vector3>();
        simplifiedPath.Add(corners[0]);

        for (int i = 1; i < corners.Count - 1; i++)
        {
            Vector3 prev = corners[i - 1];
            Vector3 current = corners[i];
            Vector3 next = corners[i + 1];

            Vector3 direction1 = (current - prev).normalized;
            Vector3 direction2 = (next - current).normalized;

            if (Vector3.Distance(direction1, direction2) > tolerance)
            {
                simplifiedPath.Add(current);
            }
        }

        simplifiedPath.Add(corners[corners.Count - 1]);
        return simplifiedPath;
    }

    private List<Vector3> CustomPathCalculation(List<Vector3> corners)
    {
        List<Vector3> customPath = new List<Vector3>();

        foreach (var corner in corners)
        {
            // 예: 특정 높이 값을 변경
            customPath.Add(new Vector3(corner.x, corner.y + 0.5f, corner.z));
        }

        return customPath;
    }

    private List<Vector3> RefinePathCorners(List<Vector3> corners, float segmentLength = 1f)
    {
        List<Vector3> refinedPath = new List<Vector3>();

        for (int i = 0; i < corners.Count - 1; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[i + 1];

            float distance = Vector3.Distance(start, end);
            int segmentCount = Mathf.CeilToInt(distance / segmentLength);

            for (int j = 0; j < segmentCount; j++)
            {
                float t = j / (float)segmentCount;
                Vector3 intermediatePoint = Vector3.Lerp(start, end, t);
                refinedPath.Add(intermediatePoint);
            }
        }

        if (corners.Count > 0)
        {
            refinedPath.Add(corners[corners.Count - 1]);
        }

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
                Gizmos.DrawSphere(pathCorners[i], 0.2f);
            }
            Gizmos.DrawSphere(pathCorners[pathCorners.Count - 1], 0.2f);
        }
    }
}
