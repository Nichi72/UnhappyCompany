using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// PatrolState는 적이 순찰을 수행하는 상태를 나타냅니다.
/// 무작위 순찰 지점을 설정하고 NavMeshAgent를 통해 이동을 제어합니다.
/// </summary>
public class CubePatrolState : IState
{
    private EnemyAICube controller;
    private UtilityCalculator utilityCalculator;
    private Vector3 currentPatrolPoint;
    private bool isMovingToPoint = false;
    private float minDistanceToPoint = 1f; // 목적지 도달 판정 거리

    public CubePatrolState(EnemyAICube controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
    }

    public void Enter()
    {
        SetNewPatrolPoint(); // 초기 패트롤 포인트 설정
    }

    public void ExecuteMorning()
    {
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.player.position);
        if(distanceToPlayer < controller.AttackRadius)
        {
            controller.ChangeState(new CubeAttackState(controller, utilityCalculator));
            return;
        }
        if (!isMovingToPoint || 
            Vector3.Distance(controller.transform.position, currentPatrolPoint) < minDistanceToPoint)
        {
            SetNewPatrolPoint();
        }

        DrawPatrolGizmos();
    }

    public void ExecuteAfternoon()
    {
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.player.position);
        if(distanceToPlayer < controller.AttackRadius)
        {
            controller.ChangeState(new CubeAttackState(controller, utilityCalculator));
            return;
        }
        if (!isMovingToPoint || 
            Vector3.Distance(controller.transform.position, currentPatrolPoint) < minDistanceToPoint)
        {
            SetNewPatrolPoint();
        }

        DrawPatrolGizmos();
    }

    public void ExecuteFixedMorning()
    {
        // PatrolState에서는 FixedUpdate에서 별도의 물리 연산이 필요 없으므로 빈 메서드로 둡니다.
    }

    public void ExecuteFixedAfternoon()
    {
        // PatrolState에서는 FixedUpdate에서 별도의 물리 연산이 필요 없으므로 빈 메서드로 둡니다.
    }

    public void ExecuteFixed()
    {
        // 기본 FixedExecute는 빈 메서드로 둡니다.
    }

    private void DrawPatrolGizmos()
    {
        Vector3 directionToTarget = (currentPatrolPoint - controller.transform.position).normalized;
        Debug.DrawRay(controller.transform.position, directionToTarget * 5f, Color.red);
        Debug.DrawLine(currentPatrolPoint + Vector3.up * 0.1f, currentPatrolPoint + Vector3.up * 2f, Color.green);
        Debug.DrawLine(currentPatrolPoint - Vector3.right * 0.5f, currentPatrolPoint + Vector3.right * 0.5f, Color.green);
        Debug.DrawLine(currentPatrolPoint - Vector3.forward * 0.5f, currentPatrolPoint + Vector3.forward * 0.5f, Color.green);
    }

    private void SetNewPatrolPoint()
    {
        TimeOfDay currentTime = controller.CurrentTimeOfDay;
        Vector3 newPoint;

        if (currentTime == TimeOfDay.Morning)
        {
            newPoint = GetRandomPatrolPointMorning();
        }
        else
        {
            newPoint = GetRandomPatrolPointAfternoon();
        }

        currentPatrolPoint = newPoint;
        controller.agent.SetDestination(currentPatrolPoint);
        isMovingToPoint = true;
    }

    private Vector3 GetRandomPatrolPointMorning()
    {
        Vector3 randomDirection = Random.insideUnitSphere * (controller.PatrolRadius / 2);
        randomDirection += controller.transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, controller.PatrolRadius / 2, NavMesh.AllAreas);
        return hit.position;
    }

    private Vector3 GetRandomPatrolPointAfternoon()
    {
        Vector3 randomDirection = Random.insideUnitSphere * controller.PatrolRadius;
        randomDirection += controller.transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, controller.PatrolRadius, NavMesh.AllAreas);
        return hit.position;
    }

    public void Exit()
    {
        isMovingToPoint = false;
    }
} 