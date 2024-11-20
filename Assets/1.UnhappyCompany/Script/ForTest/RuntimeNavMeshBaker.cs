using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshSurface))]
public class RuntimeNavMeshBaker : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;

    public static event Action OnNavMeshBaked; // NavMesh ����ŷ �Ϸ� �̺�Ʈ

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame(); // �� ���� �Ϸ���� ���
        navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh(); // NavMesh ����ŷ

        // NavMesh ����ŷ �Ϸ� �˸�
        OnNavMeshBaked?.Invoke();
    }
}
