using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AttackState는 적이 플레이어를 공격하는 상태를 나타냅니다.
/// 공격 쿨타임을 관리하며, 공격 범위를 벗어나면 상태를 전환합니다.
/// </summary>
public class AttackState : IState
{
    private EnemyAIController controller; // EnemyAIController 참조
    private UtilityCalculator utilityCalculator; // 유틸리티 계산기 참조
    private float attackCooldown = 1.5f; // 공격 쿨타임
    private float lastAttackTime; // 마지막 공격 시간

    /// <summary>
    /// AttackState의 생성자. 상태 전환에 필요한 컨트롤러와 유틸리티 계산기를 할당합니다.
    /// </summary>
    /// <param name="controller">EnemyAIController 참조</param>
    /// <param name="calculator">UtilityCalculator 참조</param>
    public AttackState(EnemyAIController controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
    }

    /// <summary>
    /// 상태 시작 시 호출되는 메서드. 공격을 시작하며 초기 공격 시간을 설정합니다.
    /// </summary>
    public void Enter()
    {
        // 공격 시작 시 애니메이션 전환 등 필요한 로직을 여기에 추가할 수 있습니다.
        lastAttackTime = Time.time; // 현재 시간을 마지막 공격 시간으로 설정
    }

    /// <summary>
    /// 매 프레임마다 호출되는 메서드. 플레이어를 향해 회전하고 공격을 수행합니다.
    /// </summary>
    public void Execute()
    {
        // 플레이어를 향해 회전하여 바라보게 함
        Vector3 direction = (controller.player.position - controller.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, lookRotation, Time.deltaTime * 5f);

        // 공격 쿨타임이 지났는지 확인
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Attack(); // 공격 수행
            lastAttackTime = Time.time; // 마지막 공격 시간 갱신
        }

        // 플레이어와의 현재 거리 계산
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.player.position);
        if (distanceToPlayer > controller.attackRadius && distanceToPlayer < controller.chaseRadius) // 공격 거리에서 벗어났고 추적 반경 내에 있는지 확인
        {
            controller.ChangeState(new ChaseState(controller, utilityCalculator)); // 추적 상태로 전환
        }
        else if (distanceToPlayer >= controller.chaseRadius) // 추적 반경을 벗어났는지 확인
        {
            controller.ChangeState(new PatrolState(controller, utilityCalculator)); // 다시 순찰 상태로 전환
        }
    }

    /// <summary>
    /// 상태 종료 시 호출되는 메서드. 공격 종료 시 필요한 로직을 추가할 수 있습니다.
    /// </summary>
    public void Exit()
    {
        // 공격 종료 시 애니메이션 초기화 등 필요한 로직을 여기에 추가할 수 있습니다.
    }

    /// <summary>
    /// 실제 공격을 수행하는 메서드. 데미지를 주거나 이펙트를 생성할 수 있습니다.
    /// </summary>
    private void Attack()
    {
        // 실제 공격 로직 구현 예시
        Debug.Log("공격!");
        // 예: 플레이어의 체력 감소, 이펙트 재생 등 추가 구현 가능
    }
} 