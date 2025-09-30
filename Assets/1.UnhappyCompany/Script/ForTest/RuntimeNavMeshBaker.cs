using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshSurface))]
public class RuntimeNavMeshBaker : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;

    public static event Action OnNavMeshBaked; // NavMesh 베이킹 완료 이벤트

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame(); // 맵 생성 완료까지 대기
        navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh(); // NavMesh 베이킹

        // NavMesh 베이킹 완료 알림
        OnNavMeshBaked?.Invoke();
    }
}
