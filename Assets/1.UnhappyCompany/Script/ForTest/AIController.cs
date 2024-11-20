using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    public Transform target; // 목표 지점

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

        // 에이전트를 NavMesh 위로 이동
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position); // 에이전트를 NavMesh 위로 이동
        }
        else
        {
            Debug.LogError("NavMesh 위에 에이전트를 배치할 수 없습니다.");
        }

        if (target != null)
        {
            agent.SetDestination(target.position); // 목표 지점으로 이동 시작
        }
    }

    void Update()
    {
        if (navMeshReady && target != null)
        {
            agent.SetDestination(target.position); // 지속적으로 목표 업데이트
        }
    }
}
