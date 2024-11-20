using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class RollingCubeManager : MonoBehaviour
{
    public Transform target; // Ÿ��(�÷��̾�)�� Transform
    //public Transform followTarget; // ����ٴ� ������Ʈ
    public RollingCubeController rollingCubeController;
    public GameObject rollingCubeControllerPrefab;
    private NavMeshAgent agent; // NavMesh ��� ����
    public List<Vector3> pathCorners; // ��� ���� ���
    public float checkRange = 1f; // ��������Ʈ ���� ���� ����
    private int pathCornersIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false; // �̵��� ���� �����ϹǷ� ��Ȱ��ȭ
        pathCorners = new List<Vector3>();
        Instantiate(rollingCubeControllerPrefab);
        StartCoroutine(CalculatePathToTarget());
    }

    private void Update()
    {
        // followTarget�� �׻� NavMeshAgent ��ġ�� ���󰡵��� ����
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
            yield return new WaitForSeconds(2f); // ��θ� 2�ʸ��� ����
        }
    }

    public IEnumerator SetPathToPointAsync(Vector3 targetPoint)
    {
        agent.enabled = true;

        // ��ǥ ������ NavMesh ���� �ִ� ���� ����� ������ ����
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(targetPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Target point is not on the NavMesh.");
            pathCorners.Clear();
            agent.enabled = false;
            yield break;
        }

        agent.SetDestination(hit.position);

        // ��� ��� ���
        yield return new WaitUntil(() => agent.pathStatus != NavMeshPathStatus.PathInvalid);

        if (agent.path.status == NavMeshPathStatus.PathComplete)
        {
            List<Vector3> rawCorners = agent.path.corners.ToList();

            // ���� ��� �߰��� ����
            List<Vector3> refinedPath = RefinePathCorners(rawCorners, 1f);

            // �ڳ� ���� �� ���� ���� �߰�
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
            return transform.position; // ��ΰ� ������ ���� ��ġ ��ȯ

        var nextPoint = pathCorners[pathCornersIndex];

        // ��������Ʈ ���� ����
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

            // �� �ڳ� �� �Ÿ� ���
            float distance = Vector3.Distance(start, end);
            int segmentCount = Mathf.CeilToInt(distance / segmentLength); // ����ȭ�� ���� ����

            // ����ȭ�� �� �߰�
            for (int j = 0; j < segmentCount; j++)
            {
                float t = j / (float)segmentCount; // ���� ��
                Vector3 intermediatePoint = Vector3.Lerp(start, end, t); // �߰��� ���
                refinedPath.Add(intermediatePoint);
            }
        }

        // ������ �ڳ� �߰�
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

            // �� �� ������ ����
            Vector3 direction = (next - current).normalized;

            // ������ ���
            if (i < corners.Count - 2)
            {
                Vector3 nextDirection = (corners[i + 2] - next).normalized;
                float angle = Vector3.Angle(direction, nextDirection);

                if (angle > 30f) // 30�� �̻��� �ڳ�
                {
                    // ���� ������ ����
                    Vector3 helperPoint = next - direction * helperDistance;
                    refinedPath.Add(helperPoint);
                }
            }
        }

        // ������ ���� �߰�
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
                Gizmos.DrawSphere(pathCorners[i], 0.2f); // �� ���� ���� ��ü ǥ��
            }
            Gizmos.DrawSphere(pathCorners[pathCorners.Count - 1], 0.2f); // ������ �� ǥ��
        }
    }
}
