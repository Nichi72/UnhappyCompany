using UnityEngine;
using UnityEngine.AI;

// NavMeshAgent 컴포넌트를 필요로 함
[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseAIController : MonoBehaviour
{
    protected NavMeshAgent agent;      // NavMeshAgent를 저장할 변수
    protected bool navMeshReady = false; // NavMesh가 준비되었는지 여부

    public Transform target; // 추적할 목표

    protected virtual void OnEnable()
    {
        // NavMesh 베이킹 완료 이벤트에 콜백 등록
        RuntimeNavMeshBaker.OnNavMeshBaked += OnNavMeshBaked;
    }

    protected virtual void OnDisable()
    {
        // 이벤트에서 콜백 해제
        RuntimeNavMeshBaker.OnNavMeshBaked -= OnNavMeshBaked;
    }

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // NavMeshAgent 컴포넌트 가져오기
    }

    protected virtual void OnNavMeshBaked()
    {
        navMeshReady = true;

        // 에이전트를 NavMesh 위로 이동
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position); // 에이전트 위치 설정
        }
        else
        {
            Debug.LogError("NavMesh 위에 에이전트를 배치할 수 없습니다.");
        }
    }

    protected virtual void Update()
    {
        if (navMeshReady)
        {
            CustomUpdate(); // 하위 클래스에서 구현할 메서드 호출
        }
    }

    // 하위 클래스에서 구현해야 하는 추상 메서드
    protected abstract void CustomUpdate();


}
