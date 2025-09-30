using UnityEngine;
using UnityEngine.AI;

// NavMeshAgent ������Ʈ�� �ʿ�� ��
[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseAIController : MonoBehaviour
{
    protected NavMeshAgent agent;      // NavMeshAgent�� ������ ����
    protected bool navMeshReady = false; // NavMesh�� �غ�Ǿ����� ����

    public Transform target; // ������ ��ǥ

    protected virtual void OnEnable()
    {
        // NavMesh ����ŷ �Ϸ� �̺�Ʈ�� �ݹ� ���
        RuntimeNavMeshBaker.OnNavMeshBaked += OnNavMeshBaked;
    }

    protected virtual void OnDisable()
    {
        // �̺�Ʈ���� �ݹ� ����
        RuntimeNavMeshBaker.OnNavMeshBaked -= OnNavMeshBaked;
    }

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // NavMeshAgent ������Ʈ ��������
    }

    protected virtual void OnNavMeshBaked()
    {
        navMeshReady = true;

        // ������Ʈ�� NavMesh ���� �̵�
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position); // ������Ʈ ��ġ ����
        }
        else
        {
            Debug.LogError("NavMesh ���� ������Ʈ�� ��ġ�� �� �����ϴ�.");
        }
    }

    protected virtual void Update()
    {
        if (navMeshReady)
        {
            CustomUpdate(); // ���� Ŭ�������� ������ �޼��� ȣ��
        }
    }

    // ���� Ŭ�������� �����ؾ� �ϴ� �߻� �޼���
    protected abstract void CustomUpdate();


}
