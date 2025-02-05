using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    public Transform target; // ��ǥ ����

    private NavMeshAgent agent;
    private bool navMeshReady = false;

    void OnEnable()
    {
        RuntimeNavMeshBaker.OnNavMeshBaked += OnNavMeshBaked;
    }

    void OnDisable()
    {
        RuntimeNavMeshBaker.OnNavMeshBaked -= OnNavMeshBaked;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void OnNavMeshBaked()
    {
        navMeshReady = true;

        // ������Ʈ�� NavMesh ���� �̵�
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position); // ������Ʈ�� NavMesh ���� �̵�
        }
        else
        {
            Debug.LogError("NavMesh ���� ������Ʈ�� ��ġ�� �� �����ϴ�.");
        }

        if (target != null)
        {
            agent.SetDestination(target.position); // ��ǥ �������� �̵� ����
        }
    }

    void Update()
    {
        if (navMeshReady && target != null)
        {
            agent.SetDestination(target.position); // ���������� ��ǥ ������Ʈ
        }
    }
}
